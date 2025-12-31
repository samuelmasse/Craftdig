namespace Craftdig.Server.Cli;

public class Cli(string[] args)
{
    public int? Run()
    {
        var props = typeof(ServerConfig)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => x.Name)
            .ToArray();

        string[] help = ["-?", "-h", "--help"];

        if (help.Any(x => args.Contains(x)))
        {
            Console.WriteLine();
            Console.WriteLine($"Usage:");
            Console.WriteLine(" CraftdigServer [options]");
            Console.WriteLine();
            Console.WriteLine("Run a Craftdig server");
            Console.WriteLine();
            Console.WriteLine("Options:");

            foreach (var prop in props)
                Console.WriteLine($" --{prop}");

            return 0;
        }

        bool declaring = false;
        foreach (var arg in args)
        {
            if (arg.StartsWith("--"))
            {
                if (!props.Contains(arg[2..]))
                {
                    Console.WriteLine($"Craftdig: unrecognized option '{arg}'");
                    return 1;
                }

                declaring = true;
            }
            else if (!declaring)
            {
                Console.WriteLine($"Craftdig: invalid syntax '{arg}'");
                return 1;
            }
            else declaring = false;
        }

        if (declaring)
        {
            Console.WriteLine($"Craftdig: incomplete option '{args[^1]}'");
            return 1;
        }

        return null;
    }
}
