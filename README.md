# Custom Raw Socket Protocol  (VortexTunnel)
## Introduction
This documentation provides an overview of the Custom Raw Socket Protocol, a high-performance communication protocol designed for secure and efficient data transfer. The protocol combines AES-GCM encryption and Diffie-Hellman key exchange to ensure confidentiality, authenticity, and efficient communication.

## Key Features
Fast and performant communication.
AES-FCM encryption for data confidentiality.
Diffie-Hellman key exchange for secure key generation.
Scalable and efficient design.
Benchmarked performance of handling 500,000 messages in 0.96 seconds on an Intel Core i7-12700 processor.
## Getting Started
### Prerequisites
Before using the Custom Raw Socket Protocol, ensure you have the following prerequisites:

* dotnet 7+

### Installation
just add VortexTunnel project to your target project.

### Usage
Basic Usage
```csharp
var _socket = new Socket( AddressFamily.InterNetwork , socketType: SocketType.Stream , protocolType: ProtocolType.Tcp);
_socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4444));
var vortex_socket = _socket.UpgradeToVortexTunnelAsClient(EncryptionType.AES);
await vortex_socket.InitHandshake();
await vortex_socket.Send( System.Text.Encoding.UTF8.GetBytes("hello world"));
```


## Security Considerations

### Replay Attacks

The Custom Raw Socket Protocol, while designed for high performance and secure communication, is susceptible to replay attacks. A replay attack occurs when an attacker intercepts and retransmits previously captured data packets, potentially causing unintended or malicious actions in the system.

To mitigate the risk of replay attacks, consider implementing additional security measures, such as:

- **Timestamps**: Include timestamps in your protocol to detect and reject old or replayed messages based on the timestamp's freshness.

- **Nonce or Sequence Numbers**: Use nonces or sequence numbers in your communication to ensure that each message is unique, preventing replay attacks.

- **Session Management**: Implement session management and authentication mechanisms to verify the identity of the parties involved in the communication.

It is crucial to design your application to handle and detect replay attacks effectively, especially when dealing with sensitive or critical data. Understanding and addressing this security concern is essential to ensure the protocol's integrity and reliability.

## Benchmark Results
Our benchmarks show impressive performance:

500,000 messages processed in 0.96 seconds on an Intel Core i7-12700 (both client and server on the same machine).


Contributing
The most welcome!

License
MIT



Contact
neo.vortex@pm.me
