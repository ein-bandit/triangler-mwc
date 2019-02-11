using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {

    }

    public void PlayerCountUpdate(int playerCount)
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            FindObjectOfType<MenuController>().UpdatePlayerCount(playerCount);
        }
    }
}
