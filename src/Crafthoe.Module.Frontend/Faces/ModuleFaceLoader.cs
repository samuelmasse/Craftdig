namespace Crafthoe.Module.Frontend;

[ModuleLoader]
public class ModuleFaceLoader(ModuleEntsMut entsMut, ModuleFaceAtlas faceAtlas, ModuleFaceTextures faceTextures)
{
    public void Run()
    {
        var faces = entsMut.Set.Where(x => x.GetIsFace()).ToList();

        faces.ForEach(x => x
            .FaceIndex(faceAtlas[x.FaceFile()])
            .FaceTexture(faceTextures[x.FaceFile()]));
    }
}
