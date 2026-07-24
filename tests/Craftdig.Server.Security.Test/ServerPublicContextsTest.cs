namespace Craftdig.Server.Security.Test;

[TestClass]
public sealed class ServerPublicContextsTest
{
    [TestMethod]
    public void EmptyConfiguration_FailsClosedAndLogsAWarning()
    {
        var logs = new AppLogStream();
        var contexts = new ServerPublicContexts(new(logs), new() { PublicServerContexts = [] });

        Assert.IsTrue(ServerContext.TryParseCanonical("localhost", 36676, out var context));
        Assert.IsFalse(contexts.Allows(context));
        StringAssert.Contains(Collect(logs), "No public Identity server contexts are configured");
    }

    [TestMethod]
    public void ConfiguredContexts_AllowExactMatchesOnlyAndRejectDuplicates()
    {
        var log = new AppLog(new AppLogStream());
        var contexts = new ServerPublicContexts(log, new() { PublicServerContexts = ["localhost:36676"] });

        Assert.IsTrue(ServerContext.TryParseCanonical("localhost", 36676, out var matching));
        Assert.IsTrue(ServerContext.TryParseCanonical("127.0.0.1", 36676, out var otherHost));
        Assert.IsTrue(ServerContext.TryParseCanonical("localhost", 36677, out var otherPort));
        Assert.IsTrue(contexts.Allows(matching));
        Assert.IsFalse(contexts.Allows(otherHost));
        Assert.IsFalse(contexts.Allows(otherPort));

        Assert.ThrowsExactly<InvalidDataException>(() =>
            new ServerPublicContexts(log, new() { PublicServerContexts = ["localhost:36676", "localhost:36676"] }));
    }

    private static string Collect(AppLogStream logs)
    {
        logs.Collect(-1);
        var output = new StringBuilder();
        foreach (var segment in logs.Segments)
        {
            foreach (var entry in segment.Entries)
                output.Append(entry.Chars.Span);
        }

        return output.ToString();
    }
}
