namespace Crafthoe.Client;

[Player]
public class PlayerSocket(TcpClient tcp, Stream stream) : NetSocket(tcp, stream);
