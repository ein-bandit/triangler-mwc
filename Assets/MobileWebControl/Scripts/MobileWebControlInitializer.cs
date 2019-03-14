using System.Collections;
using System.Collections.Generic;
using System.IO;
using MobileWebControl;
using MobileWebControl.Network;
using MobileWebControl.Network.Data;
using MobileWebControl.Network.WebRTC;
using MobileWebControl.Network.WebServer;
using UnityEngine;

namespace MobileWebControl
{
    public abstract class MobileWebControlInitializer : MonoBehaviour
    {
        public int httpServerPort = 8880;
        public string webResourcesFolder;
        public int webRTCServerPort = 7770;
        private readonly string defaultFolder = "MobileWebControl/WebResources";

        private static MobileWebControlInitializer instance = null;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }

        protected void InitializeMobileWebControl(INetworkDataInterpreter networkDataInterpreter)
        {
            Debug.Log("No Webserver set - using default implementation");
            IWebServer webServer = new SimpleHTTPServer(
                GetFullPath(webResourcesFolder),
                GetFullPath(defaultFolder),
                httpServerPort);
            Debug.Log("No WebRTC Server set - using default implementation");

            IWebRTCServer webRTCServer = new WebRTCServer(webRTCServerPort);

            MobileWebController.Instance.Initialize(webServer, webRTCServer, networkDataInterpreter);
        }

        private string GetFullPath(string relativePath)
        {
            return Path.Combine(Application.dataPath, relativePath);
        }

        private void OnApplicationQuit()
        {
            MobileWebController.Instance.Cleanup();
        }
    }
}