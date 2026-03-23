namespace Craftdig.Server;

[Server]
public class ServerScratchedBagMut : EntIdxBagMut<ServerComponents.IsScratched>;

[Server]
public class ServerScratchedBag(ServerScratchedBagMut bag) : EntIdxBag<ServerComponents.IsScratched>(bag);
