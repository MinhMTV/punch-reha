using PunchReha.Models;

namespace PunchReha.Services;

/// <summary>
/// Abstracts sensor input — switches between touch simulation and real BLE sensor.
/// Provides a unified API for the game engine.
/// </summary>
public class SensorService : IDisposable
{
    private readonly TouchPunchDetector _touchDetector = new();
    private readonly BlePunchDetector _bleDetector = new();
    private IPunchDetector _activeDetector;

    public event EventHandler<PunchEvent>? PunchDetected;

    public bool IsBleConnected { get; private set; }
    public bool IsUsingBle => _activeDetector == _bleDetector;
    public string Mode => IsUsingBle ? "BLE Sensor" : "Touch (Simuliert)";

    public SensorService()
    {
        // Default: touch simulation
        _activeDetector = _touchDetector;

        // Forward events from both detectors
        _touchDetector.PunchDetected += (s, e) => PunchDetected?.Invoke(s, e);
        _bleDetector.PunchDetected += (s, e) => PunchDetected?.Invoke(s, e);
    }

    public TouchPunchDetector TouchDetector => _touchDetector;

    /// <summary>
    /// Start in touch simulation mode.
    /// </summary>
    public void StartTouchMode()
    {
        _bleDetector.Stop();
        _activeDetector = _touchDetector;
        _touchDetector.Start();
        System.Diagnostics.Debug.WriteLine("[SensorService] Mode: Touch");
    }

    /// <summary>
    /// Switch to BLE sensor mode.
    /// TODO: Implement actual BLE connection when sensor is available.
    /// </summary>
    public async Task<bool> TryConnectBleAsync()
    {
        try
        {
            // TODO: Use Plugin.BLE to scan and connect
            // var adapter = CrossBluetoothLE.Current.Adapter;
            // await adapter.StartScanningForDevicesAsync();
            // var device = await PickDeviceAsync();
            // await adapter.ConnectToDeviceAsync(device);
            // Subscribe to characteristic notifications

            _touchDetector.Stop();
            _activeDetector = _bleDetector;
            _bleDetector.Start();
            IsBleConnected = true;

            System.Diagnostics.Debug.WriteLine("[SensorService] Mode: BLE (placeholder)");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SensorService] BLE connection failed: {ex.Message}");
            StartTouchMode(); // Fallback to touch
            return false;
        }
    }

    public void DisconnectBle()
    {
        _bleDetector.Stop();
        IsBleConnected = false;
        StartTouchMode();
    }

    /// <summary>
    /// Forward sensor data from BLE to the detector.
    /// Call this when BLE notifications arrive.
    /// </summary>
    public void OnBleSensorData(float ax, float ay, float az, float impact)
    {
        _bleDetector.OnSensorData(ax, ay, az, impact);
    }

    public void SetScreenSize(float width, float height)
    {
        _touchDetector.SetScreenSize(width, height);
    }

    public void Start() => _activeDetector.Start();
    public void Stop() => _activeDetector.Stop();

    public void Dispose()
    {
        _touchDetector.Stop();
        _bleDetector.Stop();
        GC.SuppressFinalize(this);
    }
}
