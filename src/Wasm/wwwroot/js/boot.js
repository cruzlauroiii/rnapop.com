// Detect app root from current URL and start Blazor with correct base path
(function () {
    var path = location.pathname;
    var base = path.substring(0, path.lastIndexOf("/") + 1);

    var el = document.createElement("base");
    el.href = base;
    document.head.insertBefore(el, document.head.firstChild);

    Blazor.start();
})();
