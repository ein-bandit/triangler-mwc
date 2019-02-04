var featureDisplayArea = document.getElementById("feature-area");

var supportedFeatures = [];
var enabledFeatures = [];

var features = {
  tapDetection: {
    available: true,
    message: "on screen actions enabled",
    listener: function(sendFunc) {
      for (var areaIndex in config.tapDetection.areas) {
        var id = config.tapDetection.areas[areaIndex];
        if ((tapArea = document.getElementById(id)) !== null) {
          tapArea.addEventListener("click", function(evt) {
            evt.target.classList.add("tapped");
            setTimeout(() => {
              evt.target.classList.remove("tapped");
            }, 250);
            sendFunc({ type: "tap", data: id });
          });
        }
      }
    }
  },
  vibrate: {
    available: navigator.vibrate,
    message: "vibration support",
    listener: function() {
      //no setup needed: just call navigator.vibrate(duration);
    }
  },
  deviceOrientation: {
    available: window.DeviceOrientationEvent,
    message: "device orientation available",
    listener: function(sendFunc) {
      window.addEventListener("deviceorientation", function(evt) {
        sendFunc({
          type: "accelerometer",
          data: {
            a: Math.floor(evt.alpha),
            b: Math.floor(evt.beta),
            c: Math.floor(evt.gamma)
          }
        });
      });
    }
  },
  deviceProximity: {
    available: "ondeviceproximity" in window,
    message: "device proximity available",
    listener: function(sendFunc) {
      window.addEventListener("deviceproximity", function(evt) {
        sendFunc({ type: "proximity", data: evt.value > 0 });
      });
    }
  },
  deviceMotion: {
    available: window.DeviceMotionEvent,
    message: "device motion available",
    listener: function(sendFunc) {
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
        //sendFunc({type:'deviceMotion', data: evt.value > 0}));
      });
    }
  },
  ambientLight: {
    available: "ondevicelight" in window,
    message: "ambient light available",
    listener: function(sendFunc) {
      window.addEventListener("devicelight", function(evt) {
        sendFunc(evt.value);
      });
    }
  }
};

for (var property in features) {
  if (features.hasOwnProperty(property) && features[property].available) {
    if (config[property].enabled) {
      addAvailableFeatureMessage(features[property].message);
      //feature is supported by browser and enabled in config.
      enabledFeatures.push(property);
    }
    supportedFeatures.push(property);
  }
}

if (debug) {
  console.log("available features: " + supportedFeatures.join(","));
}

function addAvailableFeatureMessage(message) {
  if (!featureDisplayArea || !message) {
    return;
  }
  var feature = document.createElement("div");
  feature.classList.add("detected-feature");
  var featureMessage = document.createElement("span");
  featureMessage.innerHTML = message;
  feature.appendChild(featureMessage);
  featureDisplayArea.appendChild(feature);
}
