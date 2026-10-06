// wwwroot/js/swal-delete.js

(function () {
    // Helper: find closest ancestor matching selector
    function closest(el, selector) {
        while (el && el.nodeType === 1) {
            if (el.matches(selector)) return el;
            el = el.parentElement;
        }
        return null;
    }

    // Helper: get anti-forgery token value (from current form or global hidden form)
    function getAntiForgeryToken(sourceForm) {
        // 1) prefer the token inside the same form (if it's a form submit)
        if (sourceForm) {
            var input = sourceForm.querySelector('input[name="__RequestVerificationToken"]');
            if (input && input.value) return input.value;
        }
        // 2) fallback to global hidden form token
        var globalForm = document.getElementById('__global-af-token');
        if (globalForm) {
            var globalInput = globalForm.querySelector('input[name="__RequestVerificationToken"]');
            if (globalInput && globalInput.value) return globalInput.value;
        }
        return null;
    }

    // Core confirm dialog (SweetAlert2). Returns a Promise<boolean>
    function confirmDelete(message, title) {
        const lang = window.Resources.lang;
        console.log(lang);

        if (window.Swal && Swal.fire) {
            if (lang.startsWith("ar")) {
                
                return Swal.fire({
                    title: `هل انت متأكد من حذف هذا العنصر؟`,
                    text: message || '',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: '<i class="fas fa-trash mx-1"></i>  نعم حذف',
                    confirmButtonColor: '#47a744',
                    cancelButtonText: '<i class="fas fa-times mx-1"></i>  الغاء',
                    reverseButtons: true,
                    focusCancel: true
                }).then(r => !!r.isConfirmed);
            } else {
                return Swal.fire({
                    title: `Are you sure this item has been deleted?`,
                    text: message || '',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: '<i class="fas fa-trash mx-1"></i>  Yes, delete',
                    confirmButtonColor: '#47a744',
                    cancelButtonText: '<i class="fas fa-times mx-1"></i>  cancel',
                    reverseButtons: true,
                    focusCancel: true
                }).then(r => !!r.isConfirmed);
            }
            
        } else {
            // graceful fallback if SweetAlert2 not loaded
            return Promise.resolve(window.confirm(title || message || 'Delete?'));
        }
    }

    // (A) Intercept DELETE FORMS: <form method="post" data-swal="delete">...</form>
    document.addEventListener('submit', function (e) {
        var form = closest(e.target, 'form[data-swal="delete"]');
        if (!form) return;

        // Build message from attributes if provided
        var itemName = form.getAttribute('data-name') || 'this item';
        var msg = form.getAttribute('data-message');

        // Stop immediate submit, confirm first
        e.preventDefault();
        e.stopPropagation();

        confirmDelete(msg).then(function (ok) {
            if (!ok) return;

            // If the form already has antiforgery (usual case), just submit
            // That way everything (enctype, files, etc.) behaves normally.
            form.submit();
        });
    }, true);

    // (B) Intercept DELETE LINKS (works for GET or POST patterns)
    // Usage examples:
    // 1) Simple GET:  <a href="/Items/Delete/5" data-swal="delete" data-name="Item #5">Delete</a>
    // 2) Force POST:  <a href="/Items/Delete/5" data-swal="delete" data-method="post" data-name="Item #5">Delete</a>
    //    (will auto-build a hidden POST form with anti-forgery token)
    document.addEventListener('click', function (e) {
        var link = closest(e.target, '[data-swal="delete"]');
        if (!link || link.tagName !== 'A') return;

        e.preventDefault();

        var itemName = link.getAttribute('data-name') || 'this item';
        var msg = link.getAttribute('data-message') || `هل انت متأكد من حذف ${itemName}.`;
        var method = (link.getAttribute('data-method') || 'get').toLowerCase();
        var href = link.getAttribute('href');

        confirmDelete(msg).then(function (ok) {
            if (!ok) return;

            if (method === 'get' || !method) {
                // Navigate (matches routes that delete via GET—only if your app permits)
                window.location.href = href;
            } else if (method === 'post') {
                // Create and submit a transient POST form with anti-forgery
                var form = document.createElement('form');
                form.method = 'post';
                form.action = href;
                form.style.display = 'none';

                // Anti-forgery input (required by ASP.NET Core when [ValidateAntiForgeryToken] is used)
                var tokenVal = getAntiForgeryToken(null);
                if (tokenVal) {
                    var tokenInput = document.createElement('input');
                    tokenInput.type = 'hidden';
                    tokenInput.name = '__RequestVerificationToken';
                    tokenInput.value = tokenVal;
                    form.appendChild(tokenInput);
                }

                document.body.appendChild(form);
                form.submit();
            }
        });
    }, false);

})();
