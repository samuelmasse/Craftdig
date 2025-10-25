namespace Crafthoe.Module;

[ModuleLoader]
public class ModuleClientLoader(ModuleFaceLoader faceLoader)
{
    public void Run()
    {
        faceLoader.Run();
    }
}
