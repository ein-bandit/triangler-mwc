using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private PlayerManager playerManager;

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
    }

    public void AdvanceToGame()
    {
        Debug.Log("advance to game");
        SceneManager.LoadScene("Game");
    }

    public void PlayerCountUpdate(int playerCount)
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            FindObjectOfType<MenuController>().UpdatePlayerCount(playerCount);
        }
    }
}
