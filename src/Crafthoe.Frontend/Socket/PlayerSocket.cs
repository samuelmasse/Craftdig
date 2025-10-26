namespace Crafthoe.Frontend;

[Player]
public record class PlayerSocket(Socket Raw, NetSocket Socket, string Host, int Port);
