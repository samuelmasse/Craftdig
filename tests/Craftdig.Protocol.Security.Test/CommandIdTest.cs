namespace Craftdig;

[TestClass]
public sealed class CommandIdTest
{
    [TestMethod]
    public void Commands_V1Ids_AreFrozen()
    {
        (string Name, ushort Id)[] expected =
        [
            (nameof(Commands.CommonStart), 10000),
            (nameof(Commands.Ping), 10001),
            (nameof(Commands.Pong), 10002),
            (nameof(Commands.ServerStart), 20000),
            (nameof(Commands.BeginAuth), 20001),
            (nameof(Commands.CompleteAuth), 20002),
            (nameof(Commands.ResultAuth), 20003),
            (nameof(Commands.NoAuth), 20004),
            (nameof(Commands.SpawnPlayer), 20005),
            (nameof(Commands.MovePlayer), 20006),
            (nameof(Commands.ForgetChunk), 20007),
            (nameof(Commands.ForgetSection), 20008),
            (nameof(Commands.InventoryAction), 20009),
            (nameof(Commands.ServerStatus), 20010),
            (nameof(Commands.PresenceChallenge), 20011),
            (nameof(Commands.PresenceProof), 20012),
            (nameof(Commands.ClientStart), 30000),
            (nameof(Commands.ReadyAuth), 30001),
            (nameof(Commands.ChunkUpdate), 30002),
            (nameof(Commands.WorldIndicesUpdate), 30003),
            (nameof(Commands.PositionUpdate), 30004),
            (nameof(Commands.SlowDown), 30005),
            (nameof(Commands.SlowTick), 30006),
            (nameof(Commands.SectionUpdate), 30007),
            (nameof(Commands.EntUpdate), 30008),
            (nameof(Commands.ServerPopulation), 30009),
            (nameof(Commands.ServerDescription), 30010),
            (nameof(Commands.ServerIcon), 30011),
            (nameof(Commands.ServerStatusDone), 30012),
            (nameof(Commands.ChunkLightUpdate), 30013),
            (nameof(Commands.EntSyncSchema), 30014),
            (nameof(Commands.InventoryActionResult), 30015),
            (nameof(Commands.BeginTerrainLoad), 30016),
            (nameof(Commands.PlayerIdentity), 30017),
            (nameof(Commands.PresenceRoundChunk), 30018),
            (nameof(Commands.PresenceProofBatch), 30019),
        ];

        var actual = Enum.GetValues<Commands>();
        Assert.AreEqual(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i].Name, actual[i].ToString(), $"Command name at index {i} changed.");
            Assert.AreEqual(expected[i].Id, (ushort)actual[i], $"Command ID for {actual[i]} changed.");
        }
    }

    [TestMethod]
    public void SecurityCommandMarkers_UseFrozenIds()
    {
        Assert.AreEqual((ushort)20001, BeginAuthCommand.CommandId);
        Assert.AreEqual((ushort)20002, CompleteAuthCommand.CommandId);
        Assert.AreEqual((ushort)30001, ReadyAuthCommand.CommandId);
        Assert.AreEqual((ushort)20011, PresenceChallengeCommand.CommandId);
        Assert.AreEqual((ushort)20012, PresenceProofCommand.CommandId);
        Assert.AreEqual((ushort)30017, PlayerIdentityCommand.CommandId);
        Assert.AreEqual((ushort)30018, PresenceRoundChunkCommand.CommandId);
        Assert.AreEqual((ushort)30019, PresenceProofBatchCommand.CommandId);
    }
}
