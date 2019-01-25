using System;
using LitJson;
using MobileWebControl.NetworkData;
using MobileWebControl.NetworkData.InputData;
using UnityEngine;

//you can use your own enum types as well, just return the enum element.
public class MyNetworkDataInterpreter : INetworkDataInterpreter
{
    public DataHolder InterpretByteData(Guid guid, byte[] bytes)
    {
        InputDataHolder p = ParseAndDistributeData(guid, bytes);
        return new DataHolder(guid, p.type, p.data);
    }

    public DataHolder InterpretStringData(Guid guid, string message)
    {
        InputDataHolder p = ParseAndDistributeData(guid, message);
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
    private InputDataHolder ParseAndDistributeData(Guid id, string message)
    {

        JsonData parsedMessage = JsonMapper.ToObject(message);
        InputDataType defaultType = InputDataType.invalid;
        MySpecialInputData customType = MySpecialInputData.invalid;

        object data = null;
        bool success = Enum.TryParse(parsedMessage["type"].ToString(), out defaultType);

        if (success)
        {
            data = ParseDataByType(defaultType, parsedMessage["data"]);
            return new InputDataHolder(id, defaultType, data);
        }
        else
        {
            //either parse again:
            //bool success = Enum.TryParse(parsedMessage["type"].ToString(), out customType);
            Debug.Log("received invalid data type");
            data = "this is a test";
            customType = MySpecialInputData.text;
            return new InputDataHolder(id, customType, data);
        }
    }

    private InputDataHolder ParseAndDistributeData(Guid id, byte[] bytes)
    {
        return new InputDataHolder(id, InputDataType.invalid, null);
    }

    private object ParseDataByType(InputDataType type, JsonData data)
    {
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

    #endregion
}