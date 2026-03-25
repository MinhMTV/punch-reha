# 🥊 Punch Reha

Boxhandschuh-Rehabilitationstraining mit Sensor-Integration (.NET MAUI, C#)

## Überblick

Gamifiziertes Reha-Training für Boxhandschuh-Patienten. Das Spiel zeigt Ziele auf dem Screen, die der Patient durch Schlagen treffen muss.

## Features

### Gameplay
- **10 Level** mit steigender Schwierigkeit (Einstieg → Champion)
- **Touch-Modus** (Placeholder): Länger drücken = härterer Schlag
- **Richtungserkennung**: Links, Rechts, Hoch, Tief, Gerade
- **Combo-System**: Aufeinanderfolgende Treffer = Bonus
- **Score**: Berechnet aus Treffern × Power × Combo × Speed

### Statistiken
- Treffer/Fehlschläge, Genauigkeit (%)
- Max/Avg Power, Max Combo
- Durchschnittliche Reaktionszeit
- Schlag-Verteilung nach Richtung
- Session-History mit Level-Filter
- Gesamtstatistiken über alle Sessions

### Sensor (vorbereitet)
- **Touch-Modus**: Placeholder für Development
- **BLE-Sensor**: `BlePunchDetector` mit Richtungsklassifikation
- **SensorService**: Abstraktionsschicht zwischen Touch und BLE

## Steuerung (Touch-Modus)

| Aktion | Effekt |
|--------|--------|
| Tap (kurz) | Leichter Schlag (20%) |
| Halten 0.5s | Mittlerer Schlag (50%) |
| Halten 1s | Starker Schlag (80%) |
| Halten 2s+ | Maximaler Schlag (100%) |

## Sensor-Protokoll

```json
{
  "acceleration": { "x": 0.5, "y": -0.3, "z": 9.8 },
  "impact": 0.85
}
```

| Sensor-Daten | Erkennung |
|-------------|-----------|
| Y > 0.7 | Hoher Schlag |
| Y < -0.7 | Tiefer Schlag |
| X < -0.5 | Linker Schlag |
| X > 0.5 | Rechter Schlag |

## Build

```bash
dotnet restore
dotnet build -f net8.0-android    # Android
dotnet build -f net8.0-ios        # iOS (macOS only)
dotnet build -f net8.0-windows10.0.19041.0  # Windows
```

## Dependencies

- .NET 8.0 + MAUI
- CommunityToolkit.Mvvm
- Plugin.BLE (BLE Sensor)

## Projektstruktur

```
PunchReha/
├── Models/          → PunchEvent, Target, GameStats, GameLevel
├── Services/        → GameEngine, SensorService, TouchPunchDetector, BlePunchDetector, SessionStorage
├── Converters/      → XAML Value Converters
├── ViewModels/      → Menu, Game, Stats, History (MVVM)
├── Views/           → XAML Pages
├── Platforms/       → Platform-specific code
└── MauiProgram.cs   → DI Setup
```
