using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMobileWebControlInitializer : MobileWebControlInitializer
{
    private void Start()
    {
        //use default ports - override in inspector possible.
        this.Initialize();
    }
}
