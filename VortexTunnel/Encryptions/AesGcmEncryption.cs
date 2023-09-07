using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class AesGcmEncryption
{
    public static byte[] Encrypt(byte[] plainData , byte[] key)
    {
    
        var nonceSize = AesGcm.NonceByteSizes.MaxSize;
        var tagSize = AesGcm.TagByteSizes.MaxSize;
        var cipherSize = plainData.Length;
    
        // We write everything into one big array for easier encoding
        var encryptedDataLength = 4 + nonceSize + 4 + tagSize + cipherSize;
        var encryptedData = encryptedDataLength < 1024
            ? stackalloc byte[encryptedDataLength]
            : new byte[encryptedDataLength].AsSpan();
    
        // Copy parameters
        BinaryPrimitives.WriteInt32LittleEndian(encryptedData[..4], nonceSize);
        BinaryPrimitives.WriteInt32LittleEndian(encryptedData.Slice(4 + nonceSize, 4), tagSize);
        var nonce = encryptedData.Slice(4, nonceSize);
        var tag = encryptedData.Slice(4 + nonceSize + 4, tagSize);
        var cipherBytes = encryptedData.Slice(4 + nonceSize + 4 + tagSize, cipherSize);
    
        // Generate secure nonce
        RandomNumberGenerator.Fill(nonce);
    
        // Encrypt
        using var aes = new AesGcm(key);
        aes.Encrypt(nonce, plainData.AsSpan(), cipherBytes, tag);
    
        // Encode for transmission
        return (encryptedData.ToArray());
    }
    
    
    public static ReadOnlySequence<byte> Decrypt(System.Buffers.ReadOnlySequence<byte> cipherData , byte[] key)
    {
        Span<byte> encryptedData = cipherData.ToArray();
        // Extract parameter sizes
        var nonceSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData[..4]);
        var tagSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData.Slice(4 + nonceSize, 4));
        var cipherSize = encryptedData.Length - 4 - nonceSize - 4 - tagSize;
    
        // Extract parameters
        var nonce = encryptedData.Slice(4, nonceSize);
        var tag = encryptedData.Slice(4 + nonceSize + 4, tagSize);
        var cipherBytes = encryptedData.Slice(4 + nonceSize + 4 + tagSize, cipherSize);
    
        // Decrypt
        var plainBytes = new byte[cipherSize];
        using var aes = new AesGcm(key);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
    
        // Convert plain bytes back into string
        return new ReadOnlySequence<byte>(plainBytes);
    }
}