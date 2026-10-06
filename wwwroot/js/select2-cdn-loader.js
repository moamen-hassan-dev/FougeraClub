// CDN Select2 loader for quick integration
// This file is created to load select2 from CDN if not present in the project
// You can remove this if you add select2 locally

(function () {
    var link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = 'https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css';
    document.head.appendChild(link);

    var script = document.createElement('script');
    script.src = 'https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js';
    document.body.appendChild(script);
})();
