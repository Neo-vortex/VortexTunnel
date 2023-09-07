using System.Net.Sockets;
using VortexTunnel.Consts;
using VortexTunnel.Enums;
using VortexTunnel.Structs;

namespace VortexTunnel;

public sealed partial class VortexTunnel
{
    private Task QueueMessage(VortexMessage vortexMessage, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                return _sendChannel.Writer.WriteAsync(vortexMessage, cancellationToken);
            }
            catch (Exception e)
            {
                _logCallback?.Invoke($"VortexTunnel Error :  {e.Message}");
                throw;
            }
        }, cancellationToken);
     
    }
    private async void MessageCommitter()
    {
        try
        {
            while (_sendChannel.Reader.Completion.IsCompleted == false)
            {
                var message = await _sendChannel.Reader.ReadAsync();
                var combinedBuffer = await EncapsulateMessage(message);
                await _socket.SendAsync(combinedBuffer, SocketFlags.None);
            }
        }
        catch (Exception e)
        {
            _logCallback?.Invoke($"VortexTunnel Error :  {e.Message}");
            throw;
        }
    }

    private async Task<byte[]> EncapsulateMessage(VortexMessage message)
    {
        var wireMessage = await SecurityTransformEncrypt(message.ToBytes(), message.Flags);
        var wireMessageSizeBytes = BitConverter.GetBytes(wireMessage.Length);
        var totalSize = wireMessageSizeBytes.Length + sizeof(byte) + wireMessage.Length;
        var combinedBuffer = new byte[totalSize];
        Buffer.BlockCopy(wireMessageSizeBytes, 0, combinedBuffer, 0, wireMessageSizeBytes.Length);
        combinedBuffer[wireMessageSizeBytes.Length] = message.Flags;
        Buffer.BlockCopy(wireMessage, 0, combinedBuffer, wireMessageSizeBytes.Length + 1, wireMessage.Length);
        return combinedBuffer;
    }

    private async Task<byte[]> SecurityTransformEncrypt(byte[] data, byte flag)
    {
        if (flag != (byte)Flags.NORMAL) return data;

        return AesGcmEncryption.Encrypt(data, _sharedKey);
    }
}