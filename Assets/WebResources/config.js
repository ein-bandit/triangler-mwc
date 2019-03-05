//define which browser apis should be enabled and used to stream data.

const config = {
  serverPort: 7770,
  features: {
    tapDetection: {
      enabled: true,
      areas: {
        boost: "tap-area-boost",
        stealth: "tap-area-stealth",
        fire: "tap-area-fire",
        resetOrientiation: "tap-area-reset_orientation"
      }
    },
    vibrate: { enabled: true }, //just referenced to show detected feature message.
    deviceOrientation: { enabled: true },
    deviceProximity: { enabled: true },
    deviceMotion: { enabled: false },
    deviceLight: { enabled: false }
  }
};

const debug = false;
