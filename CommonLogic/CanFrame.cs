namespace CommonLogic;

public struct CanFrame
{
    public Bus Bus { get; set; }
    public uint CanId { get; set; }
    public byte[] Value { get; set; }

    public override string ToString()
    {
        return "BusID: " + (byte)Bus + " :: ID :: " + CanId + ":: Value :: " + BitConverter.ToString(Value);
    }
}

public enum Bus : byte
{
    canAero,
    canOpen,
    metaBus
}