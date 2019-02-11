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
    public GameObject menuPlayerPrefab;

    void Start()
    {
        NetworkEventDispatcher.StartListening(NetworkEventType.Register_Player, RegisterPlayer);
        NetworkEventDispatcher.StartListening(NetworkEventType.Unregister_Player, UnregisterPlayer);
        NetworkEventDispatcher.StartListening(NetworkEventType.Network_Input_Event, ReceivePlayerInput);

        SceneManager.activeSceneChanged += activeSceneChanged;
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
                StartCoroutine(players[playerGuid].Player.StartPlayerMovement());

            }
        }
    }

    public void ActivateGameOnClients()
    {
        foreach (Guid playerGuid in players.Keys)
        {
            MobileWebController.instance.SendToClients(playerGuid, "start");
        }
    }

    public void RegisterPlayer(DataHolder playerInfo)
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            Guid playerGuid = (Guid)playerInfo.data;
            GameObject player = Instantiate(playerPrefab, transform);
            //getRandomPlayerColor
            Color playerColor = playerColors[players.Keys.Count + 1];
            player.GetComponent<Player>().SetPlayerColor(playerColor);
            player.SetActive(false);
            GameObject menuPlayer = Instantiate(menuPlayerPrefab, transform);
            menuPlayer.GetComponent<MenuPlayer>().SetPlayerColor(playerColor, players.Keys.Count);

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
        if (players.ContainsKey((Guid)data.identifier))
        {
            PlayerHolder player = players[(Guid)data.identifier];
            if (((InputDataType)data.type).Equals(InputDataType.ready))
            {
                Debug.Log("setting player ready");
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

    public Color GetRandomPlayerColor(Color notThisColor)
    {
        int playerColorIndex = playerColors.FindIndex(color => { return !color.Equals(notThisColor); });
        int randomIndex = 0;
        do
        {
            randomIndex = UnityEngine.Random.Range(0, playerColors.Count);
        } while (randomIndex == playerColorIndex);

        return playerColors[randomIndex];
    }
}
