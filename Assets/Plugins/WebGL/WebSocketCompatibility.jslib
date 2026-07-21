mergeInto(LibraryManager.library, {
    WebSocketEnsureCompatibility: function() {
        if (typeof Blob !== 'undefined') {
            return;
        }

        // UnityWebSocket checks `ev.data instanceof Blob` before handling text.
        // Weixin Mini Game only supplies string/ArrayBuffer messages and has no Blob global.
        var UnsupportedBlob = function WebSocketUnsupportedBlob() {};
        if (typeof globalThis !== 'undefined') {
            globalThis.Blob = UnsupportedBlob;
        }
        if (typeof window !== 'undefined') {
            window.Blob = UnsupportedBlob;
        }
        if (typeof GameGlobal !== 'undefined') {
            GameGlobal.Blob = UnsupportedBlob;
        }
    }
});
