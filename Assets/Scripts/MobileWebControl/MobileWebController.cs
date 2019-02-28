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
using MobileWebControl.NetworkData.InputData;

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
        public static int webRTCPort = 1234;

        private static SimpleHTTPServer webserver;
        public static int webserverPort = 8880;

        private bool isAlive = true;

        public OutputDataSendMode outputDataSendMode;

        public static string webServerAddress
        {
            get
            {
                return webserver.PublicIPAddress;
            }
        }

        //private static readonly String WebResourcesLocation = "WebResources";

        public static MobileWebController instance;
        void Awake()
        {
            if (!instance)
            {
                instance = this;
                DontDestroyOnLoad(this);

                Initialize();
            }
            else
            {
                Destroy(this);
            }
        }

        private void Initialize()
        {
            webserver = new SimpleHTTPServer(Path.Combine(Application.streamingAssetsPath, "WebResources"), webserverPort);
            //webserver = new SimpleHTTPServer(Path.Combine(Application.dataPath, WebResourcesLocation), webserverPort);
            Debug.Log($"opened webserver on port {webserverPort}.");
            //Debug.Log($"serving files from: {Application.streamingAssetsPath}");

            webRTCServer = new WebRTCServer(webRTCPort);
            Debug.Log($"created websocket on port {webRTCPort}.");


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

        public void OnReceiveStringData(IComparable identifier, string message)
        {
            if (CheckRetrievedMessage(message))
            {
                PassReceivedMessage(
                    NetworkEventType.Network_Input_Event,
                    interpreter.InterpretInputDataFromText(identifier, message)
                );
            }
        }

        public void OnReceiveBinaryData(IComparable identifier, byte[] message)
        {
            throw new NotImplementedException();
        }

        public void OnRegisterClient(IComparable identifier)
        {
            PassReceivedMessage(
                NetworkEventType.Register_Player,
                interpreter.RegisterClient(identifier)
            );
        }

        public void OnUnregisterClient(IComparable identifier)
        {
            PassReceivedMessage(
                NetworkEventType.Unregister_Player,
                interpreter.UnregisterClient(identifier)
            );
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

        private void PassReceivedMessage(NetworkEventType eventType, DataHolder data)
        {
            if (!isAlive) return;
            NetworkEventDispatcher.TriggerEvent(eventType, data);
        }

        public void SendMessageToClient(IComparable identifier, OutputDataType type, object data)
        {
            if (outputDataSendMode == OutputDataSendMode.text)
            {
                webRTCServer.SendWebRTCMessage(identifier, convertOutputToJSONString(type, data));
            }
            else if (outputDataSendMode == OutputDataSendMode.bytes)
            {
                webRTCServer.SendWebRTCMessage(identifier, convertOutputToBytes(type, data));
            }
        }

        private string convertOutputToJSONString(OutputDataType type, object data)
        {
            return interpreter.ConvertOutputDataToText(type, data);
        }

        private byte[] convertOutputToBytes(OutputDataType type, object data)
        {
            return interpreter.ConvertOutputDataToBytes(type, data);
        }

        private void OnApplicationQuit()
        {
            instance.isAlive = false;
            NetworkEventDispatcher.ClearEventDictionary();

            Debug.Log("shutting down.");
            webRTCServer.Dispose();
            webserver.Stop();
        }
    }
}