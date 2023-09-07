using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using VortexTunnel.Consts;
using VortexTunnel.Enums;
using VortexTunnel.Structs;

namespace VortexTunnel;

public sealed partial class VortexTunnel
{

    private async void RawByteReader()
    {
        try
        {
            while (_receiveChannel.Reader.Completion.IsCompleted == false)
            {
                var buffer = _pipe.Writer.GetMemory();
                var byteReadCount = await _socket.ReceiveAsync(buffer, SocketFlags.None);
                if (byteReadCount == 0) return;
                _pipe.Writer.Advance(byteReadCount);
                await _pipe.Writer.FlushAsync();
            }
        }
        catch (Exception e)
        {
            _logCallback?.Invoke($"VortexTunnel Error :  {e.Message}");
            throw;
        }
    }
    private  async void MessageReader()
    {
        try
        {
            while (_receiveChannel.Reader.Completion.IsCompleted == false)
            {
                var result = await ReadOneRawMessage();
                await _receiveChannel.Writer.WriteAsync(VortexMessage.FromBytes(result.Item2.ToArray(), result.Item3));
                _pipe.Reader.AdvanceTo( result.sequencePosition);

            }
        }
        catch (Exception e)
        {
            _logCallback?.Invoke($"VortexTunnel Error :  {e.Message}");
            throw;
        }
    }
    private async Task<VortexMessage> ReadOneMessage()
    {
        return await _receiveChannel.Reader.ReadAsync();
    }

    
    private async Task<(ReadResult result, ReadOnlySequence<byte> buffer, byte flag , SequencePosition sequencePosition)> ReadOneRawMessage()
    {
        var result = await _pipe.Reader.ReadAtLeastAsync(Consts.Consts.HEADER_SIZE);
        var headerSlice = Decouple(result, out var messageSize, out var flag);
        _pipe.Reader.AdvanceTo(headerSlice.End);
        result = await _pipe.Reader.ReadAtLeastAsync(messageSize);
        var buffer = await SecurityTransformDecrypt(result.Buffer.Slice(0, messageSize), flag);
        return (result, buffer, flag ,result.Buffer.Slice(0, messageSize).End );
    }

    private static ReadOnlySequence<byte> Decouple(ReadResult result, out int messageSize, out byte flag)
    {
            var buffer = result.Buffer;
            var headerSlice = buffer.Slice(0, Consts.Consts.HEADER_SIZE);
            messageSize = MemoryMarshal.Read<int>(headerSlice.ToArray());
            flag = headerSlice.Slice(sizeof(int)).First.Span[0];
            return headerSlice;
    }


    private async Task<ReadOnlySequence<byte>> SecurityTransformDecrypt(ReadOnlySequence<byte> slice, byte flag)
    {
        if (flag != (byte)Flags.NORMAL) return slice;
        
        return AesGcmEncryption.Decrypt(slice, _sharedKey);
    }

}