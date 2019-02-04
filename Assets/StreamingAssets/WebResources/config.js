//define which browser apis should be enabled and used to stream data.

const config = {
  tapDetection: { enabled: true, areas: ["tap-area"] },
  vibrate: { enabled: true },
  deviceOrientation: { enabled: false },
  deviceProximity: { enabled: true },
  deviceMotion: { enabled: true },
  ambientLight: { enabled: false }
};

const debug = true;
