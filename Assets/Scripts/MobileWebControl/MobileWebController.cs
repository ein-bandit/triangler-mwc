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

    //is unitys interface to actions on other threads (webserver, webrtcserver).
    //uses an async event system to wait for unitys thread to be ready and dispatch the retrieved network events.

    //put this script on a gameobject that is a singleton ie. has a class as singleton already.
    //starts the server (which is on another thread) and registers callbacks.
    //therefore events are collected and distributed in next update.
    public class MobileWebController : MonoBehaviour
    {
        private INetworkDataInterpreter interpreter;

        public string InterpreterClassName;

        private WebRTCServer webRTCServer;
        public int webRTCPort = 1234;

        private SimpleHTTPServer webserver;
        public int webserverPort = 8880;

        //TODO: implement last event remembering mechanism
        public bool sendLastEvent = false;

        //temporary event store. holds events until unitys th
        private Dictionary<NetworkEventType, DataHolder> tempEventStore;

        private NetworkEventDispatcher networkEventDispatcher;

        void Awake()
        {
            webserver = new SimpleHTTPServer(Path.Combine(Application.streamingAssetsPath, "WebResources"), webserverPort);
            Debug.Log($"opened webserver on port {webserverPort}.");
            //Debug.Log($"serving files from: {Application.streamingAssetsPath}");

            webRTCServer = new WebRTCServer(webRTCPort);
            Debug.Log($"created websocket on port {webRTCPort}.");


            //handler methods get called from another thread.
            webRTCServer.OnReceiveDataMessage += OnReceiveData;
            webRTCServer.OnReceiveBinaryDataMessage += OnReceiveBinaryData;
            webRTCServer.OnRegisterClient += OnRegisterClient;
            webRTCServer.OnUnregisterClient += OnUnregisterClient;

            if (InterpreterClassName.Length == 0)
            {
                throw new Exception("You have not set an interpreter.");
            }
            else
            {
                //try to refactor this to a class reference instead of string.
                var type = Type.GetType(InterpreterClassName);
                interpreter = (INetworkDataInterpreter)Activator.CreateInstance(type);
            }
        }

        public void OnReceiveData(IComparable identifier, string message)
        {
            if (CheckRetrievedMessage(message))
            {
                //UnityEngine.Debug.Log($"received data {identifier},{message}");
                DataHolder data = interpreter.InterpretStringData(identifier, message);

                NetworkEventDispatcher.TriggerEvent(NetworkEventType.Network_Input_Event, data);
            }
        }

        public void OnReceiveBinaryData(IComparable identifier, byte[] message)
        {
            throw new NotImplementedException();
        }

        public void OnRegisterClient(IComparable identifier)
        {
            //Debug.Log("received a new player: " + identifier);
            DataHolder data = interpreter.RegisterClient(identifier);

            NetworkEventDispatcher.TriggerEvent(NetworkEventType.Register_Player, data);
        }

        public void OnUnregisterClient(IComparable identifier)
        {
            //Debug.Log("unregister player: " + identifier);
            DataHolder data = interpreter.UnregisterClient(identifier);

            NetworkEventDispatcher.TriggerEvent(NetworkEventType.Unregister_Player, data);
        }

        private bool CheckRetrievedMessage(string message)
        {
            if (message == null || message.Length < 19)
            {
                Debug.Log($"Failed check for message syntax. ignored received event. {message.Length},{message}");
                return false;
            }
            return true;
        }

        private void OnApplicationQuit()
        {
            Debug.Log("quitting");
            webRTCServer.Dispose();
            webserver.Stop();
        }
    }
}