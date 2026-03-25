namespace PunchReha.Models;

/// <summary>
/// Represents a single punch event detected by the sensor or simulation.
/// </summary>
public record PunchEvent
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public float Power { get; init; }           // 0.0 - 1.0 (normalized force)
    public long DurationMs { get; init; }        // How long the contact lasted
    public PunchDirection Direction { get; init; } = PunchDirection.Straight;
    public (float X, float Y, float Z)? RawAcceleration { get; init; }
}

public enum PunchDirection
{
    Straight,  // Jab / gerader Schlag
    Left,      // Von links
    Right,     // Von rechts
    High,      // Hoher Schlag (Aufwärtshaken)
    Low        // Tiefer Schlag (zum Körper)
}

/// <summary>
/// A target that appears on screen for the player to punch.
/// </summary>
public record Target
{
    public long Id { get; init; }
    public PunchDirection Direction { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public long LifetimeMs { get; init; }
    public PunchEvent? HitBy { get; init; }
    public bool IsHit { get; init; }

    public bool IsExpired => (DateTime.UtcNow - CreatedAt).TotalMilliseconds > LifetimeMs;
    public bool WasMissed => !IsHit && IsExpired;
}

/// <summary>
/// Tracks the current game session statistics.
/// </summary>
public record GameStats
{
    public int TotalPunches { get; init; }
    public int Hits { get; init; }
    public int Misses { get; init; }
    public float TotalPower { get; init; }
    public float MaxPower { get; init; }
    public long AvgReactionMs { get; init; }
    public int Combo { get; init; }
    public int MaxCombo { get; init; }
    public Dictionary<PunchDirection, int> PunchByDirection { get; init; } = new();

    public float Accuracy => TotalPunches > 0 ? (float)Hits / TotalPunches : 0f;
    public float AvgPower => Hits > 0 ? TotalPower / Hits : 0f;

    public GameStats AddHit(float power, long reactionMs, PunchDirection direction)
    {
        var newHits = Hits + 1;
        var newCombo = Combo + 1;
        var dirCounts = new Dictionary<PunchDirection, int>(PunchByDirection);
        dirCounts[direction] = dirCounts.GetValueOrDefault(direction) + 1;

        return this with
        {
            TotalPunches = TotalPunches + 1,
            Hits = newHits,
            TotalPower = TotalPower + power,
            MaxPower = Math.Max(MaxPower, power),
            AvgReactionMs = newHits > 1
                ? (AvgReactionMs * (newHits - 1) + reactionMs) / newHits
                : reactionMs,
            Combo = newCombo,
            MaxCombo = Math.Max(MaxCombo, newCombo),
            PunchByDirection = dirCounts
        };
    }

    public GameStats AddMiss()
    {
        return this with
        {
            TotalPunches = TotalPunches + 1,
            Misses = Misses + 1,
            Combo = 0
        };
    }
}
