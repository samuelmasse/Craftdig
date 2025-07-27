namespace Crafthoe.Player;

public record PlayerControls(
    Control CameraFront,
    Control CameraRight,
    Control CameraBack,
    Control CameraLeft,
    Control CameraUp,
    Control CameraDown) : ControlList;
