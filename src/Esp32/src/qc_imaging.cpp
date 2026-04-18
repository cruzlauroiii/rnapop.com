/*
 * ============================================================
 * ESP32 #4 — QC & Imaging Controller
 * ============================================================
 *
 * Handles quality control instruments and gel imaging.
 *
 * Equipment:
 *   - Gel Imager: ESP32-CAM (OV2640), blue LED transilluminator, orange filter servo
 *   - Dynamic Light Scattering (simplified): 650nm laser + photodetector at 90°
 *   - Turbidity / OD sensor: LED + dual photodetectors (sample + reference)
 *
 * Communication: WebSocket server on port 9125
 *   Sends images as base64 JPEG over WebSocket.
 *   Sends DLS autocorrelation data + calculated particle size.
 *   Sends turbidity / OD readings.
 */

#include <Arduino.h>
#include <WiFi.h>
#include <WebSocketsServer.h>
#include <ArduinoJson.h>
#include <ESP32Servo.h>

#include "config.h"
#include "config_multi.h"

// Optional: ESP32-CAM specific
// If using ESP32-S3 with external OV2640 via DVP interface:
#if defined(CAMERA_MODEL_ESP32S3_EYE) || defined(BOARD_HAS_PSRAM)
#include "esp_camera.h"
#define HAS_CAMERA 1
#else
#define HAS_CAMERA 0
#endif

WebSocketsServer ws(QC_WSS_PORT);
Servo filterServo;

// ===== State =====
struct QCState {
    // Gel Imager
    bool transilluminatorOn = false;
    int transilluminatorBrightness = 100;   // 0-255
    int filterPosition = 0;                  // 0=open, 90=orange filter in place
    bool imageReady = false;
    int imageSize = 0;

    // DLS (Dynamic Light Scattering)
    bool dlsRunning = false;
    float dlsParticleSizeNm = 0;       // Z-average diameter
    float dlsPDI = 0;                   // Polydispersity index (0-1, lower = more uniform)
    bool dlsReady = false;

    // Turbidity
    float turbidityOD = 0;              // Optical density (absorbance)
    float turbidityTransmission = 0;    // % transmission
    bool turbidityReady = false;
} state;

unsigned long lastBroadcast = 0;

// ===== Gel Imager =====

void setTransilluminator(bool on, int brightness) {
    state.transilluminatorOn = on;
    state.transilluminatorBrightness = brightness;
    if (on) {
        analogWrite(PIN_TRANSILLUM_PWM, brightness);
        digitalWrite(PIN_TRANSILLUM_LED, HIGH);
    } else {
        analogWrite(PIN_TRANSILLUM_PWM, 0);
        digitalWrite(PIN_TRANSILLUM_LED, LOW);
    }
}

void setOrangeFilter(bool engaged) {
    state.filterPosition = engaged ? 90 : 0;
    filterServo.write(state.filterPosition);
    delay(300);  // Wait for servo to reach position
}

#if HAS_CAMERA
void initCamera() {
    camera_config_t config;
    config.ledc_channel = LEDC_CHANNEL_0;
    config.ledc_timer = LEDC_TIMER_0;
    config.pin_d0 = 11;  // Adjust for your ESP32-S3 camera board
    config.pin_d1 = 9;
    config.pin_d2 = 8;
    config.pin_d3 = 10;
    config.pin_d4 = 12;
    config.pin_d5 = 18;
    config.pin_d6 = 17;
    config.pin_d7 = 16;
    config.pin_xclk = 15;
    config.pin_pclk = 13;
    config.pin_vsync = 6;
    config.pin_href = 7;
    config.pin_sccb_sda = 4;
    config.pin_sccb_scl = 5;
    config.pin_pwdn = -1;
    config.pin_reset = -1;
    config.xclk_freq_hz = 20000000;
    config.frame_size = FRAMESIZE_SXGA;     // 1280x1024 for gel imaging
    config.pixel_format = PIXFORMAT_JPEG;
    config.grab_mode = CAMERA_GRAB_LATEST;
    config.fb_location = CAMERA_FB_IN_PSRAM;
    config.jpeg_quality = 10;               // Low number = high quality
    config.fb_count = 2;

    esp_err_t err = esp_camera_init(&config);
    if (err != ESP_OK) {
        Serial.printf("Camera init failed: 0x%x\n", err);
    } else {
        Serial.println("Camera initialized (OV2640 SXGA).");
    }
}

