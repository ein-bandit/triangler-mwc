var gameEnd = document.getElementById("game-end");
var gameWinner = document.getElementById("game-winner");

var player = document.getElementById("player-triangle");

const connectElement = document.getElementById("connect");
var connectBtn = connectElement.getElementsByClassName("connect-btn")[0];
var contentElement = document.getElementById("content");

const gamelogic = {};
gamelogic.changeState = function(state) {
  console.log("received state change - " + state);
  switch (state) {
    case "ready":
      deactivateNoSleep();
      connectElement.classList.add("hidden");
      contentElement.classList.add("hidden");
      connectBtn.classList.remove("disabled");
      readyElement.classList.remove("hidden");
      gameEnd.classList.add("hidden");
      gameWinner.classList.add("hidden");

      toggleTapArea(readyBtn, false);

      readyBtn.innerHTML = "ready";

      if (debug) {
        dataElement.classList.remove("hidden");
      }
      break;
    case "game_start":
      readyElement.classList.add("hidden");
      contentElement.classList.remove("hidden");

      for (var prop in config.features.tapDetection.areas) {
        if (config.features.tapDetection.areas.hasOwnProperty(prop)) {
          document
            .getElementById(config.features.tapDetection.areas[prop])
            .classList.remove("disabled");
        }
      }

      sendingEnabled = true;
      break;
    case "game_over":
      sendingEnabled = false;
      contentElement.classList.add("hidden");
      gameEnd.classList.remove("hidden");
      navigator.vibrate(100);
      setTimeout(() => {
        navigator.vibrate(100);
      }, 200);
      break;
    case "game_winner":
      sendingEnabled = false;
      contentElement.classList.add("hidden");
      gameEnd.classList.remove("hidden");
      gameWinner.classList.remove("hidden");
      break;
    case "connect":
      deactivateNoSleep();
      connecting = false; //disconnect occurred.
      sendingEnabled = false;
      connectElement.classList.remove("hidden");
      contentElement.classList.add("hidden");
      readyElement.classList.add("hidden");
      gameEnd.classList.add("hidden");
      gameWinner.classList.add("hidden");

      connectBtn.innerHTML = "connect";

      if (debug) {
        dataElement.classList.add("hidden");
      }
      break;
  }
};

var boostElement = document.getElementById(
  config.features.tapDetection.areas.boost
);
var stealthElement = document.getElementById(
  config.features.tapDetection.areas.stealth
);
var fireElement = document.getElementById(
  config.features.tapDetection.areas.fire
);

gamelogic.executeCommand = function(command) {
  console.log("received command - " + command);

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
    activateNoSleep();

    setTimeout(() => {
      readyBtn.innerHTML = "starting...";
      readyBtn.classList.remove("tapped");
    }, 300);

    //wait a bit to make sure connection is established correctly, maybe not needed.
    setTimeout(() => {
      uwc.sendFunction({ type: "ready", data: "ready" }, true);
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

    var webRTCMessageCallbacks = initializeWebRTCMessageCallbacks();
    uwc.initializeConnection(webRTCMessageCallbacks);

    function initializeWebRTCMessageCallbacks() {
      //overriding client actions from mobile web control.
      var actions = {};
      actions.initialize = function() {
        gamelogic.changeState("ready");
      };

      actions.onMessage = function(message) {
        if (message.type === "command") {
          gamelogic.executeCommand(message.data);
        } else if (message.type === "change_state") {
          gamelogic.changeState(message.data);
        } else {
          console.error("invalid data retrieved", message);
        }
      };

      actions.onClose = function() {
        gamelogic.changeState("connect");
      };
      return actions;
    }
  }
};
