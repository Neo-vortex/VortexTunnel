using System.Net.Sockets;
using VortexTunnel.Enums;

namespace VortexTunnel.Extensions;

public static class SocketExtentions
{
    public static VortexTunnel UpgradeToVortexTunnelAsClient
    (this Socket socket , EncryptionType encryptionType)
    
    {
        var result = new VortexTunnel(socket, encryptionType, SocketRule.CLIENT);
        
        return result;
    }
    public static VortexTunnel UpgradeToVortexTunnelAsServer
        (this Socket socket , EncryptionType encryptionType)
    {
        var result =  new VortexTunnel(socket, encryptionType, SocketRule.SERVER);


        return result;
    }
}