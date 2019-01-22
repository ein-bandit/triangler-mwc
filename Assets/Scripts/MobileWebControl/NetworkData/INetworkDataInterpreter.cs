using System;

namespace MobileWebControl.NetworkData
{
    public interface INetworkDataInterpreter
    {
        DataHolder InterpretStringData(Guid guid, string msg);
        DataHolder InterpretByteData(Guid guid, byte[] bytes);

        DataHolder RegisterClient(Guid guid);
        DataHolder UnregisterClient(Guid guid);

    }
}