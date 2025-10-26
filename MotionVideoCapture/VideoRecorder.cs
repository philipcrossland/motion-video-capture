using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MotionVideoCapture;

public class VideoRecorder
{
    private readonly VideoCaptureOptions _options;
    private readonly ILogger<VideoRecorder> _logger;
    private readonly SemaphoreSlim _recordingSemaphore = new(1, 1);

    public VideoRecorder(VideoCaptureOptions options, ILogger<VideoRecorder> logger)
    {
        _options = options;
        _logger = logger;
        
        // Create output directory if it doesn't exist
        var outputFolder = GetOutputFolder();
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            _logger.LogInformation("Created output directory: {OutputFolder}", outputFolder);
        }
    }

    public async Task RecordVideoAsync(CancellationToken cancellationToken = default)
    {
        // Prevent multiple simultaneous recordings
        if (!await _recordingSemaphore.WaitAsync(0, cancellationToken))
        {
            _logger.LogWarning("Recording already in progress, skipping new request");
            return;
        }

        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var outputFolder = GetOutputFolder();
            var outputPath = Path.Combine(outputFolder, $"motion_{timestamp}.mp4");

            _logger.LogInformation("Starting video recording: {OutputPath}", outputPath);

            // Build FFmpeg command
            // Note: For Raspberry Pi, you might need to use different input sources like:
            // - v4l2 for USB cameras: -f v4l2 -i /dev/video0
            // - libcamera for Pi Camera: -f lavfi -i testsrc (or use libcamera-vid directly)
            var arguments = $"-f v4l2 -video_size {_options.Width}x{_options.Height} " +
                          $"-framerate {_options.FrameRate} -i {_options.VideoDevice} " +
                          $"-t {_options.DurationSeconds} -c:v libx264 -preset ultrafast " +
                          $"-y \"{outputPath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };

            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("Video recording completed successfully: {OutputPath}", outputPath);
            }
            else
            {
                _logger.LogError("Video recording failed with exit code {ExitCode}. Error: {Error}",
                    process.ExitCode, errorBuilder.ToString());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during video recording");
        }
        finally
        {
            _recordingSemaphore.Release();
        }
    }

    private string GetOutputFolder()
    {
        var rawPath = _options.OutputFolder;
        var expandedPath = rawPath.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        return expandedPath;
    }
}
