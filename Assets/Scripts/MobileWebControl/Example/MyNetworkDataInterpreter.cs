using System;
using LitJson;
using MobileWebControl.NetworkData;
using MobileWebControl.NetworkData.InputData;
using UnityEngine;

//you can use your own enum types as well, just return the enum element.
public class MyNetworkDataInterpreter : INetworkDataInterpreter
{
    private struct InputData
    {
        public Enum type;
        public object data;

        public InputData(Enum type, object data)
        {
            this.type = type;
            this.data = data;
        }
    }
    public DataHolder InterpretByteData(Guid guid, byte[] bytes)
    {
        //convert bytes to string.
        InputData p = ParseAndDistributeData(bytes);
        return new DataHolder(guid, p.type, p.data);
    }

    public DataHolder InterpretStringData(Guid guid, string message)
    {
        InputData p = ParseMessage(message);
        return new DataHolder(guid, p.type, p.data);
    }

    public DataHolder RegisterClient(Guid guid)
    {
        //with type register and guid as data.
        return new DataHolder(guid, InputDataType.register_player, guid);
    }

    public DataHolder UnregisterClient(Guid guid)
    {
        //with type unregister and guid as data.
        return new DataHolder(guid, InputDataType.unregister_player, guid);
    }

    #region interpret-data
    private InputData ParseMessage(string message)
    {
        JsonData jsonMesssage = JsonMapper.ToObject(message);
        System.Enum type = ParseMessageType(jsonMesssage["type"]);

        object data = ParseMessageData(type, jsonMesssage["data"]);

        return new InputData(type, data);
    }

    private Enum ParseMessageType(JsonData type)
    {
        if (!type.IsString || type.Count == 0)
        {
            return InputDataType.invalid;
        }

        InputDataType defaultType;
        if (Enum.TryParse(type.ToString(), out defaultType))
        {
            return defaultType;
        }

        MySpecialInputData customType;
        if (Enum.TryParse(type.ToString(), out customType))
        {
            return customType;
        }

        return InputDataType.invalid;
    }

    private object ParseMessageData(Enum type, JsonData data)
    {
        if (!data.IsObject || data.Count == 0)
        {
            return null;
        }

        switch (type)
        {
            case InputDataType.accelerometer:
                return new Vector3(float.Parse(data["a"].ToString()) * -1,
                                    float.Parse(data["b"].ToString()) * -1,
                                    float.Parse(data["c"].ToString()));
            default:
                return null;
        }
    }

    private InputData ParseAndDistributeData(byte[] bytes)
    {
        return new InputData(InputDataType.invalid, null);
    }

    #endregion
}