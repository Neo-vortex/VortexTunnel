using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using VortexTunnel.Enums;
using VortexTunnel.Extensions;


var x = new Socket( AddressFamily.InterNetwork , socketType: SocketType.Stream , protocolType: ProtocolType.Tcp);
x.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444));
x.Listen(1);
var re = await x.AcceptAsync();
var y = re.UpgradeToVortexTunnelAsServer(EncryptionType.AES);

await y.InitHandshake();

var reader = y.MessageFlow;
var i = 0;
var watch = new Stopwatch();
while (await reader.WaitToReadAsync())
{
    watch.Start();
    while (reader.TryRead(out var message))
    {
        i++;
        if (i == 499999)
        {
            Console.WriteLine(watch.Elapsed.Milliseconds / 1000.0);
        }
    }
}


while (true)
{
    Thread.Sleep(100);
}