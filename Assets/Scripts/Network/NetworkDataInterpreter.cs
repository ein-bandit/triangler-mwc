using System;
using System.Collections.Generic;
using System.Text;
using LitJson;
using UnityEngine;
using UnityToolbag;
using UnityWebRTCControl.Network.Data;

//you can use your own enum types as well, just return the enum element.
//identifier is Guid for this example (available from library)
public class NetworkDataInterpreter : INetworkDataInterpreter
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

    private const String dataTypeKey = "type";
    private const String dataObjectKey = "data";

    public InputDataHolder InterpretInputDataFromJson(IComparable identifier, string message)
    {
        InputData p = ParseMessage(message);
        return new InputDataHolder(identifier, p.type, p.data);
    }

    public InputDataHolder RegisterClient(IComparable identifier)
    {
        //with type register and guid as data.
        return new InputDataHolder(identifier, InputDataType.register, identifier);
    }

    public InputDataHolder UnregisterClient(IComparable identifier)
    {
        //with type unregister and guid as data.
        return new InputDataHolder(identifier, InputDataType.unregister, identifier);
    }

    #region interpret-data
    private InputData ParseMessage(string message)
    {
        JsonData jsonMessage = JsonMapper.ToObject(message);
        try
        {
            Enum type = ParseMessageType(jsonMessage[dataTypeKey]);
            object data = ParseMessageData(type, jsonMessage[dataObjectKey]);
            return new InputData(type, data);
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            //inform via unity console that jsonMessage could not be parsed due to not found key.
            //maybe the dispatcher is not needed here, event though i am in another thread.
            Dispatcher.InvokeAsync(() =>
            {
                Debug.LogError($"At least one of the given keys [{dataTypeKey},{dataObjectKey}] was not found in json data. {keyNotFoundException}, {jsonMessage}");
            });
        }
        catch (Exception exception)
        {
            Dispatcher.InvokeAsync(() =>
            {
                Debug.LogError($"error while parsing data. {exception}");
            });
        }
        return new InputData(InputDataType.invalid, null);
    }

    private Enum ParseMessageType(JsonData type)
    {
        if (!type.IsString || type.ToString().Length == 0)
        {
            return InputDataType.invalid;
        }

        InputDataType defaultType;
        if (Enum.TryParse(type.ToString(), out defaultType))
        {
            return defaultType;
        }

        SpecialInputData customType;
        if (Enum.TryParse(type.ToString(), out customType))
        {
            return customType;
        }

        return InputDataType.invalid;
    }

    private object ParseMessageData(Enum type, JsonData data)
    {
        if (data.ToString().Length == 0)
        {
            return null;
        }

        switch (type)
        {
            case InputDataType.orientation:
                return new Vector3(float.Parse(data["a"].ToString()),
                                    float.Parse(data["b"].ToString()),
                                    float.Parse(data["c"].ToString()));
            case InputDataType.tap:
                return data.ToString();
            case InputDataType.proximity:
                return Boolean.Parse(data.ToString());
            default:
                return null;
        }
    }

    public string ConvertOutputDataToJson(Enum outputDataType, object outputData)
    {
        return CreateJsonOutput(outputDataType, outputData).ToJson();
    }

    private JsonData CreateJsonOutput(Enum outputDataType, object outputData)
    {
        Debug.Log($"sending {JsonMapper.ToJson(new OutputDataHolder(outputDataType, outputData))}");
        return JsonMapper.ToJson(new OutputDataHolder(outputDataType, outputData));
    }

    public class OutputDataHolder
    {
        public string type;
        public object data;
        public OutputDataHolder(Enum type, object data)
        {
            this.type = Enum.GetName(typeof(OutputDataType), type);
            this.data = data;
        }
    }

    #endregion
}