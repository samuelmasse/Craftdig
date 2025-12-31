namespace Craftdig.Client;

[Player]
public class PlayerSocket(AppLog log, TcpClient tcp, Stream stream) : NetSocket(log, tcp, stream);
