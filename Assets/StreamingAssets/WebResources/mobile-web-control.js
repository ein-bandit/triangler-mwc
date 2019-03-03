const serverAddress = window.location.hostname + ":" + config.serverPort;

const sendMode = "string"; //"bytes"

const errorElement = document.getElementById("error");
const dataElement = document.getElementById("data");
const contentElement = document.getElementById("content");
const connectElement = document.getElementById("connect");
var connectBtn = connectElement.getElementsByClassName("connect-btn")[0];

var connecting = false;
var gameStarted = false;

var mobileWebControl = {
  sendFunction: function() {},
  connectClient: function() {
    if (!connecting) {
      connecting = true;
      connectBtn.classList.add("disabled");
      setTimeout(() => {
        connectBtn.innerHTML = "connecting...";
      }, 300);

      connectTapAnimation();
      connect(
        serverAddress,
        setupDataChannelAndListeners
      );
    }
  }
};

function setupDataChannelAndListeners() {
  createLocalDataChannel("message-data-channel", {
    initialize: function() {
      gamelogic.changeState("ready");
    },
    message: function(message) {
      if (message.type === "command") {
        gamelogic.executeCommand(message.data);
      } else if (message.type === "change_state") {
        gamelogic.changeState(message.data);
      } else {
        console.error("invalid data retrieved", message);
      }
    },
    error: function(error) {
      console.error("DataChannel error event received", error);
    },
    close: function() {
      removeListeners();
      gamelogic.changeState("connect");
    }
  });

  //setup listeners for enabled features.
  mobileWebControl.sendFunction = function(data, isCommand) {
    if (
      isCommand ||
      (gameStarted === true && dataChannel.readyState === "open")
    ) {
      if (debug) {
        console.log("sending: " + data.type + ", " + JSON.stringify(data.data));
        dataElement.innerHTML = JSON.stringify(data);
      }

      //HINT: if your webrtc library supports multiple dataChannels you can use seperate channels for each data type.

      if (sendMode === "byte") {
        dataChannel.send(convertToBytes(data));
      } else {
        dataChannel.send(JSON.stringify(data));
      }
    }
  };

  enabledFeatures.forEach(featureName => {
    features[featureName].registration(true);
  });
  //end setupDataChannelAndListeners
}

function convertToBytes(data) {
  //convert object to byte[];
  return data.toByteArray();
}

function removeListeners() {
  enabledFeatures.forEach(featureName => {
    features[featureName].registration(false);
  });
}

function connectTapAnimation() {
  connectBtn.classList.add("tapped");
  setTimeout(() => {
    connectBtn.classList.remove("tapped");
  }, 250);
}
