using System;
using MobileWebControl.Network.Input;

namespace MobileWebControl.Network
{
    //identifier (usually guid, but can be any comparable type)
    public interface INetworkDataInterpreter
    {
        InputDataHolder InterpretInputDataFromText(IComparable identifier, string msg);
        InputDataHolder InterpretInputDataFromBytes(IComparable identifier, byte[] bytes);

        InputDataHolder RegisterClient(IComparable identifier);
        InputDataHolder UnregisterClient(IComparable identifier);

        string ConvertOutputDataToText(Enum outputDataType, object outputData);
        byte[] ConvertOutputDataToBytes(Enum outputDataType, object outputData);

    }
}