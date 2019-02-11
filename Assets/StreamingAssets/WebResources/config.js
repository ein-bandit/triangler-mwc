//define which browser apis should be enabled and used to stream data.

const config = {
  tapDetection: {
    enabled: true,
    areas: [
      "tap-area-boost",
      "tap-area-mask",
      "tap-area-fire",
      "tap-area-reset-orientation"
    ]
  },
  vibrate: { enabled: true },
  deviceOrientation: { enabled: true },
  deviceProximity: { enabled: true },
  deviceMotion: { enabled: false },
  deviceLight: { enabled: false }
};

const debug = true;
