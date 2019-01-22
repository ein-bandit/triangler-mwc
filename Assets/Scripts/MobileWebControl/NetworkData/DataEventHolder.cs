
namespace MobileWebControl.NetworkData
{
    public struct DataEventHolder
    {
        public NetworkEventType type;
        public DataHolder data;

        public DataEventHolder(NetworkEventType type, DataHolder data)
        {
            this.type = type;
            this.data = data;
        }
    }
}