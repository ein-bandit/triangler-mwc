using System;

namespace MobileWebControl.NetworkData
{
    public class DataHolder
    {
        public Guid receiver;

        public System.Enum type;

        public object data;

        public DataHolder(Guid receiver, System.Enum type, object data)
        {
            this.receiver = receiver;
            this.type = type;
            this.data = data;
        }
    }
}