const serverPort = "1234";
const serverAddress = window.location.hostname + ":" + serverPort;

const sendMode = "string"; //"byte"

const errorElement = document.getElementById("error");
const dataElement = document.getElementById("data");
const contentElement = document.getElementById("content");
const connectElement = document.getElementById("connect");
var connectBtn = connectElement.getElementsByClassName("connect-btn")[0];

var connecting = false;

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
    connectElement.classList.add("hidden");
    contentElement.classList.remove("hidden");

    if (debug) {
      dataElement.classList.remove("hidden");
    }
  } else if (mode === "connect") {
    connecting = false; //disconnect occurred.
    connectElement.classList.remove("hidden");
    contentElement.classList.add("hidden");

    if (debug) {
      dataElement.classList.add("hidden");
    }
  }
}
function setupDataChannelAndListeners() {
  updateScene("game");
  createLocalDataChannel();

  //setup listeners for enabled features.
  enabledFeatures.forEach(featureName => {
    //TODO: reaftor to dataChannels["all"] ??
    features[featureName].listener(function(data) {
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
