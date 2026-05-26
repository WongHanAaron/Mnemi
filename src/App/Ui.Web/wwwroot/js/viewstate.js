// ViewState detection and resize handling for Mnemi web app

let resizeTimeout;
let resizeHandler = null;

window.addOnResizeHandler = function (dotNetHelper, initialWidth) {
    // Store the DotNet reference
    resizeHandler = dotNetHelper;

    // Debounced resize handler
    window.addEventListener('resize', function () {
        if (resizeTimeout) {
            clearTimeout(resizeTimeout);
        }

        resizeTimeout = setTimeout(function () {
            const width = window.innerWidth;
            if (resizeHandler) {
                resizeHandler.invokeMethod('OnResize', width);
            }
        }, 100); // 100ms debounce
    });
};

window.removeOnResizeHandler = function () {
    if (resizeHandler) {
        resizeHandler.dispose();
        resizeHandler = null;
    }

    // Note: We can't remove the event listener without keeping a reference
    // This is acceptable as the listener is lightweight
};

window.getViewState = function () {
    const width = window.innerWidth;
    if (width < 768) return 'Phone';
    if (width < 1024) return 'Tablet';
    return 'Desktop';
};
