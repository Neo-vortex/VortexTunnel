using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using VortexTunnel.Consts;
using VortexTunnel.Enums;
using VortexTunnel.Structs;

namespace VortexTunnel;

public sealed partial class VortexTunnel
{
    private async Task<bool> PerformSecurityHandshakeAsClient()
    {
        try
        {

            await QueueMessage(new VortexMessage()
            {
                Data = _diffieHellman.ExportSubjectPublicKeyInfo(),
                Flags = (byte)Flags.AUTHENTICATION
            });
            var serverResponse = await ReadOneMessage();
            var serverPublicKey = serverResponse.Data;
#if OS_WINDOWS
        var tmpKey = new ECDiffieHellmanCng();
        tmpKey.ImportSubjectPublicKeyInfo(serverPublicKey, out _);
        _sharedKey = _diffieHellmanOpen.DeriveKeyFromHash(tmpKey.PublicKey, HashAlgorithmName.SHA512);
#else
            var tmpKey = new ECDiffieHellmanOpenSsl();
            tmpKey.ImportSubjectPublicKeyInfo(serverPublicKey, out _);
            _sharedKey = SHA256.HashData(_diffieHellman.DeriveKeyFromHash(tmpKey.PublicKey, HashAlgorithmName.SHA512)) ;
#endif
            return true;
        }
        catch (Exception e)
        {
            _logCallback?.Invoke($"VortexTunnel Error :  {e.Message}");
            return false;
        }
    }
    private async Task<bool> PerformSecurityHandshakeAsServer()
    {

        try
        {
            var clientRequest = await ReadOneMessage();
            if (clientRequest.Flags != (byte)Flags.AUTHENTICATION)
            {
                _logCallback?.Invoke($"VortexTunnel Error :  Client did not send the correct security measures");
                return false;
            }

            await QueueMessage(new VortexMessage()
            {
                Data = _diffieHellman.ExportSubjectPublicKeyInfo(),
                Flags = (byte)Flags.AUTHENTICATION
            });

            var serverPublicKey = clientRequest.Data;
#if OS_WINDOWS
        var tmpKey = new ECDiffieHellmanCng();
        tmpKey.ImportSubjectPublicKeyInfo(serverPublicKey, out _);
        _sharedKey = _diffieHellmanOpen.DeriveKeyFromHash(tmpKey.PublicKey, HashAlgorithmName.SHA512);
#else
            var tmpKey = new ECDiffieHellmanOpenSsl();
            tmpKey.ImportSubjectPublicKeyInfo(serverPublicKey, out _);
            _sharedKey = SHA256.HashData(_diffieHellman.DeriveKeyFromHash(tmpKey.PublicKey, HashAlgorithmName.SHA512)) ;
#endif
            return true;
        }
        catch (Exception e)
        {
            _logCallback?.Invoke($"VortexTunnel Error :  {e.Message}");
            return false;
        }
    }
    private async Task<bool> PerformProtocolHandshakeAsServer()
    {
        try
        {
            var clientRequest = await ReadOneMessage();
            if (clientRequest.Flags !=  (byte) Flags.PROTOCOL)
            {
                _logCallback?.Invoke($"VortexTunnel Error :  Client did not send the correct protocol version");
                return false;
            }
            var clientProtocolVersion = BitConverter.ToInt32(clientRequest.Data);
            if (clientProtocolVersion == Consts.Consts.SUPPORTED_PROTOCOL_VERSIONS[0])
            {
                await QueueMessage(new VortexMessage()
                {
                    Data = BitConverter.GetBytes(Consts.Consts.SUPPORTED_PROTOCOL_VERSIONS[0]),
                    Flags = (byte)Flags.PROTOCOL
                });
                return true;
            }

            _logCallback?.Invoke($"VortexTunnel Error :   This version protocol is not supported : {clientProtocolVersion}");
            return false;
        }
        catch (Exception e)
        {
            _logCallback?.Invoke($"VortexTunnel Error :  {e.Message}");
            return false;
        }
    }
    private async Task<bool> PerformProtocolHandshakeAsClient()
    {
        try
        {
            await QueueMessage(new VortexMessage()
            {
                Data = BitConverter.GetBytes(Consts.Consts.SUPPORTED_PROTOCOL_VERSIONS[0]),
                Flags = (byte)Flags.PROTOCOL
            });
            var serverResponse = await ReadOneMessage();
            var serverProtocolVersion = BitConverter.ToInt32(serverResponse.Data);
            if (serverProtocolVersion == Consts.Consts.SUPPORTED_PROTOCOL_VERSIONS[0]) return true;

            _logCallback?.Invoke($"VortexTunnel Error :  Server does not support the protocol version");
            return false;
        }
        catch (Exception e)
        {
            _logCallback?.Invoke($"VortexTunnel Error :  {e.Message}");
            return false;
        }
    }
    private Task InitHandshakeAsClient()
    {
        return Task.Run(async () =>
        {
            try
            {
                InitialVariables();
                StartServices();
                var securityHandshakeResult = await PerformSecurityHandshakeAsClient();
                if (!securityHandshakeResult)
                {
                    throw new Exception("Security handshake failed");
                }
                var protocolHandshakeResult = await PerformProtocolHandshakeAsClient();
                if (!protocolHandshakeResult)
                {
                    throw new Exception("Protocol handshake failed");
                }
                _isHandshakeComplete = true;
            }
            catch (Exception e)
            {
                _logCallback?.Invoke($"VortexTunnel Error :  {e.Message}");
                throw;
            }
        });
       
    }
    private Task InitHandshakeAsServer()
    {
        return Task.Run(async () =>
        {
            try
            {
                InitialVariables();
                StartServices();
                var securityHandshakeResult =  await PerformSecurityHandshakeAsServer();
                if (!securityHandshakeResult)
                {
                    throw new Exception("Security handshake failed");
                }
                var protocolHandshakeResult =  await PerformProtocolHandshakeAsServer();
                if (!protocolHandshakeResult)
                {
                    throw new Exception("Protocol handshake failed");
                }
                _isHandshakeComplete = true;
            }
            catch (Exception e)
            {
                _logCallback?.Invoke($"VortexTunnel Error :  {e.Message}");
                throw;
            }
        });
    }
}