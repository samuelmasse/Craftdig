namespace Craftdig.Server.Security.Test;

[TestClass]
public sealed class ServerPublicContextsTest
{
    [TestMethod]
    public void EmptyConfiguration_FailsClosedAndLogsAWarning()
    {
        using var output = new StringWriter();
        using var logging = new LogRuntime(output) { UseColor = false };
        var contexts = new ServerPublicContexts(logging.Log, new() { PublicServerContexts = [] });

        Assert.IsTrue(ServerContext.TryParseCanonical("localhost", 36676, out var context));
        Assert.IsFalse(contexts.Allows(context));
        logging.Flush();
        StringAssert.Contains(output.ToString(), "No public Identity server contexts are configured");
    }

    [TestMethod]
    public void ConfiguredContexts_AllowExactMatchesOnlyAndRejectDuplicates()
    {
        var log = new LogRuntime(TextWriter.Null).Log;
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
}
