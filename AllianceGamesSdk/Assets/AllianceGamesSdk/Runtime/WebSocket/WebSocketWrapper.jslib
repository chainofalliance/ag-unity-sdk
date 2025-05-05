var WebSocketWrapper = {
    $webSocketMap: {},

    Connect: function (
        urlPointer,
        onOpenCallback,
        onMessageCallback,
        onErrorCallback,
        onCloseCallback
    ) {
        const url = UTF8ToString(urlPointer);
        if (webSocketMap[url]) {
            if (onErrorCallback) {
                var msg = "WebSocket already opened.";
                var msgBytes = lengthBytesUTF8(msg);
                var msgBuffer = _malloc(msgBytes + 1);
                stringToUTF8(msg, msgBuffer, msgBytes);

                try {
                    Module.dynCall_vii(onErrorCallback, urlPointer, msgBuffer)
                } finally {
                    _free(msgBuffer);
                }
            }
            return;
        }

        const ws = new WebSocket(url);
        ws.binaryType = 'arraybuffer';

        ws.onopen = function () {
            if (onOpenCallback) {
                var len = lengthBytesUTF8(url) + 1;
                var strPtr = _malloc(len);
                stringToUTF8(url, strPtr, len);

                try {
                    Module.dynCall_vi(onOpenCallback, strPtr);
                } finally {
                    _free(strPtr);
                }
            }
        };

        ws.onmessage = function (ev) {
            if (!onMessageCallback) {
                return;
            }

            if (ev.data instanceof ArrayBuffer) {
                var dataBuffer = new Uint8Array(ev.data);

                var buffer = _malloc(dataBuffer.length);
                HEAPU8.set(dataBuffer, buffer);

                var len = lengthBytesUTF8(url) + 1;
                var strPtr = _malloc(len);
                stringToUTF8(url, strPtr, len);

                try {
                    Module.dynCall_viii(onMessageCallback, strPtr, buffer, dataBuffer.length);
                } finally {
                    _free(buffer);
                    _free(strPtr);
                }
            }
        };

        ws.onerror = function (ev) {
            if (onErrorCallback) {
                var msg = "WebSocket error.";
                var msgBytes = lengthBytesUTF8(msg);
                var msgBuffer = _malloc(msgBytes + 1);
                stringToUTF8(msg, msgBuffer, msgBytes);

                var len = lengthBytesUTF8(url) + 1;
                var strPtr = _malloc(len);
                stringToUTF8(url, strPtr, len);

                try {
                    Module.dynCall_vii(onErrorCallback, strPtr, msgBuffer)
                } finally {
                    _free(msgBuffer);
                    _free(strPtr);
                }
            }
        };

        ws.onclose = function (ev) {
            if (onCloseCallback) {
                var len = lengthBytesUTF8(url) + 1;
                var strPtr = _malloc(len);
                stringToUTF8(url, strPtr, len);
                try {
                    Module.dynCall_vii(onCloseCallback, strPtr, ev.code);
                } finally {
                    _free(strPtr);
                }
            }
        };

        webSocketMap[url] = ws;
    },

    Close: function (urlPointer, code, reasonPointer) {
        const url = UTF8ToString(urlPointer);
        const ws = webSocketMap[url];
        if (!ws) return -3;
        if (ws.readyState === 2) return -4;
        if (ws.readyState === 3) return -5;

        var reason = (reasonPointer ? UTF8ToString(reasonPointer) : undefined);

        try {
            ws.close(code, reason);
        } catch (err) {
            return -7;
        }
    },

    Send: function (urlPointer, bufferPtr, offset, count) {
        const url = UTF8ToString(urlPointer);
        const ws = webSocketMap[url];
        if (!ws) return -3;
        if (ws.readyState !== 1) return -6;

        ws.send(HEAPU8.buffer.slice(bufferPtr + offset, bufferPtr + count - offset));
    },

    GetState: function (urlPointer) {
        const url = UTF8ToString(urlPointer);
        const ws = webSocketMap[url];
        if (!ws) return 3;
        return ws.readyState;
    }
}

autoAddDeps(WebSocketWrapper, '$webSocketMap');
mergeInto(LibraryManager.library, WebSocketWrapper);