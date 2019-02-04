const serverPort = "1234";
const serverAddress = window.location.hostname + ":" + serverPort;

const sendMode = "string"; //"byte"

const errorElement = document.getElementById("error");
const dataElement = document.getElementById("data");
const connectElement = document.getElementById("connect");
const contentElement = document.getElementById("content");

function connectClient() {
  //rtc_main.js
  connect(
    serverAddress,
    setupDataChannelAndListeners
  );
}

function updateScene(mode) {
  if (mode === "game") {
    connectElement.classList.add("hidden");
    contentElement.classList.remove("hidden");

    if (debug) {
      dataElement.classList.remove("hidden");
    }
  } else if (mode === "connect") {
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
        console.log("sending: " + data.type, data);
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
