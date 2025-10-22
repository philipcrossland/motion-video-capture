using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MotionVideoCapture;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Setup logging
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConfiguration(configuration.GetSection("Logging"))
        .AddConsole();
});

var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("Motion Video Capture starting...");

// Load configuration options
var pirSensorOptions = configuration.GetSection("PirSensor").Get<PirSensorOptions>() 
    ?? new PirSensorOptions();
var videoCaptureOptions = configuration.GetSection("VideoCapture").Get<VideoCaptureOptions>() 
    ?? new VideoCaptureOptions();

// Create components
var pirSensorLogger = loggerFactory.CreateLogger<PirSensorMonitor>();
var videoRecorderLogger = loggerFactory.CreateLogger<VideoRecorder>();

using var pirSensor = new PirSensorMonitor(pirSensorOptions, pirSensorLogger);
var videoRecorder = new VideoRecorder(videoCaptureOptions, videoRecorderLogger);

// Setup cancellation token for graceful shutdown
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    logger.LogInformation("Shutdown requested...");
};

// Wire up motion detection to video recording
pirSensor.MotionDetected += async (sender, args) =>
{
    logger.LogInformation("Motion detected, starting video recording...");
    await videoRecorder.RecordVideoAsync(cts.Token);
};

// Start monitoring
pirSensor.StartMonitoring();

logger.LogInformation("Monitoring for motion. Press Ctrl+C to exit.");

// Keep the application running until cancellation is requested
try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (TaskCanceledException)
{
    logger.LogInformation("Application shutting down...");
}

logger.LogInformation("Motion Video Capture stopped.");
