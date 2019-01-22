using System;

public struct PlayerInputData
{
    public Guid playerGuid;
    public InputDataType type;
    public object data;

    public PlayerInputData(Guid guid, InputDataType type, object data)
    {
        this.playerGuid = guid;
        this.type = type;
        this.data = data;
    }
}