namespace MotionVideoCapture;

public class PirSensorOptions
{
    public int GpioPin { get; set; } = 17;
    public int DebounceMilliseconds { get; set; } = 1000;
}
