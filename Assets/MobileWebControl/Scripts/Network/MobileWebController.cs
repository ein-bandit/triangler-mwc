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
    public class MobileWebController
    {
        public OutputDataSendMode outputDataSendMode;

        public string webServerAddress
        {
            get
            {
                return webServer.GetPublicIPAddress();
            }
        }

        private INetworkDataInterpreter interpreter;

        private IWebRTCServer webRTCServer;

        private IWebServer webServer;


        private static MobileWebController instance;
        public static MobileWebController Instance
        {
            get
            {
                if (instance == null)
                {
                    UnityEngine.Debug.Log("creating new instance");
                    instance = new MobileWebController();
                }
                return instance;
            }
        }

        private bool isAlive = true;

        public void Initialize(IWebServer httpServer,
                                IWebRTCServer webRTCServer,
                                INetworkDataInterpreter networkDataInterpreter)
        {
            this.webServer = httpServer;
            this.webRTCServer = webRTCServer;
            this.interpreter = networkDataInterpreter;
        }

        public void OnReceiveStringData(IComparable identifier, string message)
        {
            PassReceivedMessage(
                NetworkEventType.Network_Input_Event,
                interpreter.InterpretInputDataFromText(identifier, message)
            );
        }

        public void OnReceiveBinaryData(IComparable identifier, byte[] message)
        {
            PassReceivedMessage(
                NetworkEventType.Network_Input_Event,
                interpreter.InterpretInputDataFromBytes(identifier, message)
            );
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

        private void PassReceivedMessage(NetworkEventType eventType, InputDataHolder data)
        {
            if (!isAlive) return;
            NetworkEventDispatcher.TriggerEvent(eventType, data);
        }

        public void SendMessageToClient(IComparable identifier, OutputDataType type, object data)
        {
            if (outputDataSendMode == OutputDataSendMode.text)
            {
                webRTCServer.SendWebRTCMessage(identifier, convertOutputToString(type, data));
            }
            else if (outputDataSendMode == OutputDataSendMode.bytes)
            {
                webRTCServer.SendWebRTCMessage(identifier, convertOutputToBytes(type, data));
            }
        }

        private string convertOutputToString(OutputDataType type, object data)
        {
            return interpreter.ConvertOutputDataToText(type, data);
        }

        private byte[] convertOutputToBytes(OutputDataType type, object data)
        {
            return interpreter.ConvertOutputDataToBytes(type, data);
        }

        public void Cleanup()
        {
            isAlive = false;
            NetworkEventDispatcher.ClearEventDictionary();

            Debug.Log("shutting down.");
            webRTCServer.CloseConnection();
            webServer.CloseConnection();
        }
    }
}