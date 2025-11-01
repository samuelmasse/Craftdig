namespace Crafthoe.Player.Frontend;

public record PlayerControls(
    Control CameraFast,
    Control CameraFront,
    Control CameraRight,
    Control CameraBack,
    Control CameraLeft,
    Control CameraUp,
    Control CameraDown,
    Control CameraJump,
    Control CameraCrouch) : ControlList;
