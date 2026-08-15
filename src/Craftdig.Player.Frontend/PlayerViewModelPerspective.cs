namespace Craftdig;

[Player]
public class PlayerViewModelPerspective : Perspective3D
{
    public PlayerViewModelPerspective()
    {
        Fov = 70;
        Near = 0.01f;
        Far = 4;
    }
}
