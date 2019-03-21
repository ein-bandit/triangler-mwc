using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWebRTCControl;

public class UWCInitializer : AbstractUWCInitializer
{
    private void Start()
    {
        //using default webserver and webrtc server.
        //use default ports - override in inspector possible.
        this.InitializeUWC(new NetworkDataInterpreter());
    }
}
