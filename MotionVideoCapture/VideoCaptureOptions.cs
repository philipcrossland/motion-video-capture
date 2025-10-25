namespace MotionVideoCapture;

public class VideoCaptureOptions
{
    public string OutputFolder { get; set; } = "./videos";
    public int DurationSeconds { get; set; } = 30;
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public int FrameRate { get; set; } = 30;
    public string VideoDevice { get; set; } = "/dev/video0";
}
