namespace Crafthoe.Client;

[Player]
public class PlayerSocket(TcpClient tcp, SslStream ssl) : NetSocket(tcp, ssl);
