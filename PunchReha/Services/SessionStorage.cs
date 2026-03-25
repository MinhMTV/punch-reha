using System.Text.Json;
using PunchReha.Models;

namespace PunchReha.Services;

/// <summary>
/// Persists game session results to local storage.
/// Uses Preferences + JSON for simplicity (can swap to SQLite later).
/// </summary>
public class SessionStorage
{
    private const string Key = "punch_reha_sessions";
    private readonly List<GameSessionResult> _cache = new();

    public SessionStorage()
    {
        Load();
    }

    /// <summary>
    /// Save a completed game session.
    /// </summary>
    public void SaveSession(GameSessionResult result)
    {
        _cache.Add(result);
        Persist();
    }

    /// <summary>
    /// Get all saved sessions, newest first.
    /// </summary>
    public List<GameSessionResult> GetAll() =>
        _cache.OrderByDescending(s => s.PlayedAt).ToList();

    /// <summary>
    /// Get sessions for a specific level.
    /// </summary>
    public List<GameSessionResult> GetByLevel(int levelNumber) =>
        _cache.Where(s => s.LevelNumber == levelNumber)
              .OrderByDescending(s => s.PlayedAt)
              .ToList();

    /// <summary>
    /// Get the best score for a level.
    /// </summary>
    public int GetBestScore(int levelNumber) =>
        _cache.Where(s => s.LevelNumber == levelNumber)
              .Select(s => s.TotalScore)
              .DefaultIfEmpty(0)
              .Max();

    /// <summary>
    /// Get total stats across all sessions.
    /// </summary>
    public GlobalStats GetGlobalStats()
    {
        if (_cache.Count == 0) return new GlobalStats();

        return new GlobalStats
        {
            TotalSessions = _cache.Count,
            TotalPunches = _cache.Sum(s => s.TotalPunches),
            TotalHits = _cache.Sum(s => s.Hits),
            TotalPlayTimeSeconds = _cache.Sum(s => s.DurationSeconds),
            BestScore = _cache.Max(s => s.TotalScore),
            AvgAccuracy = _cache.Average(s => s.Accuracy),
            LevelsPlayed = _cache.Select(s => s.LevelNumber).Distinct().Count()
        };
    }

    public void Clear()
    {
        _cache.Clear();
        Preferences.Remove(Key);
    }

    private void Load()
    {
        try
        {
            var json = Preferences.Get(Key, "[]");
            _cache = JsonSerializer.Deserialize<List<GameSessionResult>>(json) ?? new();
        }
        catch
        {
            _cache = new();
        }
    }

    private void Persist()
    {
        var json = JsonSerializer.Serialize(_cache);
        Preferences.Set(Key, json);
    }
}

/// <summary>
/// A persisted game session result.
/// </summary>
public record GameSessionResult
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public int LevelNumber { get; init; }
    public string LevelName { get; init; } = "";
    public int TotalScore { get; init; }
    public int TotalPunches { get; init; }
    public int Hits { get; init; }
    public int Misses { get; init; }
    public float Accuracy { get; init; }
    public float MaxPower { get; init; }
    public float AvgPower { get; init; }
    public int MaxCombo { get; init; }
    public long AvgReactionMs { get; init; }
    public int DurationSeconds { get; init; }
    public DateTime PlayedAt { get; init; } = DateTime.UtcNow;
    public Dictionary<PunchDirection, int> PunchByDirection { get; init; } = new();
}

public record GlobalStats
{
    public int TotalSessions { get; init; }
    public int TotalPunches { get; init; }
    public int TotalHits { get; init; }
    public long TotalPlayTimeSeconds { get; init; }
    public int BestScore { get; init; }
    public float AvgAccuracy { get; init; }
    public int LevelsPlayed { get; init; }
}
