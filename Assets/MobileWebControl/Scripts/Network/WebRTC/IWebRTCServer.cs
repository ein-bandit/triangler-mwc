﻿using System;

namespace MobileWebControl.Network.WebRTC
{
    public interface IWebRTCServer
    {
        void SendWebRTCMessage(IComparable identifier, string message);
        void CloseConnection();
    }
}
