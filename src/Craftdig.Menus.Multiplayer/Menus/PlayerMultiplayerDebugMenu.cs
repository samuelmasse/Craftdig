namespace Craftdig.Menus.Common;

[Player]
public class PlayerMultiplayerDebugMenu(
    RootText text,
    RootKeyboard keyboard,
    AppStyle s,
    PlayerPongReceiver pongReceiver,
    PlayerEnt playerEnt,
    PlayerPosition playerPosition,
    PlayerPositionUpdateReceiver playerPositionUpdateReceiver,
    PlayerSlowTickReceiver slowTickReceiver)
{
    public void Create(EntObj root)
    {
        List<Func<ReadOnlySpan<char>>> lines =
        [
            () => text.Format("Ping: {0} ms", pongReceiver.Latency),
            () => text.Format("Tolerance: {0}", playerPosition.Tolerance),
            () => text.Format("Correcting: {0}", playerPosition.Correcting),
            () => text.Format("Matching: {0:0.0000}", playerPosition.Matching),
            () => text.Format("Slow: {0}", slowTickReceiver.Count),
            () => text.Format("Listen: {0}", playerPosition.Listen),
            () => text.Format("Debounce: {0}", playerPosition.Debounce),
            () => text.Format("Client: {0:0.00}", playerEnt.Ent.Position()),
            () => text.Format("Server: {0:0.00}", playerPositionUpdateReceiver.Latest.Position),
        ];

        Node(root, out var list)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .SizeInnerSumRelativeV(s.Vertical)
            .InnerLayoutV(InnerLayout.VerticalList)
            .IsDisabledV(true)
            .OnUpdateF(() =>
            {
                if (keyboard.IsKeyPressed(Keys.F2))
                    playerPosition.Correcting = !playerPosition.Correcting;
            });
        {
            lines.ForEach(x => Node(list)
                .Mut(s.Label)
                .ColorV((0.5f, 0.5f, 0.5f, 0.5f))
                .TextF(x));

            for (int i = 0; i < playerPosition.Tolerance; i++)
            {
                int t = i;

                (PositionUpdateCommand, double) Process()
                {
                    var cmd = t < playerPosition.Expected.Length ? playerPosition.Expected[t] : default;

                    double min = double.MaxValue;
                    foreach (var r in playerPosition.Received)
                    {
                        var dist = Vector3d.Distance(r.Position, cmd.Position);
                        if (dist < min)
                            min = dist;
                    }

                    return (cmd, min);
                }

                Node(list)
                    .Mut(s.Label)
                    .ColorF(() =>
                    {
                        var (cmd, dist) = Process();
                        if (dist > playerPosition.Matching)
                            return (1, 0.5f, 0.5f, 0.5f);

                        return (0.5f, 0.5f, 0.5f, 0.5f);
                    })
                    .TextF(() =>
                    {
                        var (cmd, dist) = Process();
                        return text.Format("{0:0.0000} : {1:0.00}", dist,  cmd.Position);
                    });
            }
        }

        Node(root).OnUpdateF(() =>
        {
            if (keyboard.IsKeyPressed(Keys.F4))
                list.IsDisabledV() = !list.IsDisabledV();
        });
    }
}
