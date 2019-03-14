using System;

namespace MobileWebControl
{
    public interface IWebRTCServer
    {
        void SendWebRTCMessage(IComparable identifier, string message);
        void SendWebRTCMessage(IComparable identifier, byte[] message);
        void CloseConnection();
    }
}
