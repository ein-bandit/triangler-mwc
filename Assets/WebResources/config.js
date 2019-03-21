const config = {
  serverPort: 7770,
  features: {
    tapDetection: [
      "tap-area-boost",
      "tap-area-stealth",
      "tap-area-fire",
      "tap-area-reset_orientation"
    ],
    vibration: true,
    deviceOrientation: true,
    deviceProximity: false,
    deviceMotion: false,
    deviceLight: false
  },
  customFeatureHandlers: {
    deviceLight: evt => {
      console.log("deviceLight changed", evt);
    }
  },
  exposeToWindow: true,
  debug: true
};

export default config;
