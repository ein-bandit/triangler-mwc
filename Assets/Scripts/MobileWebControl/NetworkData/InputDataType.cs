using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MobileWebControl.NetworkData.InputData
{
    public enum InputDataType
    {
        orientation,
        lightsensor,
        proximity,
        motion,
        tap,

        register,
        unregister,
        ready,

        invalid,

        //maybe added later
        camera,
        microfon
    }

}