namespace Crafthoe.Server.Cli;

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
            Console.WriteLine(" CrafthoeServer [options]");
            Console.WriteLine();
            Console.WriteLine("Run a Crafthoe server");
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
                    Console.WriteLine($"Crafthoe: unrecognized option '{arg}'");
                    return 1;
                }

                declaring = true;
            }
            else if (!declaring)
            {
                Console.WriteLine($"Crafthoe: invalid syntax '{arg}'");
                return 1;
            }
            else declaring = false;
        }

        if (declaring)
        {
            Console.WriteLine($"Crafthoe: incomplete option '{args[^1]}'");
            return 1;
        }

        return null;
    }
}
