using System.Device.Gpio;
using Microsoft.Extensions.Logging;

namespace MotionVideoCapture;

public class PirSensorMonitor : IDisposable
{
    private readonly GpioController _controller;
    private readonly int _pin;
    private readonly int _debounceMilliseconds;
    private readonly ILogger<PirSensorMonitor> _logger;
    private DateTime _lastTriggerTime = DateTime.MinValue;
    
    public event EventHandler? MotionDetected;

    public PirSensorMonitor(PirSensorOptions options, ILogger<PirSensorMonitor> logger)
    {
        _pin = options.GpioPin;
        _debounceMilliseconds = options.DebounceMilliseconds;
        _logger = logger;
        
        _controller = new GpioController();
        _controller.OpenPin(_pin, PinMode.Input);
        
        _logger.LogInformation("PIR sensor initialized on GPIO pin {Pin}", _pin);
    }

    public void StartMonitoring()
    {
        _controller.RegisterCallbackForPinValueChangedEvent(
            _pin,
            PinEventTypes.Rising,
            OnPinValueChanged);
        
        _logger.LogInformation("Started monitoring PIR sensor");
    }

    private void OnPinValueChanged(object sender, PinValueChangedEventArgs args)
    {
        var now = DateTime.UtcNow;
        var timeSinceLastTrigger = (now - _lastTriggerTime).TotalMilliseconds;
        
        if (timeSinceLastTrigger >= _debounceMilliseconds)
        {
            _lastTriggerTime = now;
            _logger.LogInformation("Motion detected!");
            MotionDetected?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Dispose()
    {
        _controller?.Dispose();
        _logger.LogInformation("PIR sensor monitor disposed");
    }
}
