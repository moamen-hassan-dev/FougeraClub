

//side menu

document.addEventListener('DOMContentLoaded', function () {
    const menuBtn = document.getElementById('menu-icon');
    const sidebar = document.querySelector('.sidebar');

    // Make sure the side bar starts hidden
    sidebar.classList.add('hide');

    menuBtn.addEventListener('click', function () {
        if (sidebar.classList.contains('show')) {
            sidebar.classList.remove('show');
            sidebar.classList.add('hide');
            document.body.style.overflow = 'auto'; // Re-scroll after disappearing
            document.documentElement.style.overflow = 'auto'; // Re-scroll after disappearing
        } else {
            sidebar.classList.remove('hide');
            sidebar.classList.add('show');
            document.body.style.overflow = 'hidden'; // Prevent scrolling while Sidebar appears
            document.documentElement.style.overflow = 'hidden'; // Prevent scrolling while Sidebar appears
        }
    });
});


