using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : MonoBehaviour
{
    private Dictionary<Guid, Player> players = new Dictionary<Guid, Player>();

    public GameObject playerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        DataEventManager.StartListening(DataEventType.Register_Player, RegisterPlayer);
        DataEventManager.StartListening(DataEventType.Network_Input_Event, ReceivePlayerInput);
    }

    private void OnDestroy()
    {
        DataEventManager.StopListening(DataEventType.Register_Player, RegisterPlayer);
        DataEventManager.StopListening(DataEventType.Network_Input_Event, ReceivePlayerInput);

    }

    void RegisterPlayer(object guid)
    {
        Guid playerGuid = (Guid)guid;
        GameObject player = Instantiate(playerPrefab);
        Player playerObj = player.GetComponent<Player>();

        players.Add(playerGuid, playerObj);
    }

    void ReceivePlayerInput(object data)
    {
        PlayerInputData inputData = (PlayerInputData)data;

        players[inputData.playerGuid].ReceiveInput(inputData.type, inputData.data);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
