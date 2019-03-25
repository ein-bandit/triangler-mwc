const uwc = window.uwc;

var gameEnd = document.getElementById("game-end");
var gameWinner = document.getElementById("game-winner");

var player = document.getElementById("player-triangle");

const connectElement = document.getElementById("connect");
var connectBtn = connectElement.getElementsByClassName("connect-btn")[0];
var contentElement = document.getElementById("content");

//setup handlers
uwc.eventDispatcher.on("uwc.initialized", function() {
  uiLogic.changeState("ready");
});

uwc.eventDispatcher.on("uwc.message", function(message) {
  if (message.type === "command") {
    uiLogic.executeCommand(message.data);
  } else if (message.type === "change_state") {
    uiLogic.changeState(message.data);
  } else {
    console.error("invalid data retrieved", message);
  }
});

uwc.eventDispatcher.on("uwc.quit", function() {
  uiLogic.changeState("connect");
});

//setup ui logic
const uiLogic = {};
uiLogic.changeState = function(state) {
  switch (state) {
    case "ready":
      connectElement.classList.add("hidden");
      contentElement.classList.add("hidden");
      connectBtn.classList.remove("disabled");
      readyElement.classList.remove("hidden");
      gameEnd.classList.add("hidden");
      gameWinner.classList.add("hidden");

      toggleTapArea(readyBtn, false);

      readyBtn.innerHTML = "ready";

      break;
    case "game_start":
      readyElement.classList.add("hidden");
      contentElement.classList.remove("hidden");

      stealthElement.classList.remove("disabled");
      boostElement.classList.remove("disabled");
      fireElement.classList.remove("disabled");

      uwc.eventDispatcher.emit("uwc.sending-enabled", true);
      break;
    case "game_over":
      uwc.eventDispatcher.emit("uwc.sending-enabled", false);

      contentElement.classList.add("hidden");
      gameEnd.classList.remove("hidden");
      uwc.eventDispatcher.emit("features.vibrate", 100);
      setTimeout(() => {
        uwc.eventDispatcher.emit("features.vibrate", 100);
      }, 200);
      break;
    case "game_winner":
      uwc.eventDispatcher.emit("uwc.sending-enabled", false);
      contentElement.classList.add("hidden");
      gameEnd.classList.remove("hidden");
      gameWinner.classList.remove("hidden");
      break;
    case "connect":
      uwc.eventDispatcher.emit("uwc.sending-enabled", false);
      connectElement.classList.remove("hidden");
      contentElement.classList.add("hidden");
      readyElement.classList.add("hidden");
      gameEnd.classList.add("hidden");
      gameWinner.classList.add("hidden");

      connectBtn.innerHTML = "connect";

      break;
  }
};

var boostElement = document.getElementById("tap-area-boost");
var stealthElement = document.getElementById("tap-area-stealth");
var fireElement = document.getElementById("tap-area-fire");

uiLogic.executeCommand = function(command) {
  var clientCommand = command;
  var extra = null;
  if (command.indexOf(":") !== -1) {
    var split = command.split(":");
    clientCommand = split[0];
    extra = split[1];
  }
  switch (clientCommand) {
    case "color":
      player.style.borderColor =
        "transparent transparent #" + extra + " transparent";
      break;
    case "boost_activated":
      toggleTapArea(boostElement, true);
      navigator.vibrate(500);
      break;
    case "boost_available":
      toggleTapArea(boostElement, false);
      break;
    case "stealth_activated":
      toggleTapArea(stealthElement, true);
      break;
    case "stealth_available":
      toggleTapArea(stealthElement, false);
      break;
    case "fire_activated":
      toggleTapArea(fireElement, true);
      break;
    case "fire_available":
      toggleTapArea(fireElement, false);
      break;
  }
};

function toggleTapArea(elem, disable) {
  elem.classList[disable === true ? "add" : "remove"]("disabled");
}

//guiHelpers

const readyElement = document.getElementById("ready");
const readyBtn = readyElement.getElementsByClassName("ready-btn")[0];

const guiHelper = {
  handleReadyClick: function() {
    readyBtn.classList.add("disabled");
    readyBtn.classList.add("tapped");

    setTimeout(() => {
      readyBtn.innerHTML = "starting...";
      readyBtn.classList.remove("tapped");
    }, 300);

    //wait a bit to make sure connection is established correctly, maybe not needed.
    setTimeout(() => {
      uwc.eventDispatcher.emit("uwc.send-message", {
        type: "ready",
        data: "ready"
      });
    }, 1000);
  },
  handleConnectClick: function() {
    connectBtn.classList.add("disabled");

    connectBtn.classList.add("tapped");
    setTimeout(() => {
      connectBtn.classList.remove("tapped");
    }, 250);

    setTimeout(() => {
      connectBtn.innerHTML = "connecting...";
    }, 300);

    //setup complete - trigger init.
    uwc.eventDispatcher.emit("uwc.connect");
  }
};

if (uwc.config.debug) {
  const featureDisplayArea = document.getElementById("feature-area");

  const messages = {
    tapDetection: "on screen actions enabled",
    vibration: "vibration support",
    deviceOrientation: "device orientation available",
    deviceProximity: "device proximity available",
    deviceMotion: "device motion available",
    deviceLight: "ambient light available"
  };

  for (var prop in uwc.availableFeatures) {
    if (uwc.config.features[prop] && uwc.config.features[prop] !== false) {
      var feature = document.createElement("div");
      feature.classList.add("detected-feature");
      var featureMessage = document.createElement("span");
      featureMessage.innerHTML = messages[prop];
      feature.appendChild(featureMessage);
      featureDisplayArea.appendChild(feature);
    }
  }
}
