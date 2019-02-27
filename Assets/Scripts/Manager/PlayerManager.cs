﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MobileWebControl;
using MobileWebControl.NetworkData;
using MobileWebControl.NetworkData.InputData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    private DictionaryMap<Guid, IPlayer> playerToGuid = new DictionaryMap<Guid, IPlayer>();
    private Dictionary<IPlayer, PlayerConstraints> playerConstraints = new Dictionary<IPlayer, PlayerConstraints>();
    private List<Color> playerColors = new List<Color>() { Color.cyan, Color.green, Color.red, Color.yellow };
    public GameObject playerPrefab;
    public GameObject aiPrefab;
    public GameObject projectilePrefab;
    public GameObject menuPlayerPrefab;

    public int startGameDelayInSeconds = 3;

    public int aiPlayers = 0;

    public float playerReadyDelay = 3f;

    public void ForcePlayersReady()
    {
        //force game start
        if (playerConstraints.Count > 0)
        {
            //set players ready and advance.
            foreach (Player p in playerConstraints.Keys)
            {
                SetPlayerReady(p);
            }
            CheckAllPlayersReady();
        }
    }

    void Start()
    {
        NetworkEventDispatcher.StartListening(NetworkEventType.Register_Player, RegisterPlayer);
        NetworkEventDispatcher.StartListening(NetworkEventType.Unregister_Player, UnregisterPlayer);
        NetworkEventDispatcher.StartListening(NetworkEventType.Network_Input_Event, ReceivePlayerInput);


        if (aiPlayers > 0)
        {
            for (int i = 0; i < aiPlayers; i++)
            {
                RegisterAIPlayer();
            }
        }
    }

    private void RegisterAIPlayer()
    {
        IPlayer player = InstantiatePlayer(true);
        SetPlayerReady(player);
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
    }

    public void InitPlayersForScene(GameScene scene)
    {
        foreach (KeyValuePair<IPlayer, PlayerConstraints> kv in playerConstraints)
        {
            kv.Key.ActivatePlayerObject(GameScene.Game == scene);
            kv.Value.MenuPlayer.gameObject.SetActive(GameScene.Menu == scene);
            if (!kv.Key.isAIControlled())
            {
                StartCoroutine(
                    SendNewPlayerStatus(
                        playerToGuid.Reverse[kv.Key],
                        GameScene.Menu == scene ? PlayerStatus.ready : PlayerStatus.game_start,
                        GameScene.Menu == scene ? playerReadyDelay : 0f)
                    );
            }
        }
    }

    public void StartGame()
    {
        foreach (IPlayer player in playerConstraints.Keys)
        {
            player.StartMovement();
        }
    }

    public void SendMessageToAllClients(string message)
    {
        foreach (Guid player in playerToGuid.GetKeys())
        {
            MobileWebController.instance.SendToClients(player, message);
        }
    }

    public void SendMessageToClient(Player player, string message)
    {
        MobileWebController.instance.SendToClients(playerToGuid.Reverse[player], message);
    }

    private void SendMessageToClient(Guid guid, string message)
    {
        MobileWebController.instance.SendToClients(guid, message);
    }

    public void RegisterPlayer(DataHolder playerInfo)
    {
        Guid playerGuid = (Guid)playerInfo.data;
        IPlayer player = InstantiatePlayer(false);

        playerToGuid.Add(playerGuid, player);

        if (GameManager.instance.GetActiveGameScene() == GameScene.Menu)
        {
            GameManager.instance.UpdatePlayerCount(playerConstraints.Keys.Count);
            StartCoroutine(SendNewPlayerStatus(playerGuid, PlayerStatus.ready, playerReadyDelay));
        }
    }

    private IPlayer InstantiatePlayer(bool isAI)
    {
        GameObject holder = new GameObject("Player" + playerConstraints.Count + (isAI ? "_AI" : ""));
        holder.transform.parent = transform;
        GameObject playerObj = Instantiate(isAI ? aiPrefab : playerPrefab, holder.transform);
        IPlayer player = isAI ? (IPlayer)playerObj.GetComponent<AIPlayer>() : (IPlayer)playerObj.GetComponent<Player>();

        Color playerColor = isAI ? Color.grey : playerColors[playerConstraints.Keys.Count];
        GameObject projectileObj = Instantiate(projectilePrefab, holder.transform);
        Projectile projectile = projectileObj.GetComponent<Projectile>();

        player.Initialize(playerColor, projectile, playerConstraints.Keys.Count);
        playerObj.SetActive(false);

        projectile.Initialize(player);
        projectileObj.SetActive(false);

        GameObject menuPlayer = Instantiate(menuPlayerPrefab, holder.transform);
        menuPlayer.GetComponent<MenuPlayer>().Init(playerColor, playerConstraints.Keys.Count);
        menuPlayer.SetActive(GameManager.instance.GetActiveGameScene() == GameScene.Menu);

        playerConstraints.Add(
            player,
            new PlayerConstraints(menuPlayer.GetComponent<MenuPlayer>())
        );

        return player;
    }

    public void UnregisterPlayer(DataHolder playerInfo)
    {
        IPlayer player = playerToGuid.Forward[(Guid)playerInfo.identifier];
        PlayerConstraints constraints = playerConstraints[player];
        playerConstraints.Remove(player);
        playerToGuid.Remove((Guid)playerInfo.identifier);

        player.DestroyMe();
        Destroy(constraints.MenuPlayer.gameObject);

        if (GameManager.instance.GetActiveGameScene() == GameScene.Menu)
        {
            GameManager.instance.UpdatePlayerCount(playerConstraints.Keys.Count);
        }
        else if (GameManager.instance.GetActiveGameScene() == GameScene.Game)
        {
            CheckRemainingPlayers();
        }
    }

    //Gets called from NetworkEventDispatcher -> definitely a client / Player instance.
    public void ReceivePlayerInput(DataHolder data)
    {
        Player player = (Player)playerToGuid.Forward[(Guid)data.identifier];

        if (player != null)
        {
            PlayerConstraints constraints = playerConstraints[player];

            if (GameManager.instance.GetActiveGameScene() == GameScene.Menu
                && ((InputDataType)data.type).Equals(InputDataType.ready))
            {
                SetPlayerReady(player);
                CheckAllPlayersReady();
            }
            else if (GameManager.instance.GetActiveGameScene() == GameScene.Game)
            {
                player.ReceiveInput((InputDataType)data.type, data.data);
            }
        }
        else
        {
            Debug.Log($"could not handle message of unknown client. {data.identifier}");
        }
    }

    private void CheckAllPlayersReady()
    {
        //if all area ready and at least 1 human player.
        if (playerConstraints.Values.All(pc => pc.ReadyAndAlive) && playerToGuid.GetKeys().Count > 0)
        {
            Debug.Log("starting game");
            GameManager.instance.StartGameCountdown();
        }
    }

    public void HandlePlayerDeath(IPlayer player)
    {
        SetPlayerReady(player);

        CheckRemainingPlayers();
        if (!player.isAIControlled())
        {
            Guid guid = playerToGuid.Reverse[player];
            SendNewPlayerStatus(guid, PlayerStatus.game_over);
        }
    }

    private void CheckRemainingPlayers()
    {
        List<IPlayer> alive = playerToGuid.GetValues().FindAll(a => playerConstraints[a].ReadyAndAlive == true);

        if (alive.Count == 0)
        {
            //stop AIs.
            GameManager.instance.TriggerGameEnd(null);
        }
        else if (alive.Count == 1)
        {
            Player player = (Player)playerToGuid.GetValues().FindLast(p => playerConstraints[p].ReadyAndAlive);
            player.DisablePlayer();
            SendNewPlayerStatus(playerToGuid.Reverse[player], PlayerStatus.game_winner);

            GameManager.instance.TriggerGameEnd(player.GetUIIdentifier());
        }
    }

    private IEnumerator SendNewPlayerStatus(Guid playerGuid, PlayerStatus status, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        SendMessageToClient(playerGuid, status.ToString());
    }

    private void SetPlayerReady(IPlayer player)
    {
        PlayerConstraints pc = playerConstraints[player];
        pc.SetReadyAndAlive(true);
        playerConstraints[player] = pc;
    }
}
