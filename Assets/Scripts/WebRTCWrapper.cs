using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebRTCWrapper : MonoBehaviour
{
    public Text data;
    public GameObject cube;

    public bool startServer = true;

    private string text = "";

    private WebRTCServer wss;

    void Awake()
    {
        if (startServer)
        {
            wss = new WebRTCServer(1234);
            wss.OnCallbackDataMessage += OnReceiveData;
        }

    }

    public void OnReceiveData(string msg)
    {
        UnityEngine.Debug.Log($"received data ${msg}");
        text = msg;
    }

    void Update()
    {
        data.text = text;
    }

    private void OnApplicationQuit()
    {
        Debug.Log("quitting");
        wss.Dispose();
    }
}
