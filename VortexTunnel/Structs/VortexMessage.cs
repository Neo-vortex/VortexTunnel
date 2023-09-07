using VortexTunnel.Enums;

namespace VortexTunnel.Structs;

public struct VortexMessage
{
    public VortexMessage(byte[] data, byte flags)
    {
        Data = data;
        Flags = flags;
    }

    public byte[] Data { init; get; }
    public byte Flags { init; get; }
    
    // to serilize the vortex message to byte first we create an array of bytes  write flag as byte then the whole data
    public unsafe byte[] ToBytes()
    {
        return Data;
    }

    public static unsafe VortexMessage FromBytes(byte[] rawData, byte flags)
    {
            return new VortexMessage(rawData, flags);
    }
    
}