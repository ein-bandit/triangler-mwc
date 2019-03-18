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
    public abstract class AbstractMobileWebControlInitializer : MonoBehaviour
    {
        [Header("Default Server Configuration")]
        public int httpServerPort = 8880;
        public string webResourcesFolder;
        public int webRTCServerPort = 7770;

        private readonly string standardResourcesFolder = "MobileWebControl/WebResources";

        private IWebServer webServer;
        private IWebRTCServer webRTCServer;

        private AbstractMobileWebControlInitializer instance = null;
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

        protected void InitializeMobileWebControl(
            INetworkDataInterpreter networkDataInterpreter,
            IWebServer webserver = null,
            IWebRTCServer webRTCServer = null)
        {
            if (webserver == null)
            {
                webServer = new SimpleHTTPServer(
                    GetFullPath(webResourcesFolder),
                    GetFullPath(standardResourcesFolder),
                    httpServerPort);
            }
            if (webRTCServer == null)
            {
                webRTCServer = new WebRTCServer(webRTCServerPort);
            }

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