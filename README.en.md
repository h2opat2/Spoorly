# Spoorly

*[🇨🇿 Čeština](README.md) · 🇬🇧 English*

> A geospatial tracker for GPS activities (running, cycling, hiking) built in modern .NET.

![.NET](https://img.shields.io/badge/.NET-10-512BD4)
![C#](https://img.shields.io/badge/C%23-nullable%20enable-239120)
![Tests](https://img.shields.io/badge/tests-xUnit-informational)

Spoorly reads `.gpx` track recordings and derives meaningful metrics from them —
distance (both flat and slope-adjusted for elevation), elevation gain/loss, duration,
average speed, and an elevation profile unrolled along a distance axis.

## About

This is primarily a **learning project** — a structured journey through modern .NET
(domain → OOP → EF Core → API → deployment), not a market product. The focus is on
understanding and idiomatic code rather than delivery speed. The author has a background
in geodesy and cartography, so the geospatial parts (distance and elevation calculations)
are written with domain knowledge.

Progress and decisions are kept in [`docs/DENIK.md`](docs/DENIK.md) (a project journal,
in Czech) and conceptual write-ups in [`docs/SKRIPTA.md`](docs/SKRIPTA.md) (in Czech).

## Features (current state)

- **GPX parsing** (`IActivityParser` / `GpxReader`) via the strategy pattern — ready for
  additional formats (TCX/FIT) without touching the rest of the code; the parser is
  selected by file extension (`ActivityParserFactory`).
- **Domain model** as immutable `record`s: `Activity → Track → TrackSegment → TrackPoint`.
- **Distance calculations** (`Distance`): `Equirectangular` (2D) and `Slope` (3D with elevation).
- **Summary statistics** (`TrackStatisticsCalculator`): distance, elevation ±, min/max
  elevation, duration, average speed; the gap between segments is not counted toward
  distance (a pause / GPS dropout).
- **Elevation profile** (`TrackProfileBuilder`): the track unrolled along a distance axis.
- **Tests** (xUnit) covering the domain logic and parsing.

## Repository layout

```
src/
  Spoorly.Core/            domain logic, no dependencies on I/O frameworks
    Model/                 Activity, Track, TrackSegment, TrackPoint (records)
    Io/                    IActivityParser, GpxReader, ActivityParserFactory
    Geo/                   Distance (Equirectangular, Slope)
    Analysis/              TrackStatisticsCalculator, TrackProfileBuilder
  Spoorly.Console/         runnable demo (loads a .gpx and prints metrics + profile)
tests/
  Spoorly.Core.Tests/      xUnit tests
data/                      sample .gpx files
docs/                      DENIK.md (journal), SKRIPTA.md (learning notes)
```

## Quick start

Requires the **.NET 10 SDK**.

```bash
# build
dotnet build

# tests
dotnet test

# run the demo (mind the working directory — the demo reads data/ relatively)
cd src/Spoorly.Console && dotnet run

# format
dotnet format
```

## Roadmap (learning phases)

- [x] **Phase 1 — C# in depth via GPX:** parsing, model, distances, statistics, profile
- [x] **Phase 2 — OOP:** strategy pattern (`IActivityParser` + factory), immutable model
- [ ] More accurate distances (Haversine / Vincenty) for longer segments
- [ ] EF Core + PostgreSQL / PostGIS (persistence)
- [ ] Minimal API
- [ ] Docker → Hetzner VPS + Caddy

## Tech stack

.NET 10 · C# (`nullable enable`, `ImplicitUsings enable`) · xUnit · `System.Xml.Linq` (GPX).
Planned: EF Core, PostgreSQL + PostGIS, Docker.

## License

Personal learning project, no license yet (default "all rights reserved").
Feel free to draw inspiration; please reach out before reusing anything.
