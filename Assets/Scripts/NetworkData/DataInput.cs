public class DataInput
{
    public InputDataType dataType;
    public object data;

    public DataInput(InputDataType type, object data)
    {
        this.dataType = type;
        this.data = data;
    }
}