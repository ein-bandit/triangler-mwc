using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private List<Player> players = new List<Player>();
    public GameObject playerGameObject;

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

    public Player RegisterNewPlayer()
    {
        GameObject newPlayer = Instantiate(playerGameObject);

        players.Add(newPlayer.GetComponent<Player>());
        Debug.Log("added a new player: " + players.Count);

        return newPlayer.GetComponent<Player>();
    }
}
