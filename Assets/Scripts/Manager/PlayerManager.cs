using System;
using System.Collections;
using System.Collections.Generic;
using MobileWebControl;
using MobileWebControl.NetworkData;
using MobileWebControl.NetworkData.InputData;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : MonoBehaviour
{
    private Dictionary<Guid, Player> players = new Dictionary<Guid, Player>();

    public GameObject playerPrefab;

    void Start()
    {
        DataEventManager.StartListening(NetworkEventType.Register_Player, RegisterPlayer);
        DataEventManager.StartListening(NetworkEventType.Unregister_Player, UnregisterPlayer);
        DataEventManager.StartListening(NetworkEventType.Network_Input_Event, ReceivePlayerInput);
    }

    private void OnDestroy()
    {
        DataEventManager.StopListening(NetworkEventType.Register_Player, RegisterPlayer);
        DataEventManager.StopListening(NetworkEventType.Unregister_Player, UnregisterPlayer);
        DataEventManager.StopListening(NetworkEventType.Network_Input_Event, ReceivePlayerInput);
    }

    void RegisterPlayer(DataHolder playerInfo)
    {
        Guid playerGuid = (Guid)playerInfo.data;
        GameObject player = Instantiate(playerPrefab);
        Player playerObj = player.GetComponent<Player>();

        players.Add(playerGuid, playerObj);
    }

    void UnregisterPlayer(DataHolder playerInfo)
    {
        Player playerObj = players[(Guid)playerInfo.data];
        Destroy(playerObj.gameObject);
    }

    void ReceivePlayerInput(DataHolder data)
    {
        players[data.receiver].ReceiveInput((InputDataType)data.type, data.data);
    }
}
