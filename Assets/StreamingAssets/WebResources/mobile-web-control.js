const serverPort = "1234";
const serverAddress = window.location.hostname + ":" + serverPort;

const sendMode = "string"; //"byte"

const errorElement = document.getElementById("error");
const dataElement = document.getElementById("data");
const contentElement = document.getElementById("content");
const connectElement = document.getElementById("connect");
const readyElement = document.getElementById("ready");
var connectBtn = connectElement.getElementsByClassName("connect-btn")[0];

var connecting = false;

var gameStarted = false;

function connectClient() {
  //rtc_main.js
  connectTapAnimation();
  if (!connecting) {
    connecting = true;
    connect(
      serverAddress,
      setupDataChannelAndListeners
    );
  }
}

function updateScene(mode) {
  if (mode === "game") {
    readyElement.classList.add("hidden");
    contentElement.classList.remove("hidden");
  } else if (mode === "ready") {
    connectElement.classList.add("hidden");
    readyElement.classList.remove("hidden");

    if (debug) {
      dataElement.classList.remove("hidden");
    }
  } else if (mode === "connect") {
    connecting = false; //disconnect occurred.
    connectElement.classList.remove("hidden");
    contentElement.classList.add("hidden");
    readyElement.classList.add("hidden");

    if (debug) {
      dataElement.classList.add("hidden");
    }
  }
}
function setupDataChannelAndListeners() {
  updateScene("ready");
  createLocalDataChannel("message-data-channel", {
    message: function(message) {
      if (event.data === "start") {
        updateScene("game");
      }
    },
    error: function(error) {
      console.error("DataChannel error event received", error);
    },
    close: function() {
      removeListeners();
      updateScene("connect");
    }
  });

  //setup listeners for enabled features.
  enabledFeatures.forEach(featureName => {
    //TODO: reaftor to dataChannels["all"] ??
    features[featureName].listener(function(data) {
      if (dataChannel.readyState != "open") {
        return;
      }
      if (debug) {
        console.log("sending: " + data.type + ", " + JSON.stringify(data.data));
        dataElement.innerHTML = JSON.stringify(data);
      }

      //TODO: if your webrtc library supports multiple dataChannels use it for each data type.

      if (sendMode === "byte") {
        dataChannel.send(convertToBytes(data));
      } else {
        dataChannel.send(JSON.stringify(data));
      }
    });
  });
}

function convertToBytes(data) {
  //convert object to byte[];
  return data.toByteArray();
}

function removeListeners() {
  enabledFeatures.forEach(featureName => {
    //find a way to remove listener.
  });
}

function connectTapAnimation() {
  connectBtn.classList.add("tapped");
  setTimeout(() => {
    connectBtn.classList.remove("tapped");
  }, 250);
}

function handleReadyClick() {
  readyElement.classList.add("disabled");

  dataChannel.send(JSON.stringify({ type: "ready", data: "ready" }));
}
