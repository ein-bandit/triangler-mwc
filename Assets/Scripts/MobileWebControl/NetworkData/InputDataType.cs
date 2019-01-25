using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MobileWebControl.NetworkData.InputData
{
    public enum InputDataType
    {
        accelerometer,
        lightsensor,
        camera,
        microfon,
        button_click,
        text,

        register_player,
        unregister_player,

        invalid
    }

}