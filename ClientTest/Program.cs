using System.Net;
using System.Net.Sockets;
using VortexTunnel.Enums;
using VortexTunnel.Extensions;

var x = new Socket( AddressFamily.InterNetwork , socketType: SocketType.Stream , protocolType: ProtocolType.Tcp);
x.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444));
var y = x.UpgradeToVortexTunnelAsClient(EncryptionType.AES);
await y.InitHandshake();

for (int i = 0; i < 500000; i++)
{
    await y.Send( System.Text.Encoding.UTF8.GetBytes("hello world"));
}



while (true)
{
    Thread.Sleep(100);
}