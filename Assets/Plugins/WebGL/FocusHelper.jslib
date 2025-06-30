mergeInto(LibraryManager.library, {
    InitAutoCanvasFocus: function () {
        document.addEventListener("fullscreenchange", function () {
            var canvas = document.getElementById("unity-canvas");
            if (document.fullscreenElement && canvas) {
                canvas.focus();
            }
        });
    }
});