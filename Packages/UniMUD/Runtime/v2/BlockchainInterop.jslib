mergeInto(LibraryManager.library, {
  _websocket: null,
  _onMessageCallback: null,
  _onErrorCallback: null,
  _onCloseCallback: null,
  _onOpenCallback: null,

  BlockchainInterop_Connect: function (rpcUrl) {
    if (typeof WebSocket === "undefined") {
      console.error("WebSockets are not supported by this browser.");
      return;
    }

    if (Module._websocket) {
      Module._websocket.close();
    }

    Module._websocket = new WebSocket(UTF8ToString(rpcUrl));

    Module._websocket.onmessage = function (event) {
      if (Module._onMessageCallback) {
        var payload = Module.stringToUTF8(event.data);
        Runtime.dynCall("vi", Module._onMessageCallback, [payload]);
      }
    };

    Module._websocket.onerror = function (event) {
      console.error("WebSocket error occurred. Retrying in 5 seconds...");
      setTimeout(function () {
        BlockchainInterop_Connect(rpcUrl); // Retry connecting after a delay
      }, 5000);
    };

    Module._websocket.onclose = function (event) {
      console.warn("WebSocket connection closed. Retrying in 5 seconds...");
      setTimeout(function () {
        BlockchainInterop_Connect(rpcUrl); // Retry connecting after a delay
      }, 5000);
    };

    Module.OnBlockchainConnected = Module.cwrap(
      "OnBlockchainConnected",
      null,
      []
    );

    Module._websocket.onopen = function (event) {
      console.log("WebSocket connected successfully.");
      // if (Module._onOpenCallback) {
      // Runtime.dynCall("v", Module._onOpenCallback);
      // }

      Module.OnBlockchainConnected();

      // Call the C# function to trigger the subscription
      //   Module.BlockchainInterop_Subscribe("newHeads");
    };
  },

  BlockchainInterop_Subscribe: function (type) {
    const typeStr = UTF8ToString(type); // Convert the input from a pointer to a JavaScript string
    console.log("Subscribing to:", typeStr); // Log for debugging

    const subscriptionRequest = {
      jsonrpc: "2.0",
      id: 1,
      method: "eth_subscribe",
      params: [typeStr],
    };

    if (Module._websocket) {
      if (Module._websocket.readyState === WebSocket.OPEN) {
        Module._websocket.send(JSON.stringify(subscriptionRequest));
        console.log(
          "Subscription request sent:",
          JSON.stringify(subscriptionRequest)
        ); // Log for debugging
      } else {
        console.error(
          "WebSocket is not in OPEN state. Current state:",
          Module._websocket.readyState
        );
      }
    } else {
      console.error("WebSocket is not initialized.");
    }
  },

  BlockchainInterop_SetOnMessageCallback: function (callback) {
    Module._onMessageCallback = callback;
  },

  BlockchainInterop_SetOnErrorCallback: function (callback) {
    Module._onErrorCallback = callback;
  },

  BlockchainInterop_SetOnCloseCallback: function (callback) {
    Module._onCloseCallback = callback;
  },

  BlockchainInterop_SetOnOpenCallback: function (callback) {
    Module._onOpenCallback = callback;
  },

  BlockchainInterop_Close: function () {
    if (Module._websocket) {
      Module._websocket.close();
      Module._websocket = null;
    }
  },
});
