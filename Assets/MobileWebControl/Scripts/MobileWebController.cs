using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.UI;
using MobileWebControl.WebRTC;
using MobileWebControl.Webserver;
using System.IO;
using MobileWebControl.Network;
using MobileWebControl.Network.Output;
using MobileWebControl.Network.Input;

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

        public OutputDataSendMode outputDataSendMode;
        public string webResourcesFolder;
        private readonly string defaultFolder = "MobileWebControl/WebResources";

        private WebRTCServer webRTCServer;
        public int webRTCPort = 7770;

        private SimpleHTTPServer webserver;
        public int webserverPort = 8880;

        public string webServerAddress
        {
            get
            {
                return instance.webserver.PublicIPAddress;
            }
        }

        public static MobileWebController instance;

        private bool isAlive = true;

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
            webserver = new SimpleHTTPServer(
                GetFullPath(webResourcesFolder),
                GetFullPath(defaultFolder),
                webserverPort
            );
            Debug.Log($"Opened Webserver on port {webserverPort}.");

            webRTCServer = new WebRTCServer(webRTCPort);
            Debug.Log($"Created Websocket for WebRTC signaling on port {webRTCPort}.");


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

        private string GetFullPath(string relativePath)
        {
            return Path.Combine(Application.dataPath, relativePath);
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

        private void PassReceivedMessage(NetworkEventType eventType, InputDataHolder data)
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