// Capture gel image and send over WebSocket as base64 JPEG
void captureAndSendImage(uint8_t clientNum) {
    // Ensure transilluminator is on and filter is in place
    setTransilluminator(true, 200);
    setOrangeFilter(true);
    delay(500);  // Settle time for LEDs

    camera_fb_t* fb = esp_camera_fb_get();
    if (!fb) {
        Serial.println("Camera capture failed!");
        return;
    }

    state.imageSize = fb->len;
    state.imageReady = true;

    Serial.printf("Captured image: %d bytes, %dx%d\n", fb->len, fb->width, fb->height);

    // Send as binary WebSocket frame (JPEG bytes)
    ws.sendBIN(clientNum, fb->buf, fb->len);

    esp_camera_fb_return(fb);
}
#else
void initCamera() {
    Serial.println("No camera module — gel imaging in simulation mode.");
}
void captureAndSendImage(uint8_t clientNum) {
    Serial.println("Camera not available. Use RPi + camera module for gel imaging.");
    // Inform client that camera hardware is not present on this board
    ws.sendTXT(clientNum, "{\"error\":\"No camera module on this ESP32. Use ESP32-S3-EYE or ESP32-CAM board.\"}");
}
#endif

// ===== Dynamic Light Scattering (DLS) =====
//
// Principle: Shine 650nm laser into LNP sample. Scattered light fluctuates
// due to Brownian motion of nanoparticles. Faster fluctuation = smaller particles.
// Autocorrelation of intensity vs time → decay rate → diffusion coefficient → size.
//
// Stokes-Einstein: d = kT / (3πηD)
//   d = hydrodynamic diameter (m)
//   k = Boltzmann constant (1.38e-23 J/K)
//   T = temperature (K)
//   η = viscosity (Pa·s), water at 25°C = 0.89e-3
//   D = diffusion coefficient (m²/s)
//
// This is a simplified single-angle (90°) DLS. Real instruments use
// multiple angles and regularization (CONTIN algorithm).

#define DLS_N_SAMPLES   50000   // Number of ADC samples
#define DLS_TAU_STEPS   100     // Autocorrelation lag steps

float dlsIntensity[DLS_N_SAMPLES];  // Stored in PSRAM if available
float dlsAutoCorr[DLS_TAU_STEPS];

