using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.UI;

public class WebRTCWrapper : MonoBehaviour
{
    public Text data;

    public bool startServer = true;

    private string text = "";
    //holds temporarily received events (by other thread of webrtc) until update collects and distributes events.
    private List<DataEventHolder> dataEvents;

    private WebRTCServer wss;

    void Awake()
    {
        if (startServer)
        {
            wss = new WebRTCServer(1234);
            wss.OnReceiveDataMessage += OnReceiveData;
            wss.OnRegisterClient += OnRegisterClient;
        }
        dataEvents = new List<DataEventHolder>();
    }

    public void OnReceiveData(Guid guid, string msg)
    {
        //UnityEngine.Debug.Log($"received data {guid},{msg}");

        DataEventHolder dataEvent =
        new DataEventHolder(DataEventType.Network_Input_Event, ParseAndDistributeData(guid, msg));

        dataEvents.Add(dataEvent);
        text = msg;
    }

    public void OnRegisterClient(Guid guid)
    {
        Debug.Log("received a new player: " + guid);
        DataEventHolder dataEvent =
            new DataEventHolder(DataEventType.Register_Player, guid);

        dataEvents.Add(dataEvent);
    }

    void Update()
    {
        if (dataEvents.Count > 0)
        {
            List<DataEventHolder> copiedList = new List<DataEventHolder>(dataEvents);
            dataEvents.Clear();

            Debug.Log($"resending {copiedList.Count} events");
            foreach (DataEventHolder data in copiedList)
            {
                DataEventManager.TriggerEvent(data.type, data.data);
            }
        }

        //handle data
        data.text = text;
    }

    private void OnApplicationQuit()
    {
        Debug.Log("quitting");
        wss.Dispose();
    }

    private PlayerInputData ParseAndDistributeData(Guid id, string message)
    {

        JsonData parsedMessage = JsonMapper.ToObject(message);
        InputDataType dataType = InputDataType.invalid;
        object data = null;
        bool success = Enum.TryParse(parsedMessage["type"].ToString(), out dataType);

        if (success)
        {
            data = ParseDataByType(dataType, parsedMessage["data"]);
            return new PlayerInputData(id, dataType, data);
        }
        else
        {
            Debug.Log("received invalid data type");
            return new PlayerInputData(id, dataType, data);
        }
    }

    private object ParseDataByType(InputDataType type, JsonData data)
    {
        switch (type)
        {
            case InputDataType.accelerometer:
                return new Vector3(float.Parse(data["x"].ToString()),
                                    float.Parse(data["y"].ToString()),
                                    float.Parse(data["z"].ToString()));
            default:
                return null;
        }
    }

    private struct DataEventHolder
    {
        public DataEventType type;
        public object data;

        public DataEventHolder(DataEventType type, object data)
        {
            this.type = type;
            this.data = data;
        }
    }
}
