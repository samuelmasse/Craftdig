using Crafthoe.Dev;

// double t = 1;
// int i = 0;
// 
// while (true)
// {
//     Console.WriteLine($"{i++} {t}");
// 
//     t = t * 0.95;
//     if (t < 0.001)
//     {
//         t = 0.001;
//         Console.ReadLine();
//     }
// }

new RootLoop(new()
{
    Window = new WindowOpenTK(new(new(), new() { StartVisible = false })),
    Driver = new GldOpenTK(),
    BootState = typeof(RootLoadNativeState)
}).Run();
