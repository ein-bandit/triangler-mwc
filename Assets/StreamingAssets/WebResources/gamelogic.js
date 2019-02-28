const gamelogic = {};

gamelogic.changeState = function(state) {
  switch (state) {
    case "ready":
      deactivateNoSleep();
      connectElement.classList.add("hidden");
      readyElement.classList.remove("hidden");

      readyBtn.innerHTML = "ready";

      readyElement.classList.remove("disabled");

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
      setTimeout(() => {
        navigator.vibrate(75);
      }, 100);
      setTimeout(() => {
        navigator.vibrate(75);
      }, 200);
      setTimeout(() => {
        navigator.vibrate(75);
      }, 300);
      break;
      //show game over screen.
      break;
    case "game_winner":
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

gamelogic.executeCommand = function(command) {
  switch (command) {
    //put this to a own method inside features?
    case "boost_activated":
      //disable tap area boost.
      navigator.vibrate(500);
      break;
    case "boost_available":
    //enable tap area boost.
    case "stealth_activated":
    //disable tap area stealth.
    case "stealth_available":
    //enable tap area stealth.
    case "fire_activated":
    //disable fire tap area.
    case "fire_available":
    //enable fire tap area
  }
};

//guiHelpers

const readyElement = document.getElementById("ready");
const readyBtn = readyElement.getElementsByClassName("ready-btn")[0];

const guiHelper = {
  handleReadyClick: function() {
    readyElement.classList.add("disabled");

    readyBtn.innerHTML = "waiting";

    activateNoSleep();

    //wait a bit to make sure connection is established correctly.
    setTimeout(() => {
      mobileWebControl.sendFunction({ type: "ready", data: "ready" }, true);
    }, 1000);
  }
};
