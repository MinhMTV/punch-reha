# 🥊 Punch Reha

Boxhandschuh-Rehabilitationstraining mit Sensor-Integration (MAUI, C#)

## Überblick

Gamifiziertes Reha-Training für Boxhandschuh-Patienten. Das Spiel zeigt Ziele auf dem Screen, die der Patient durch Schlagen treffen muss. Sensoren messen Kraft, Richtung und Geschwindigkeit.

## Features

- **10 Level** mit steigender Schwierigkeit
- **Touch-Modus** (Placeholder): Länger drücken = härterer Schlag
- **BLE-Sensor-Modus** (Vorbereitung): Automatische Punch-Erkennung
- **Richtungserkennung**: Links, Rechts, Hoch, Tief, Gerade
- **Statistiken**: Trefferquote, Combo, Max/Avg Power, Reaktionszeit
- **Dark Theme** UI für bessere Sichtbarkeit

## Steuerung (Touch-Modus)

| Aktion | Effekt |
|--------|--------|
| Tap (kurz) | Leichter Schlag (20%) |
| Halten 0.5s | Mittlerer Schlag (50%) |
| Halten 1s | Starker Schlag (80%) |
| Halten 2s+ | Maximaler Schlag (100%) |
| Links tippen | Linker Schlag |
| Rechts tippen | Rechter Schlag |
| Oben tippen | Hoher Schlag |
| Unten tippen | Tiefer Schlag |
| Mitte tippen | Gerader Schlag |

## Sensor-Integration (geplant)

Der `BlePunchDetector` erwartet folgendes Datenformat vom Sensor:

```json
{
  "acceleration": { "x": 0.5, "y": -0.3, "z": 9.8 },
  "gyroscope": { "x": 0.1, "y": -0.2, "z": 0.05 },
  "impact": 0.85,
  "timestamp": 1234567890
}
```

Richtungsklassifikation:
- `Y > 0.7` → Hoher Schlag
- `Y < -0.7` → Tiefer Schlag
- `X < -0.5` → Linker Schlag
- `X > 0.5` → Rechter Schlag

## Build

```bash
# Android
dotnet build -f net8.0-android

# iOS (nur auf macOS)
dotnet build -f net8.0-ios

# Windows
dotnet build -f net8.0-windows10.0.19041.0

# Mac
dotnet build -f net8.0-maccatalyst
```

## Dependencies

- .NET 8.0
- .NET MAUI
- CommunityToolkit.Mvvm (MVVM)
- Plugin.BLE (Bluetooth Low Energy)

## Projektstruktur

```
PunchReha/
├── Models/          → PunchEvent, Target, GameStats, GameLevel
├── Services/        → GameEngine, SensorService, TouchPunchDetector, BlePunchDetector
├── Converters/      → XAML Value Converters
├── ViewModels/      → Menu, Game, Stats (MVVM)
├── Views/           → XAML Pages (Menu, Game, Stats)
├── Platforms/       → Android, iOS, Windows, Mac-specific code
├── AppShell.xaml    → Navigation
└── MauiProgram.cs   → DI Setup
```
