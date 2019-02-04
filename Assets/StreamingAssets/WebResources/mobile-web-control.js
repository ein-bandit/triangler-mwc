const serverPort = "1234";
const serverAddress = window.location.hostname + ":" + serverPort;

var services = {
  deviceorientation: { enabled: false },
  tapDetection: { enabled: true, id: "tap-area" },
  ambientLight: { enabled: false },
  deviceProximity: { enabled: true },
  deviceMotion: { enabled: true }
};

const errorElement = document.getElementById("error");
const dataElement = document.getElementById("data");
const connectElement = document.getElementById("connect");

function initWebRTCConnection() {
  //rtc_main.js
  connect(
    serverAddress,
    setupDataChannelAndListeners
  );

  connectElement.classList.add("hidden");
  dataElement.classList.remove("hidden");
}

function setupDataChannelAndListeners() {
  console.log("services:", services.deviceorientation);
  createLocalDataChannel();

  if (services.deviceorientation.enabled) {
    addDeviceOrientationListener();
  }
  if (services.tapDetection.enabled) {
    addTapDetectionListener();
  }
  if (services.ambientLight.enabled) {
    addAmbientLightListener();
  }
  if (services.deviceProximity.enabled) {
    addDeviceProximityListener();
  }
  if (services.deviceMotion.enabled) {
    addDeviceMotionListener();
  }
}

function addDeviceOrientationListener() {
  if (window.DeviceOrientationEvent) {
    window.addEventListener(
      "deviceorientation",
      function(evt) {
        var alpha = Math.floor(evt.alpha);
        var beta = Math.floor(evt.beta);
        var gamma = Math.floor(evt.gamma);

        deviceData = { a: alpha, b: beta, c: gamma };

        dataElement.innerHTML = "state: " + dataChannel.readyState;
        if (dataChannel && dataChannel.readyState === "open") {
          dataElement.innerHTML = JSON.stringify(deviceData);
          dataChannel.send(
            JSON.stringify({ type: "accelerometer", data: deviceData })
          );
        }
      },
      function(error) {
        errorElement.innerHTML = error;
      }
    );
  }
}

function addTapDetectionListener() {
  var tapArea = document.getElementById(services.tapDetection.id);
  tapArea.addEventListener("click", function(evt) {
    console.log("sending tap");
    if (navigator && navigator.vibrate) {
      navigator.vibrate(1000);
    }
    dataChannel.send(
      JSON.stringify({ type: "tap", data: services.tapDetection.id })
    );
  });
}

function addAmbientLightListener() {
  dataElement.innerHTML +=
    "devicelight: " +
    ("ondevicelight" in window) +
    " lightlevel: " +
    ("onlightlevel" in window);
  window.addEventListener("devicelight", function(evt) {
    dataElement.innerHTML = evt.value;
  });
}

function addDeviceProximityListener() {
  dataElement.innerHTML +=
    "prox: " +
    ("ondeviceproximity" in window) +
    " user: " +
    ("onuserproximity" in window);

  if ("ondeviceproximity" in window) {
    window.addEventListener("deviceproximity", function(evt) {
      dataElement.innerHTML =
        "proximity:" + JSON.stringify(evt) + ", " + JSON.stringify(evt.value);
      //dataElement.innerHTML = evt.value;
      dataChannel.send(
        JSON.stringify({ type: "proximity", data: evt.value > 0 })
      );
    });
  }
}

function addDeviceMotionListener() {
  dataElement.innerHTML += "motion: " + ("ondevicemotion" in window);
  if ("ondevicemotion" in window) {
    window.addEventListener("devicemotion", function(evt) {
      //if (evt.acceleration.x > 0.5 || evt.acceleration.y > 0.5 || evt.acceleration.z > 0.5) {
      if (evt.rotationRate.alpha > 30) {
        dataElement.innerHTML =
          "motion:" +
          JSON.stringify(evt) +
          ", " +
          JSON.stringify({ x: evt.rotationRate.alpha });
      }
      //}
      //dataElement.innerHTML = evt.value;
      //dataChannel.send(JSON.stringify({type:'proximity', data: evt.value > 0}));
    });
  }
}
