# motion-video-capture

A .NET 9 console application that uses a PIR (Passive Infrared) motion sensor on a Raspberry Pi to trigger video recording. When motion is detected, the application records video for 30 seconds and saves it to a local folder.

## Features

- PIR sensor monitoring via GPIO pins
- Automatic 30-second video recording on motion detection
- Configurable video settings (resolution, framerate, output folder)
- Debouncing to prevent multiple rapid triggers
- Comprehensive logging
- Graceful shutdown on Ctrl+C

## Prerequisites

- Raspberry Pi with GPIO support
- .NET 9 SDK installed
- PIR motion sensor connected to GPIO pin (default: GPIO 17)
- Video capture device (USB camera or Raspberry Pi Camera)
- FFmpeg installed (`sudo apt-get install ffmpeg`)

## Configuration

Edit `appsettings.json` to customize settings:

```json
{
  "PirSensor": {
    "GpioPin": 17,                    // GPIO pin number for PIR sensor
    "DebounceMilliseconds": 1000      // Minimum time between triggers
  },
  "VideoCapture": {
    "OutputFolder": "./videos",       // Where to save recorded videos
    "DurationSeconds": 30,            // Recording duration
    "Width": 1920,                    // Video width
    "Height": 1080,                   // Video height
    "FrameRate": 30,                  // Frames per second
    "VideoDevice": "/dev/video0"      // Video device path
  }
}
```

## Hardware Setup

1. Connect PIR sensor to Raspberry Pi:
   - VCC → 5V pin
   - GND → Ground pin
   - OUT → GPIO 17 (or configured pin)

2. Connect video capture device:
   - For USB camera: Usually auto-detected as `/dev/video0`
   - For Pi Camera: May need to enable camera interface in `raspi-config`

## Building

```bash
cd MotionVideoCapture
dotnet build
```

## Running

```bash
cd MotionVideoCapture
dotnet run
```

Or run the published version:

```bash
dotnet publish -c Release -o publish
cd publish
./MotionVideoCapture
```

## Usage

1. Start the application
2. The PIR sensor will monitor for motion
3. When motion is detected, video recording starts automatically
4. Videos are saved with timestamps in the configured output folder
5. Press Ctrl+C to stop the application

## Output Files

Videos are saved with the naming format: `motion_YYYYMMDD_HHmmss.mp4`

Example: `motion_20251022_143052.mp4`

## Troubleshooting

### GPIO Access Issues
If you get permission errors accessing GPIO, run with sudo or add your user to the gpio group:
```bash
sudo usermod -a -G gpio $USER
```

### Video Device Not Found
List available video devices:
```bash
ls -l /dev/video*
v4l2-ctl --list-devices
```

### FFmpeg Not Found
Install FFmpeg:
```bash
sudo apt-get update
sudo apt-get install ffmpeg
```

## License

MIT
