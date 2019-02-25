using System;
using System.Collections;
using System.Collections.Generic;
using MobileWebControl;
using MobileWebControl.NetworkData;
using MobileWebControl.NetworkData.InputData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    private Dictionary<Guid, PlayerHolder> players = new Dictionary<Guid, PlayerHolder>();
    private List<Color> playerColors = new List<Color>() { Color.cyan, Color.green, Color.red, Color.yellow };
    public GameObject playerPrefab;
    public GameObject projectilePrefab;
    public GameObject menuPlayerPrefab;

    public float startGameDelay = 3f;

    public int dummyPlayers = 0;

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
        ph.Dummy = true;
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
            }
        }
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        yield return new WaitForSeconds(startGameDelay);
        //show Countdown;
        foreach (Guid playerGuid in players.Keys)
        {
            Debug.Log("starting game");
            players[playerGuid].Player.StartPlayerMovement();
        }
    }

    public void SendMessageToAllClients(string message)
    {
        foreach (Guid playerGuid in players.Keys)
        {
            if (players[playerGuid].Dummy == true) return;
            MobileWebController.instance.SendToClients(playerGuid, message);
        }
    }

    public void SendMessageToClient(Player player, string message)
    {
        foreach (Guid playerGuid in players.Keys)
        {
            if (players[playerGuid].Player == player)
            {
                MobileWebController.instance.SendToClients(playerGuid, message);
            }
        }
    }

    public void RegisterPlayer(DataHolder playerInfo)
    {
        if (SceneManager.GetActiveScene().name == "Menu")
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

            players.Add(
                playerGuid,
                new PlayerHolder(player.GetComponent<Player>(), menuPlayer.GetComponent<MenuPlayer>())
            );

            GameManager.instance.PlayerCountUpdate(players.Keys.Count);
        }
    }

    public void UnregisterPlayer(DataHolder playerInfo)
    {
        PlayerHolder player = players[(Guid)playerInfo.identifier];
        players.Remove((Guid)playerInfo.identifier);
        Destroy(player.Player.gameObject);
        Destroy(player.MenuPlayer.gameObject);

        GameManager.instance.PlayerCountUpdate(players.Keys.Count);
    }

    public void ReceivePlayerInput(DataHolder data)
    {
        Debug.Log(players.ContainsKey((Guid)data.identifier));
        if (players.ContainsKey((Guid)data.identifier))
        {
            PlayerHolder player = players[(Guid)data.identifier];
            if (((InputDataType)data.type).Equals(InputDataType.ready))
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
}
