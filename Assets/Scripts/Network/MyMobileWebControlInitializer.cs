using System.Collections;
using System.Collections.Generic;
using MobileWebControl;
using UnityEngine;

public class MyMobileWebControlInitializer : AbstractMobileWebControlInitializer
{
    private void Start()
    {
        //using default webserver and webrtc server.
        //use default ports - override in inspector possible.
        this.InitializeMobileWebControl(new MyNetworkDataInterpreter());
    }
}
