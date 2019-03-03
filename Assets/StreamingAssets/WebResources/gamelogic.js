const gamelogic = {};

gamelogic.changeState = function(state) {
  console.log("received state change - " + state);
  switch (state) {
    case "ready":
      deactivateNoSleep();
      connectElement.classList.add("hidden");
      contentElement.classList.add("hidden");
      readyElement.classList.remove("hidden");

      toggleTapArea(readyBtn, false);

      readyBtn.innerHTML = "ready";

      if (debug) {
        dataElement.classList.remove("hidden");
      }
      break;
    case "game_start":
      readyElement.classList.add("hidden");
      contentElement.classList.remove("hidden");
      gameStarted = true;
      break;
    case "game_over":
      gameStarted = false;
      setTimeout(() => {
        navigator.vibrate(75);
      }, 100);
      setTimeout(() => {
        navigator.vibrate(75);
      }, 200);
      setTimeout(() => {
        navigator.vibrate(75);
      }, 300);

      contentElement.classList.add("hidden");

      //show game over screen.
      break;
    case "game_winner":
      gameStarted = false;
      //show game over with winner.
      break;
    case "connect":
      deactivateNoSleep();
      connecting = false; //disconnect occurred.
      gameStarted = false;
      connectElement.classList.remove("hidden");
      contentElement.classList.add("hidden");
      readyElement.classList.add("hidden");

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
  switch (command) {
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

    readyBtn.innerHTML = "waiting";

    activateNoSleep();

    //wait a bit to make sure connection is established correctly, maybe not needed.
    setTimeout(() => {
      mobileWebControl.sendFunction({ type: "ready", data: "ready" }, true);
    }, 1000);
  }
};
