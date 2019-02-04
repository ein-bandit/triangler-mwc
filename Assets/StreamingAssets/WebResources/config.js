//define which browser apis should be enabled and used to stream data.

const config = {
  tapDetection: { enabled: true, areas: ["tap-area-1", "tap-area-2"] },
  vibrate: { enabled: true },
  deviceOrientation: { enabled: false },
  deviceProximity: { enabled: true },
  deviceMotion: { enabled: true },
  ambientLight: { enabled: false }
};

const debug = false;
