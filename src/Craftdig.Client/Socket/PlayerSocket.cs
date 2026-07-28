namespace Craftdig.Client;

[Player]
public class PlayerSocket(Log log, TcpClient tcp, Stream stream) : NetSocket(log, tcp, stream);
