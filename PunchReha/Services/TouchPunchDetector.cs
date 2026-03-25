using PunchReha.Models;

namespace PunchReha.Services;

/// <summary>
/// Touch-based punch detector for development/testing.
/// 
/// - Touch down = punch starts
/// - Touch up = punch ends  
/// - Duration of touch = power (longer press = harder punch)
/// - Touch position = direction (left/right/high/low)
///
/// Power mapping:
///   0-200ms   → 0.2 (tap)
///   200-500ms → 0.5 (medium)
///   500-1000ms → 0.8 (strong)
///   1000ms+   → 1.0 (max)
/// </summary>
public class TouchPunchDetector : IPunchDetector
{
    public event EventHandler<PunchEvent>? PunchDetected;
    public bool IsActive { get; private set; }

    private DateTime _touchStartTime;
    private bool _isTouching;
    private float _touchX, _touchY;
    private float _screenWidth, _screenHeight;

    public void SetScreenSize(float width, float height)
    {
        _screenWidth = width;
        _screenHeight = height;
    }

    public void Start()
    {
        IsActive = true;
        System.Diagnostics.Debug.WriteLine("[TouchPunchDetector] Started");
    }

    public void Stop()
    {
        IsActive = false;
        _isTouching = false;
        System.Diagnostics.Debug.WriteLine("[TouchPunchDetector] Stopped");
    }

    public void OnTouchDown(float x, float y)
    {
        if (!IsActive) return;
        _touchStartTime = DateTime.UtcNow;
        _isTouching = true;
        _touchX = x;
        _touchY = y;
    }

    public void OnTouchUp()
    {
        if (!IsActive || !_isTouching) return;
        _isTouching = false;

        var durationMs = (long)(DateTime.UtcNow - _touchStartTime).TotalMilliseconds;
        var power = CalculatePower(durationMs);
        var direction = CalculateDirection(_touchX, _touchY);

        var punchEvent = new PunchEvent
        {
            Power = power,
            DurationMs = durationMs,
            Direction = direction,
            RawAcceleration = null
        };

        System.Diagnostics.Debug.WriteLine(
            $"[TouchPunchDetector] Punch: power={power:F2} dir={direction} duration={durationMs}ms");

        PunchDetected?.Invoke(this, punchEvent);
    }

    private static float CalculatePower(long durationMs) => durationMs switch
    {
        < 200 => 0.2f + (durationMs / 200f) * 0.1f,
        < 500 => 0.3f + ((durationMs - 200) / 300f) * 0.3f,
        < 1000 => 0.6f + ((durationMs - 500) / 500f) * 0.3f,
        _ => Math.Min(0.9f + ((durationMs - 1000) / 2000f) * 0.1f, 1.0f)
    };

    private PunchDirection CalculateDirection(float x, float y)
    {
        if (_screenWidth <= 0 || _screenHeight <= 0) return PunchDirection.Straight;

        var hRatio = x / _screenWidth;
        var vRatio = y / _screenHeight;

        return (hRatio, vRatio) switch
        {
            (_, < 0.35f) => PunchDirection.High,
            (_, > 0.65f) => PunchDirection.Low,
            (< 0.35f, _) => PunchDirection.Left,
            (> 0.65f, _) => PunchDirection.Right,
            _ => PunchDirection.Straight
        };
    }
}
