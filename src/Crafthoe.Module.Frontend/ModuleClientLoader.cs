namespace Crafthoe.Module.Frontend;

[ModuleLoader]
public class ModuleClientLoader(ModuleFaceLoader faceLoader)
{
    public void Run()
    {
        faceLoader.Run();
    }
}
