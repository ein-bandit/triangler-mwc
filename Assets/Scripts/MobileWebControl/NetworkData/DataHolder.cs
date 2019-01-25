using System;

namespace MobileWebControl.NetworkData
{
    public class DataHolder
    {
        public IComparable identifier;

        public System.Enum type;

        public object data;

        public DataHolder(IComparable identifier, System.Enum type, object data)
        {
            this.identifier = identifier;
            this.type = type;
            this.data = data;
        }
    }
}