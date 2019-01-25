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
    //put this script on a gameobject that is a singleton ie. has a class as singleton already.
    public class MobileWebController : MonoBehaviour
    {
        private INetworkDataInterpreter interpreter;

        public string InterpreterClassName;

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
            wss.OnReceiveBinaryDataMessage += OnReceiveBinaryData;
            wss.OnRegisterClient += OnRegisterClient;
            wss.OnUnregisterClient += OnUnregisterClient;

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

        public void OnReceiveData(Guid guid, string message)
        {
            if (CheckRetrievedMessage(message))
            {
                //UnityEngine.Debug.Log($"received data {guid},{message}");
                DataHolder data = interpreter.InterpretStringData(guid, message);

                DataEventManager.TriggerEvent(NetworkEventType.Network_Input_Event, data);
            }
        }

        public void OnReceiveBinaryData(Guid guid, byte[] message)
        {
            throw new NotImplementedException();
        }


        public void OnRegisterClient(Guid guid)
        {
            Debug.Log("received a new player: " + guid);
            DataHolder data = interpreter.RegisterClient(guid);

            DataEventManager.TriggerEvent(NetworkEventType.Register_Player, data);
        }

        public void OnUnregisterClient(Guid guid)
        {
            Debug.Log("unregister player: " + guid);
            DataHolder data = interpreter.UnregisterClient(guid);

            DataEventManager.TriggerEvent(NetworkEventType.Unregister_Player, data);
        }

        private bool CheckRetrievedMessage(string message)
        {
            if (message == null || message.Length < 19)
            {
                Debug.Log("failed check for message syntax. ignored received event.");
                return false;
            }
            return true;
        }

        private void OnApplicationQuit()
        {
            Debug.Log("quitting");
            wss.Dispose();
            webserver.Stop();
        }
    }
}