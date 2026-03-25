using PunchReha.Models;

namespace PunchReha.Services;

/// <summary>
/// Interface for punch input — can be implemented by:
/// - TouchPunchDetector: Touch/Click simulation (for development)
/// - BlePunchDetector: Real BLE sensor (production)
/// </summary>
public interface IPunchDetector
{
    event EventHandler<PunchEvent>? PunchDetected;
    void Start();
    void Stop();
    bool IsActive { get; }
}
