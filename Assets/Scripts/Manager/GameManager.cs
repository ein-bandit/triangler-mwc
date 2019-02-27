using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private PlayerManager playerManager;

    private Coroutine startGameCountdownCoroutine;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        SceneManager.activeSceneChanged += activeSceneChanged;
    }

    private void activeSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.name == "Game")
        {
            startGameCountdownCoroutine = null;

            playerManager.InitPlayersForScene(GameScene.Game);
            StartCoroutine(FindObjectOfType<GameController>().StartCountdown());

        }
        else if (newScene.name == "Menu")
        {
            playerManager.InitPlayersForScene(GameScene.Menu);
        }
    }

    public GameScene GetActiveGameScene()
    {
        return (GameScene)System.Enum.Parse(typeof(GameScene), SceneManager.GetActiveScene().name);
    }

    public void AdvanceToGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void AdvanceToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void UpdatePlayerCount(int count)
    {
        FindObjectOfType<MenuController>().UpdatePlayerCount(count);
    }

    public void TriggerGameEnd(string playerUIIdentifier)
    {
        StartCoroutine(FindObjectOfType<GameController>().ShowEnd(playerUIIdentifier));
    }

    public void ForceGameStartCountdown()
    {
        if (startGameCountdownCoroutine == null)
        {
            playerManager.ForcePlayersReady();
        }
    }

    public void StartGameCountdown()
    {
        if (startGameCountdownCoroutine != null)
        {
            StopCoroutine(startGameCountdownCoroutine);
        }
        startGameCountdownCoroutine = StartCoroutine(FindObjectOfType<MenuController>().StartGameCountdown());
    }
}

