using System.Collections;
using System.Collections.Generic;
using System.IO;
using MobileWebControl;
using MobileWebControl.WebRTC;
using MobileWebControl.Webserver;
using UnityEngine;

public abstract class MobileWebControlInitializer : MonoBehaviour
{
    public int httpServerPort = 8880;
    public string webResourcesFolder;
    private readonly string defaultFolder = "MobileWebControl/WebResources";
    public int webRTCServerPort = 7770;

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

    protected void Initialize()
    {
        MobileWebController.Instance.Initialize(
            new SimpleHTTPServer(
                GetFullPath(webResourcesFolder),
                GetFullPath(defaultFolder),
                httpServerPort),
            new WebRTCServer(webRTCServerPort),
            new MyNetworkDataInterpreter());
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
