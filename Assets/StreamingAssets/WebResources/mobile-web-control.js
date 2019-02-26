const serverPort = "1234";
const serverAddress = window.location.hostname + ":" + serverPort;

const sendMode = "string"; //"byte"

const errorElement = document.getElementById("error");
const dataElement = document.getElementById("data");
const contentElement = document.getElementById("content");
const connectElement = document.getElementById("connect");
const readyElement = document.getElementById("ready");
var connectBtn = connectElement.getElementsByClassName("connect-btn")[0];
var readyBtn = readyElement.getElementsByClassName("ready-btn")[0];

var connecting = false;

var gameStarted = false;

function connectClient() {
  if (!connecting) {
    connecting = true;
    connectTapAnimation();
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
    gameStarted = true;
  } else if (mode === "ready") {
    connectElement.classList.add("hidden");
    readyElement.classList.remove("hidden");

    readyBtn.innerHTML = "ready";

    readyElement.classList.remove("disabled");

    if (debug) {
      dataElement.classList.remove("hidden");
    }
  } else if (mode === "connect") {
    connecting = false; //disconnect occurred.
    gameStarted = false;
    connectElement.classList.remove("hidden");
    contentElement.classList.add("hidden");
    readyElement.classList.add("hidden");

    connectBtn.innerHTML = "connect";

    if (debug) {
      dataElement.classList.add("hidden");
    }
  }
}
function setupDataChannelAndListeners() {
  connectBtn.innerHTML = "connected";

  createLocalDataChannel("message-data-channel", {
    message: function(message) {
      if (message === "ready") {
        updateScene("ready");
      } else if (message === "game_start") {
        updateScene("game");
      } else if (message == "hit") {
        //put this to a own method inside features?
        setTimeout(() => {
          navigator.vibrate(75);
        }, 100);
        setTimeout(() => {
          navigator.vibrate(75);
        }, 200);
        setTimeout(() => {
          navigator.vibrate(75);
        }, 300);
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
    features[featureName].listener(function(data) {
      if (gameStarted === true && dataChannel.readyState === "open") {
        if (debug) {
          console.log(
            "sending: " + data.type + ", " + JSON.stringify(data.data)
          );
          dataElement.innerHTML = JSON.stringify(data);
        }

        //HINT: if your webrtc library supports multiple dataChannels you can use seperate channels for each data type.

        if (sendMode === "byte") {
          dataChannel.send(convertToBytes(data));
        } else {
          dataChannel.send(JSON.stringify(data));
        }
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

  readyBtn.innerHTML = "waiting";

  dataChannel.send(JSON.stringify({ type: "ready", data: "ready" }));
}

function activateBoostLocal() {
  navigator.vibrate(500);
}
