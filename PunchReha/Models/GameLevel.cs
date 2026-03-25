namespace PunchReha.Models;

/// <summary>
/// Predefined game levels.
/// </summary>
public static class GameLevels
{
    public static IReadOnlyList<GameLevel> Levels { get; } = new[]
    {
        new GameLevel(1, "Einstieg", 60,
            targetIntervalMs: 2000, targetLifetimeMs: 3000,
            directions: new[] { PunchDirection.Straight },
            minPower: 0.1f, speedMultiplier: 1.0f),

        new GameLevel(2, "Links & Rechts", 60,
            targetIntervalMs: 1800, targetLifetimeMs: 2500,
            directions: new[] { PunchDirection.Left, PunchDirection.Right },
            minPower: 0.15f, speedMultiplier: 1.1f),

        new GameLevel(3, "Drei Richtungen", 60,
            targetIntervalMs: 1500, targetLifetimeMs: 2200,
            directions: new[] { PunchDirection.Left, PunchDirection.Right, PunchDirection.Straight },
            minPower: 0.2f, speedMultiplier: 1.2f),

        new GameLevel(4, "High & Low", 60,
            targetIntervalMs: 1500, targetLifetimeMs: 2000,
            directions: new[] { PunchDirection.High, PunchDirection.Low },
            minPower: 0.2f, speedMultiplier: 1.3f),

        new GameLevel(5, "Alle Richtungen", 60,
            targetIntervalMs: 1200, targetLifetimeMs: 1800,
            directions: Enum.GetValues<PunchDirection>(),
            minPower: 0.25f, speedMultiplier: 1.4f),

        new GameLevel(6, "Speed", 45,
            targetIntervalMs: 800, targetLifetimeMs: 1200,
            directions: Enum.GetValues<PunchDirection>(),
            minPower: 0.3f, speedMultiplier: 1.6f),

        new GameLevel(7, "Power", 60,
            targetIntervalMs: 2000, targetLifetimeMs: 3000,
            directions: Enum.GetValues<PunchDirection>(),
            minPower: 0.5f, speedMultiplier: 1.0f),

        new GameLevel(8, "Combo-Meister", 60,
            targetIntervalMs: 600, targetLifetimeMs: 1000,
            directions: Enum.GetValues<PunchDirection>(),
            minPower: 0.2f, speedMultiplier: 1.8f),

        new GameLevel(9, "Endurance", 120,
            targetIntervalMs: 1000, targetLifetimeMs: 1500,
            directions: Enum.GetValues<PunchDirection>(),
            minPower: 0.25f, speedMultiplier: 1.5f),

        new GameLevel(10, "Champion", 90,
            targetIntervalMs: 500, targetLifetimeMs: 800,
            directions: Enum.GetValues<PunchDirection>(),
            minPower: 0.3f, speedMultiplier: 2.0f),
    };

    public static GameLevel GetLevel(int number) =>
        Levels.ElementAtOrDefault(number - 1) ?? Levels.Last();
}

public record GameLevel(
    int Number,
    string Name,
    int DurationSeconds,
    long TargetIntervalMs,
    long TargetLifetimeMs,
    PunchDirection[] Directions,
    float MinPower = 0.1f,
    float SpeedMultiplier = 1.0f
);
