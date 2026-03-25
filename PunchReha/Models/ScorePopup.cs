namespace PunchReha.Models;

/// <summary>
/// A floating score popup that appears when you hit a target.
/// Fades and floats upward.
/// </summary>
public record ScorePopup
{
    public long Id { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public int Points { get; init; }
    public string Text { get; init; } = "";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public float LifetimeMs { get; init; } = 1000f;

    public float Progress => Math.Min(1f, (float)(DateTime.UtcNow - CreatedAt).TotalMilliseconds / LifetimeMs);
    public bool IsExpired => Progress >= 1f;
    public float Alpha => 1f - Progress;
    public float OffsetY => -80f * Progress; // Float upward
}