void measureDLS() {
    state.dlsRunning = true;
    state.dlsReady = false;

    // Turn on laser
    digitalWrite(PIN_DLS_LASER, HIGH);
    delay(1000);  // Laser warm-up

    Serial.println("DLS: Sampling scattered light intensity...");

    // Sample scattered light at high rate
    unsigned long sampleInterval = 1000000 / DLS_SAMPLE_RATE_HZ;  // microseconds
    unsigned long startUs = micros();

    for (int i = 0; i < DLS_N_SAMPLES; i++) {
        dlsIntensity[i] = (float)analogRead(PIN_DLS_DETECTOR);
        // Tight timing loop
        while (micros() - startUs < (unsigned long)(i + 1) * sampleInterval) {
            // Spin-wait for precise timing
        }
    }

    digitalWrite(PIN_DLS_LASER, LOW);

    Serial.println("DLS: Computing autocorrelation...");

    // Compute mean intensity
    float mean = 0;
    for (int i = 0; i < DLS_N_SAMPLES; i++) mean += dlsIntensity[i];
    mean /= DLS_N_SAMPLES;

    // Compute variance
    float variance = 0;
    for (int i = 0; i < DLS_N_SAMPLES; i++) {
        float diff = dlsIntensity[i] - mean;
        variance += diff * diff;
    }
    variance /= DLS_N_SAMPLES;

    if (variance < 1.0) {
        Serial.println("DLS: Insufficient scattering signal. Check sample.");
        state.dlsRunning = false;
        return;
    }

    // Compute normalized autocorrelation G(τ) = <I(t) * I(t+τ)> / <I>²
    for (int tau = 0; tau < DLS_TAU_STEPS; tau++) {
        float sum = 0;
        int count = DLS_N_SAMPLES - tau * (DLS_N_SAMPLES / DLS_TAU_STEPS);
        int step = tau * (DLS_N_SAMPLES / DLS_TAU_STEPS);

        for (int i = 0; i < count && (i + step) < DLS_N_SAMPLES; i++) {
            sum += (dlsIntensity[i] - mean) * (dlsIntensity[i + step] - mean);
        }
        dlsAutoCorr[tau] = sum / (count * variance);
    }

    // Fit single exponential: G(τ) = A * exp(-2Γτ) + B
    // Γ = D * q²
    // q = (4πn/λ) * sin(θ/2)
    // For 90° scattering, θ=90°, n=1.33 (water), λ=650nm:
    // q = (4π * 1.33 / 650e-9) * sin(45°) = 1.82e7 m⁻¹

    const float q = 1.82e7;            // Scattering vector (m⁻¹)
    const float kB = 1.38e-23;         // Boltzmann constant (J/K)
    const float T = 298.15;            // Temperature (K, ~25°C)
    const float eta = 0.89e-3;         // Water viscosity (Pa·s)
    const float sampleRate = (float)DLS_SAMPLE_RATE_HZ;

    // Find decay rate from first 20 points of autocorrelation (initial decay)
    // ln(G(τ)) = ln(A) - 2Γτ → linear fit of ln(G) vs τ
    float sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
    int fitPoints = 0;

    for (int i = 1; i < 20 && i < DLS_TAU_STEPS; i++) {
        if (dlsAutoCorr[i] <= 0.01) break;  // Below noise floor

        float tau_sec = (float)i * (DLS_N_SAMPLES / DLS_TAU_STEPS) / sampleRate;
        float lnG = log(dlsAutoCorr[i]);

        sumX += tau_sec;
        sumY += lnG;
        sumXY += tau_sec * lnG;
        sumX2 += tau_sec * tau_sec;
        fitPoints++;
    }

    if (fitPoints < 5) {
        Serial.println("DLS: Insufficient autocorrelation data. Check sample concentration.");
        state.dlsRunning = false;
        return;
    }

    // Linear regression: slope = -2Γ
    float slope = (fitPoints * sumXY - sumX * sumY) / (fitPoints * sumX2 - sumX * sumX);
    float gamma = -slope / 2.0;  // Decay rate

    if (gamma <= 0) {
        Serial.println("DLS: Invalid decay rate. Sample may be too concentrated.");
        state.dlsRunning = false;
        return;
    }

    // Diffusion coefficient: D = Γ / q²
    float D = gamma / (q * q);

    // Stokes-Einstein: diameter = kT / (3πηD)
    float diameter_m = kB * T / (3.0 * M_PI * eta * D);
    float diameter_nm = diameter_m * 1e9;

    // Polydispersity Index (simplified: from residuals of single-exp fit)
    // PDI = (variance of decay rates) / (mean decay rate)²
    // For single-exponential fit, approximate from curvature of ln(G) plot
    float intercept = (sumY - slope * sumX) / fitPoints;
    float residualSum = 0;
    for (int i = 1; i < fitPoints && i < 20; i++) {
        float tau_sec = (float)i * (DLS_N_SAMPLES / DLS_TAU_STEPS) / sampleRate;
        float predicted = intercept + slope * tau_sec;
        float actual = log(max(0.01f, dlsAutoCorr[i]));
        float residual = actual - predicted;
        residualSum += residual * residual;
    }
    float pdi = residualSum / fitPoints;  // Simplified PDI estimate
    pdi = constrain(pdi, 0.0, 1.0);

    state.dlsParticleSizeNm = constrain(diameter_nm, 1.0, 10000.0);
    state.dlsPDI = pdi;
    state.dlsReady = true;
    state.dlsRunning = false;

    Serial.printf("DLS: Z-average = %.1f nm, PDI = %.3f\n", state.dlsParticleSizeNm, state.dlsPDI);
    Serial.printf("DLS: Γ = %.1f s⁻¹, D = %.2e m²/s\n", gamma, D);

    // Interpretation
    if (diameter_nm >= 60 && diameter_nm <= 100) {
        Serial.println("DLS: Particle size OPTIMAL for LNP (60-100nm)");
    } else if (diameter_nm < 60) {
        Serial.println("DLS: Particles smaller than expected — increase lipid:mRNA ratio or reduce flow rate");
    } else if (diameter_nm <= 200) {
        Serial.println("DLS: Particles larger than optimal — increase flow rate for smaller LNPs");
    } else {
        Serial.println("DLS: WARNING — particles too large (>200nm), may indicate aggregation");
    }
}

// ===== Turbidity / OD Sensor =====

void measureTurbidity() {
    // Turn on LED, wait for settle
    digitalWrite(PIN_TURBIDITY_LED, HIGH);
    delay(200);

    // Read sample and reference photodetectors (average 64 readings)
    long sampleSum = 0, refSum = 0;
    for (int i = 0; i < 64; i++) {
        sampleSum += analogRead(PIN_TURBIDITY_DET);
        refSum += analogRead(PIN_TURBIDITY_REF);
        delayMicroseconds(100);
    }
    float sampleReading = sampleSum / 64.0;
    float refReading = refSum / 64.0;

    digitalWrite(PIN_TURBIDITY_LED, LOW);

    // Optical density: OD = -log10(I_sample / I_reference)
    if (refReading > 10) {
        state.turbidityOD = -log10(max(1.0f, sampleReading) / refReading);
        state.turbidityTransmission = (sampleReading / refReading) * 100.0;
    } else {
        state.turbidityOD = 0;
        state.turbidityTransmission = 0;
    }

    state.turbidityOD = constrain(state.turbidityOD, 0.0f, 4.0f);
    state.turbidityTransmission = constrain(state.turbidityTransmission, 0.0f, 100.0f);
    state.turbidityReady = true;

    Serial.printf("Turbidity: OD=%.3f, %%T=%.1f\n", state.turbidityOD, state.turbidityTransmission);
}

