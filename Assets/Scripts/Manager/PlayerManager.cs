using System;
using System.Collections;
using System.Collections.Generic;
using MobileWebControl;
using MobileWebControl.NetworkData;
using MobileWebControl.NetworkData.InputData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    private Dictionary<Guid, PlayerHolder> players = new Dictionary<Guid, PlayerHolder>();
    //convenience method to avoid iterating over players dict every time.
    private Dictionary<Player, Guid> playerGuids = new Dictionary<Player, Guid>();
    private List<Color> playerColors = new List<Color>() { Color.cyan, Color.green, Color.red, Color.yellow };
    public GameObject playerPrefab;
    public GameObject projectilePrefab;
    public GameObject menuPlayerPrefab;

    public int startGameDelayInSeconds = 3;
    public float endGameDelay = 5f;

    public int dummyPlayers = 0;

    public float playerReadyDelay = 3f;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) && players.Count > 0)
        {
            //set players ready and advance.
            foreach (KeyValuePair<Guid, PlayerHolder> ph in players)
            {
                StartCoroutine(DummyStart(ph.Key));
            }
        }
    }

    void Start()
    {
        NetworkEventDispatcher.StartListening(NetworkEventType.Register_Player, RegisterPlayer);
        NetworkEventDispatcher.StartListening(NetworkEventType.Unregister_Player, UnregisterPlayer);
        NetworkEventDispatcher.StartListening(NetworkEventType.Network_Input_Event, ReceivePlayerInput);

        SceneManager.activeSceneChanged += activeSceneChanged;

        if (dummyPlayers > 0)
        {
            for (int i = 0; i < dummyPlayers; i++)
            {
                Guid g = new Guid("00000000-0000-0000-0000-00000000000" + i);
                StartCoroutine(RegisterDummy(g));
            }
        }
    }

    private IEnumerator RegisterDummy(Guid g)
    {
        yield return new WaitForEndOfFrame();
        DataHolder d = new DataHolder(g, InputDataType.register, g);
        RegisterPlayer(d);

        PlayerHolder ph = players[g];
        ph.AI = true;
        players[g] = ph;
    }
    private IEnumerator DummyStart(Guid g)
    {
        yield return new WaitForSeconds(1f);
        DataHolder s = new DataHolder(g, InputDataType.ready, null);
        ReceivePlayerInput(s);
    }

    private void OnDestroy()
    {
        NetworkEventDispatcher.StopListening(NetworkEventType.Register_Player, RegisterPlayer);
        NetworkEventDispatcher.StopListening(NetworkEventType.Unregister_Player, UnregisterPlayer);
        NetworkEventDispatcher.StopListening(NetworkEventType.Network_Input_Event, ReceivePlayerInput);

        SceneManager.activeSceneChanged -= activeSceneChanged;
    }

    private void activeSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.name == "Game")
        {
            foreach (Guid playerGuid in players.Keys)
            {
                players[playerGuid].MenuPlayer.gameObject.SetActive(false);
                players[playerGuid].Player.gameObject.SetActive(true);
                StartCoroutine(SendNewPlayerStatus(playerGuid, PlayerStatus.game_start));
            }
            StartCoroutine(StartCountdown());
        }
        else if (newScene.name == "Menu")
        {
            foreach (Guid playerGuid in players.Keys)
            {
                players[playerGuid].Player.gameObject.SetActive(false);
                players[playerGuid].MenuPlayer.gameObject.SetActive(true);
                Debug.Log("scene changed");
                StartCoroutine(SendNewPlayerStatus(playerGuid, PlayerStatus.ready, playerReadyDelay));
            }
            Debug.Log($"updating gui to player nr {players.Keys.Count}");
            GameManager.instance.PlayerCountUpdate(players.Keys.Count);
        }
    }

    private IEnumerator StartCountdown()
    {
        Text countdownText = FindObjectOfType<Canvas>().transform.Find("Countdown").GetComponent<Text>();
        int count = startGameDelayInSeconds;
        do
        {
            countdownText.text = count.ToString();
            count--;
            yield return new WaitForSeconds(1f);
        } while (count > 0);
        //show Countdown;
        foreach (Guid playerGuid in players.Keys)
        {
            Debug.Log("starting game");
            players[playerGuid].Player.StartPlayerMovement();
        }

        countdownText.text = "GO!";
        yield return new WaitForSeconds(.75f);
        countdownText.gameObject.SetActive(false);
    }

    public void SendMessageToAllClients(string message)
    {
        foreach (Guid playerGuid in players.Keys)
        {
            if (players[playerGuid].AI == true) return;
            MobileWebController.instance.SendToClients(playerGuid, message);
        }
    }

    public void SendMessageToClient(Player player, string message)
    {
        MobileWebController.instance.SendToClients(playerGuids[player], message);
    }

    private void SendMessageToClient(Guid guid, string message)
    {
        if (players[guid].AI == true) return;

        MobileWebController.instance.SendToClients(guid, message);
    }

    public void RegisterPlayer(DataHolder playerInfo)
    {
        Guid playerGuid = (Guid)playerInfo.data;
        GameObject player = Instantiate(playerPrefab, transform);

        Color playerColor = playerColors[players.Keys.Count];
        GameObject projectile = Instantiate(projectilePrefab, transform);

        player.GetComponent<Player>().Init(playerColor, projectile.GetComponent<Projectile>(), players.Keys.Count);
        player.SetActive(false);

        projectile.GetComponent<Projectile>().Init(player.GetComponent<Player>(), playerColor);
        projectile.SetActive(false);

        GameObject menuPlayer = Instantiate(menuPlayerPrefab, transform);
        menuPlayer.GetComponent<MenuPlayer>().Init(playerColor, players.Keys.Count);
        menuPlayer.SetActive(false);

        players.Add(
            playerGuid,
            new PlayerHolder(player.GetComponent<Player>(), menuPlayer.GetComponent<MenuPlayer>())
        );
        playerGuids.Add(player.GetComponent<Player>(), playerGuid);

        if (SceneManager.GetActiveScene().name == "Menu")
        {
            menuPlayer.SetActive(true);
            GameManager.instance.PlayerCountUpdate(players.Keys.Count);

            StartCoroutine(SendNewPlayerStatus(playerGuid, PlayerStatus.ready, playerReadyDelay));
        }
    }

    public void UnregisterPlayer(DataHolder playerInfo)
    {
        Debug.Log("unregister player");
        PlayerHolder player = players[(Guid)playerInfo.identifier];
        players.Remove((Guid)playerInfo.identifier);
        playerGuids.Remove(player.Player);
        Destroy(player.Player.gameObject);
        Destroy(player.MenuPlayer.gameObject);

        if (SceneManager.GetActiveScene().name == "Menu")
        {
            GameManager.instance.PlayerCountUpdate(players.Keys.Count);
        }
        else
        {
            CheckRemainingPlayers();
        }
    }

    public void ReceivePlayerInput(DataHolder data)
    {
        if (players.ContainsKey((Guid)data.identifier))
        {
            PlayerHolder player = players[(Guid)data.identifier];

            if (SceneManager.GetActiveScene().name == "Menu"
                && ((InputDataType)data.type).Equals(InputDataType.ready))
            {
                player.Ready = true;
                player.MenuPlayer.SetReady();
                players[(Guid)data.identifier] = player;
                CheckAllPlayersReady();
            }
            else if (SceneManager.GetActiveScene().name == "Game")
            {
                player.Player.ReceiveInput((InputDataType)data.type, data.data);
            }
        }
        else
        {
            Debug.Log($"could not handle message of unknown client. {data.identifier}");
        }
    }
    private void CheckAllPlayersReady()
    {
        //TOOD: add counter (10 sek after first player registered. avoid advance when timer not finished).
        foreach (PlayerHolder pH in players.Values)
        {
            if (pH.Ready == false) return;
        }

        GameManager.instance.AdvanceToGame();
    }

    public void RegistratePlayerDeath(Player player)
    {
        Guid id = playerGuids[player];
        PlayerHolder ph = players[id];
        ph.Ready = false; //because player died.
        players[id] = ph; //necessary?
        CheckRemainingPlayers();
        SendNewPlayerStatus(id, PlayerStatus.game_over);
    }

    private void CheckRemainingPlayers()
    {
        List<Player> alive = new List<Player>();
        foreach (PlayerHolder ph in players.Values)
        {
            if (ph.Ready == true)
            {
                alive.Add(ph.Player);
            }
        }

        if (alive.Count <= 1)
        {
            foreach (Player player in alive)
            {
                player.DisablePlayer();
                SendNewPlayerStatus(playerGuids[player], PlayerStatus.game_winner);
            }
            StartCoroutine(EndGame());
        }
    }

    private IEnumerator EndGame()
    {
        Debug.Log("Game over");
        FindObjectOfType<Canvas>().transform.Find("End").gameObject.SetActive(true);
        yield return new WaitForSeconds(endGameDelay);
        SceneManager.LoadScene("Menu");
    }

    private IEnumerator SendNewPlayerStatus(Guid player, PlayerStatus status, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        SendMessageToClient(player, status.ToString());
    }
}
