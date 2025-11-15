namespace Crafthoe.Menus.Multiplayer;

[App]
public class AppClientOptions
{
    private bool useRawTcp;
    private string? noAuthUser;

    public bool AllowRawTcp { get; init; }
    public bool AllowNoAuth { get; init; }
    public string? DefaultNoAuthUser { get; set; }

    public bool UseRawTcp
    {
        get => AllowRawTcp && useRawTcp;
        set
        {
            if (AllowRawTcp)
                useRawTcp = value;
        }
    }

    public string? NoAuthUser
    {
        get => AllowNoAuth ? noAuthUser : null;
        set
        {
            if (AllowNoAuth)
                noAuthUser = value;
        }
    }
}
