using PunchReha.Models;

namespace PunchReha.Services;

/// <summary>
/// BLE-based punch detector — connects to a boxing glove sensor via Bluetooth.
/// TODO: Implement with Plugin.BLE once sensor protocol is known.
/// 
/// Expected sensor data format (placeholder):
/// {
///   "acceleration": { "x": 0.5, "y": -0.3, "z": 9.8 },
///   "gyroscope": { "x": 0.1, "y": -0.2, "z": 0.05 },
///   "impact": 0.85,
///   "timestamp": 1234567890
/// }
/// </summary>
public class BlePunchDetector : IPunchDetector
{
    public event EventHandler<PunchEvent>? PunchDetected;
    public bool IsActive { get; private set; }

    // Placeholder: Thresholds for punch detection from sensor data
    private const float ImpactThreshold = 0.3f;  // Minimum impact to count as punch
    private const float HighYThreshold = 0.7f;   // Y > this → high punch
    private const float LowYThreshold = -0.7f;   // Y < this → low punch
    private const float SideXThreshold = 0.5f;    // |X| > this → left/right

    public void Start()
    {
        IsActive = true;
        // TODO: Start BLE scan, connect to sensor, subscribe to data
        System.Diagnostics.Debug.WriteLine("[BlePunchDetector] Started (placeholder)");
    }

    public void Stop()
    {
        IsActive = false;
        // TODO: Disconnect BLE
        System.Diagnostics.Debug.WriteLine("[BlePunchDetector] Stopped");
    }

    /// <summary>
    /// Call this when sensor data arrives via BLE.
    /// Converts raw acceleration data into a PunchEvent.
    /// </summary>
    public void OnSensorData(float ax, float ay, float az, float impact)
    {
        if (!IsActive || impact < ImpactThreshold) return;

        var direction = ClassifyDirection(ax, ay, az);
        var power = Math.Min(impact, 1.0f);

        var punchEvent = new PunchEvent
        {
            Power = power,
            DurationMs = 0, // Sensor doesn't provide this directly
            Direction = direction,
            RawAcceleration = (ax, ay, az)
        };

        PunchDetected?.Invoke(this, punchEvent);
    }

    private static PunchDirection ClassifyDirection(float ax, float ay, float az)
    {
        return (ax, ay) switch
        {
            (_, > HighYThreshold) => PunchDirection.High,
            (_, < LowYThreshold) => PunchDirection.Low,
            (< -SideXThreshold, _) => PunchDirection.Left,
            (> SideXThreshold, _) => PunchDirection.Right,
            _ => PunchDirection.Straight
        };
    }
}
