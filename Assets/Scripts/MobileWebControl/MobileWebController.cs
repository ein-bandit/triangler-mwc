using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using MobileWebControl.NetworkData;
using UnityEngine;
using UnityEngine.UI;
using MobileWebControl.WebRTC;
using MobileWebControl.Webserver;
using System.IO;

namespace MobileWebControl
{
    //put this script on a gameobject that is a singleton or has a class as singleton already.
    public class MobileWebController : MonoBehaviour
    {
        private INetworkDataInterpreter interpreter;

        public string InterpreterClassName;

        //holds temporarily received events (by other thread of webrtc) until update collects and distributes events.
        private List<DataEventHolder> dataEvents;

        private WebRTCServer wss;
        public int webRTCPort = 1234;

        private SimpleHTTPServer webserver;
        public int webserverPort = 8880;

        void Awake()
        {


            webserver = new SimpleHTTPServer(Path.Combine(Application.streamingAssetsPath, "WebResources"), webserverPort);
            Debug.Log($"opened webserver on port {webserverPort}.");
            //Debug.Log($"serving files from: {Application.streamingAssetsPath}");
            wss = new WebRTCServer(webRTCPort);
            Debug.Log($"created websocket on port {webRTCPort}.");

            wss.OnReceiveDataMessage += OnReceiveData;
            wss.OnRegisterClient += OnRegisterClient;
            wss.OnUnregisterClient += OnUnregisterClient;

            dataEvents = new List<DataEventHolder>();

            if (InterpreterClassName.Length == 0)
            {
                throw new Exception("You have not set an interpreter");
            }
            else
            {
                var type = Type.GetType(InterpreterClassName);
                interpreter = (INetworkDataInterpreter)Activator.CreateInstance(type);
            }
        }

        public void OnReceiveData(Guid guid, string msg)
        {
            //UnityEngine.Debug.Log($"received data {guid},{msg}");

            DataHolder data = interpreter.InterpretStringData(guid, msg);

            DataEventHolder dataEvent =
            new DataEventHolder(NetworkEventType.Network_Input_Event, data);

            dataEvents.Add(dataEvent);
        }

        public void OnRegisterClient(Guid guid)
        {
            Debug.Log("received a new player: " + guid);
            DataHolder data = interpreter.RegisterClient(guid);

            DataEventHolder dataEvent =
                new DataEventHolder(NetworkEventType.Register_Player, data);

            dataEvents.Add(dataEvent);
        }

        public void OnUnregisterClient(Guid guid)
        {
            Debug.Log("unregister player: " + guid);
            DataHolder data = interpreter.UnregisterClient(guid);

            DataEventHolder dataEvent =
                new DataEventHolder(NetworkEventType.Unregister_Player, data);

            dataEvents.Add(dataEvent);
        }

        //use update over fixedupdate to send data as soon as possible.
        //TODO: consider sending data instantly on receive (instead of update) and use update to set data in event receiver directly.
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
        }

        private void OnApplicationQuit()
        {
            Debug.Log("quitting");
            wss.Dispose();
        }


    }
}