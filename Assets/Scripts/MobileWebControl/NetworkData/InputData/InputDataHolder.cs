using System;

namespace MobileWebControl.NetworkData.InputData
{
    public struct InputDataHolder
    {
        public Guid playerGuid;
        public Enum type;
        public object data;

        public InputDataHolder(Guid guid, Enum type, object data)
        {
            this.playerGuid = guid;
            this.type = type;
            this.data = data;
        }
    }

}