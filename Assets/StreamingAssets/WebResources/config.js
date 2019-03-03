//define which browser apis should be enabled and used to stream data.

const config = {
  serverPort: 1234,
  features: {
    tapDetection: {
      enabled: true,
      // areas: [
      //   "tap-area-boost",
      //   "tap-area-stealth",
      //   "tap-area-fire",
      //   "tap-area-reset-orientation"
      // ],
      areas: {
        boost: "tap-area-boost",
        stealth: "tap-area-stealth",
        fire: "tap-area-fire",
        resetOrientiation: "tap-area-reset-orientation"
      }
    },
    vibrate: { enabled: true },
    deviceOrientation: { enabled: true },
    deviceProximity: { enabled: true },
    deviceMotion: { enabled: false },
    deviceLight: { enabled: false }
  }
};

const debug = true;