// ===== WebSocket Handler =====
void onWsEvent(uint8_t num, WStype_t type, uint8_t* payload, size_t length) {
    if (type == WStype_CONNECTED) {
        Serial.printf("QC WS client %u connected\n", num);
        return;
    }
    if (type != WStype_TEXT) return;

    JsonDocument doc;
    if (deserializeJson(doc, payload, length)) return;

    int cmd = doc["command"].as<int>();
    int val = doc["value"].as<int>();

    // Commands: 30=capture image, 31=transilluminator on, 32=transilluminator off,
    //           33=set brightness, 34=filter engage, 35=filter disengage,
    //           36=DLS measure, 37=turbidity measure
    switch (cmd) {
        case 30:  // Capture gel image
            captureAndSendImage(num);
            break;
        case 31:  // Transilluminator on
            setTransilluminator(true, state.transilluminatorBrightness);
            break;
        case 32:  // Transilluminator off
            setTransilluminator(false, 0);
            break;
        case 33:  // Set brightness
            state.transilluminatorBrightness = constrain(val, 0, 255);
            if (state.transilluminatorOn) {
                analogWrite(PIN_TRANSILLUM_PWM, state.transilluminatorBrightness);
            }
            break;
        case 34:  // Orange filter engage
            setOrangeFilter(true);
            break;
        case 35:  // Orange filter disengage
            setOrangeFilter(false);
            break;
        case 36:  // DLS measurement
            measureDLS();
            break;
        case 37:  // Turbidity measurement
            measureTurbidity();
            break;
    }
}

void broadcastStatus() {
    if (ws.connectedClients() == 0) return;

    JsonDocument doc;

    auto img = doc["gelImager"].to<JsonObject>();
    img["transilluminatorOn"] = state.transilluminatorOn;
    img["brightness"] = state.transilluminatorBrightness;
    img["filterEngaged"] = state.filterPosition > 45;
    img["imageReady"] = state.imageReady;
    img["imageSize"] = state.imageSize;

    auto dls = doc["dls"].to<JsonObject>();
    dls["running"] = state.dlsRunning;
    dls["particleSizeNm"] = round(state.dlsParticleSizeNm * 10) / 10.0;
    dls["pdi"] = round(state.dlsPDI * 1000) / 1000.0;
    dls["ready"] = state.dlsReady;
    dls["sizeOk"] = state.dlsReady && state.dlsParticleSizeNm >= 60 && state.dlsParticleSizeNm <= 100;

    auto turb = doc["turbidity"].to<JsonObject>();
    turb["od"] = round(state.turbidityOD * 1000) / 1000.0;
    turb["transmission"] = round(state.turbidityTransmission * 10) / 10.0;
    turb["ready"] = state.turbidityReady;

    doc["connected"] = true;

    String json;
    serializeJson(doc, json);
    ws.broadcastTXT(json);
}

// ===== Setup =====
void setup() {
    Serial.begin(115200);
    Serial.println("\n====================================");
    Serial.println("ESP32 #4 — QC & Imaging");
    Serial.println("====================================\n");

    // Transilluminator
    pinMode(PIN_TRANSILLUM_LED, OUTPUT);
    pinMode(PIN_TRANSILLUM_PWM, OUTPUT);
    digitalWrite(PIN_TRANSILLUM_LED, LOW);
    analogWrite(PIN_TRANSILLUM_PWM, 0);

    // Orange filter servo
    filterServo.attach(PIN_ORANGE_FILTER);
    filterServo.write(0);  // Open position

    // DLS laser
    pinMode(PIN_DLS_LASER, OUTPUT);
    digitalWrite(PIN_DLS_LASER, LOW);

    // Turbidity LED
    pinMode(PIN_TURBIDITY_LED, OUTPUT);
    digitalWrite(PIN_TURBIDITY_LED, LOW);

    // ADC
    analogReadResolution(12);

    // Camera
    initCamera();

    // Status LED
    pinMode(PIN_QC_STATUS_LED, OUTPUT);

    // WiFi
    WiFi.mode(WIFI_STA);
    WiFi.begin(WIFI_SSID, WIFI_PASS);
    Serial.print("WiFi connecting");
    while (WiFi.status() != WL_CONNECTED) { delay(500); Serial.print("."); }
    Serial.printf("\nIP: %s\n", WiFi.localIP().toString().c_str());
    digitalWrite(PIN_QC_STATUS_LED, HIGH);

    // WebSocket
    ws.begin();
    ws.onEvent(onWsEvent);
    Serial.printf("QC WebSocket on port %d\n", QC_WSS_PORT);

    Serial.println("QC & Imaging controller ready.\n");
}

// ===== Loop =====
void loop() {
    ws.loop();

    if (millis() - lastBroadcast >= 500) {
        lastBroadcast = millis();
        broadcastStatus();
    }
}
