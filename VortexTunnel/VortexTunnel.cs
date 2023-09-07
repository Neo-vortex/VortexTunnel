using System.IO.Pipelines;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Channels;
using VortexTunnel.Consts;
using VortexTunnel.Enums;
using VortexTunnel.Structs;

namespace VortexTunnel;

public sealed partial class VortexTunnel
{
    private readonly EncryptionType _encryptionType;
    private readonly Action<string>? _logCallback;
    private readonly Socket _socket;
    private ECDiffieHellmanOpenSsl _diffieHellman;
    private bool _isHandshakeComplete;
    private Pipe _pipe;
    private Channel<VortexMessage> _receiveChannel;
    private Channel<VortexMessage> _sendChannel;
    private byte[] _sharedKey;
    private readonly SocketRule _socketRule;

    public VortexTunnel(Socket socket, EncryptionType encryptionType, SocketRule socketRule,
        Action<string> logCallback = default)
    {
        if (socket.ProtocolType != ProtocolType.Tcp) throw new Exception("VortexTunnel only supports TCP sockets");
        _socket = socket;
        _encryptionType = encryptionType;
        _socketRule = socketRule;
        _logCallback = logCallback;
        InitEncryptionInfrustructure();
    }

    public ChannelReader<VortexMessage> MessageFlow => _receiveChannel.Reader;

    public Task InitHandshake()
    {
        return _socketRule == SocketRule.SERVER ? InitHandshakeAsServer() : InitHandshakeAsClient();
    }


    private void StartServices()
    {
        var writerThread = new Thread(MessageCommitter);
        var readerThread = new Thread(MessageReader);
        var rawReaderThread = new Thread(RawByteReader);
        writerThread.Start();
        readerThread.Start();
        rawReaderThread.Start();
    }

    private void InitialVariables()
    {
        _receiveChannel = Channel.CreateUnbounded<VortexMessage>();
        _sendChannel = Channel.CreateUnbounded<VortexMessage>();
        _pipe = new Pipe();
    }

    private void InitEncryptionInfrustructure()
    {
#if OS_WINDOWS
        _diffieHellmanOpen = new ECDiffieHellmanCng();
#else
        _diffieHellman = new ECDiffieHellmanOpenSsl();
#endif
    }


    public Task Send(byte[] data, CancellationToken cancellationToken = default)
    {
        return QueueMessage(new VortexMessage()
        {
            Data = data,
            Flags = (byte)Flags.NORMAL
        }, cancellationToken);
    }
}