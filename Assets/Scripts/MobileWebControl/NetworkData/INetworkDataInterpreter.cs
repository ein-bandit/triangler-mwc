using System;

namespace MobileWebControl.NetworkData
{
    //identifier (usually guid, but can be any comparable type)
    public interface INetworkDataInterpreter
    {
        DataHolder InterpretStringData(IComparable identifier, string msg);
        DataHolder InterpretByteData(IComparable identifier, byte[] bytes);

        DataHolder RegisterClient(IComparable identifier);
        DataHolder UnregisterClient(IComparable identifier);

    }
}