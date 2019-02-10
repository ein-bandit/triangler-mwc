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
        NetworkEventDispatcher.StartListening(NetworkEventType.Register_Player, RegisterPlayer);
        NetworkEventDispatcher.StartListening(NetworkEventType.Unregister_Player, UnregisterPlayer);
        NetworkEventDispatcher.StartListening(NetworkEventType.Network_Input_Event, ReceivePlayerInput);
    }

    private void OnDestroy()
    {
        NetworkEventDispatcher.StopListening(NetworkEventType.Register_Player, RegisterPlayer);
        NetworkEventDispatcher.StopListening(NetworkEventType.Unregister_Player, UnregisterPlayer);
        NetworkEventDispatcher.StopListening(NetworkEventType.Network_Input_Event, ReceivePlayerInput);
    }

    void RegisterPlayer(DataHolder playerInfo)
    {
        Guid playerGuid = (Guid)playerInfo.data;
        GameObject player = Instantiate(playerPrefab);
        Player playerObj = player.GetComponent<Player>();
        playerObj.SetPlayerColor(Color.cyan);

        players.Add(playerGuid, playerObj);
    }

    void UnregisterPlayer(DataHolder playerInfo)
    {
        Player playerObj = players[(Guid)playerInfo.data];
        Destroy(playerObj.gameObject);
    }

    void ReceivePlayerInput(DataHolder data)
    {
        players[(Guid)data.identifier].ReceiveInput((InputDataType)data.type, data.data);
    }
}
