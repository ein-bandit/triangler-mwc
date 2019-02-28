using System;

namespace MobileWebControl.NetworkData
{
    //identifier (usually guid, but can be any comparable type)
    public interface INetworkDataInterpreter
    {
        DataHolder InterpretInputDataFromText(IComparable identifier, string msg);
        DataHolder InterpretInputDataFromBytes(IComparable identifier, byte[] bytes);

        DataHolder RegisterClient(IComparable identifier);
        DataHolder UnregisterClient(IComparable identifier);

        string ConvertOutputDataToText(Enum outputDataType, object outputData);
        byte[] ConvertOutputDataToBytes(Enum outputDataType, object outputData);

    }
}