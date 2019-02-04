using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MobileWebControl.NetworkData.InputData
{
    public enum InputDataType
    {
        accelerometer,
        lightsensor,
        proximity,
        tap,

        register,
        unregister,

        invalid,

        //maybe added later
        camera,
        microfon
    }

}