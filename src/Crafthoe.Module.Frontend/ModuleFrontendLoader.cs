namespace Crafthoe.Module.Frontend;

[ModuleLoader]
public class ModuleFrontendLoader(ModuleFaceLoader faceLoader)
{
    public void Run()
    {
        faceLoader.Run();
    }
}
