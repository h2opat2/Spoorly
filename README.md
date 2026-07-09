# Spoorly

*🇨🇿 Čeština · [🇬🇧 English](README.en.md)*

> Geoprostorový tracker GPS aktivit (běh, cyklo, turistika) psaný v moderním .NET.

![.NET](https://img.shields.io/badge/.NET-10-512BD4)
![C#](https://img.shields.io/badge/C%23-nullable%20enable-239120)
![Tests](https://img.shields.io/badge/tests-xUnit-informational)

Spoorly načítá `.gpx` záznamy tras a počítá z nich smysluplné metriky — vzdálenost
(rovinnou i skloněnou přes převýšení), nastoupané/naklesané metry, dobu, průměrnou
rychlost a výškový profil rozvinutý podél vzdálenostní osy.

## O projektu

Je to primárně **učební projekt** — strukturovaná cesta skrz moderní .NET (doména →
OOP → EF Core → API → deploy), ne produkt na trh. Důraz je na pochopení a idiomatický
kód, ne na rychlost dodání. Autor má zázemí v geodézii a kartografii, takže
geoprostorová část (výpočty vzdáleností a převýšení) je psaná se znalostí domény.

Průběh a rozhodnutí jsou vedené v [`docs/DENIK.md`](docs/DENIK.md) (deník) a teoretický
výklad konceptů v [`docs/SKRIPTA.md`](docs/SKRIPTA.md).

## Co umí (aktuální stav)

- **Parsing GPX** (`IActivityParser` / `GpxReader`) přes strategy pattern — připraveno na
  další formáty (TCX/FIT) bez zásahu do zbytku kódu; výběr parseru podle přípony
  (`ActivityParserFactory`).
- **Datový model** jako immutable `record`y: `Activity → Track → TrackSegment → TrackPoint`.
- **Výpočty vzdálenosti** (`Distance`): `Equirectangular` (2D) a `Slope` (3D s převýšením).
- **Souhrnné statistiky** (`TrackStatisticsCalculator`): délka, převýšení ±, min/max výška,
  doba, průměrná rychlost; mezera mezi segmenty se do vzdálenosti nezapočítává (pauza/výpadek GPS).
- **Výškový profil** (`TrackProfileBuilder`): trasa rozvinutá podél vzdálenostní osy.
- **Testy** (xUnit) na doménovou logiku i parsing.

## Struktura repa

```
src/
  Spoorly.Core/            doménová logika bez závislostí na I/O frameworcích
    Model/                 Activity, Track, TrackSegment, TrackPoint (records)
    Io/                    IActivityParser, GpxReader, ActivityParserFactory
    Geo/                   Distance (Equirectangular, Slope)
    Analysis/              TrackStatisticsCalculator, TrackProfileBuilder
  Spoorly.Console/         spustitelná ukázka (načte .gpx a vypíše metriky + profil)
tests/
  Spoorly.Core.Tests/      xUnit testy
data/                      ukázkové .gpx soubory
docs/                      DENIK.md (deník), SKRIPTA.md (učební text)
```

## Rychlý start

Potřebuješ **.NET 10 SDK**.

```bash
# build
dotnet build

# testy
dotnet test

# spuštění ukázky (pozor na pracovní adresář — ukázka čte data/ relativně)
cd src/Spoorly.Console && dotnet run

# formát
dotnet format
```

## Roadmapa (fáze učení)

- [x] **Fáze 1 — C# do hloubky přes GPX:** parsing, model, vzdálenosti, statistiky, profil
- [x] **Fáze 2 — OOP:** strategy pattern (`IActivityParser` + factory), immutable model
- [ ] Přesnější vzdálenosti (Haversine / Vincenty) pro delší úseky
- [ ] EF Core + PostgreSQL / PostGIS (perzistence)
- [ ] Minimal API
- [ ] Docker → Hetzner VPS + Caddy

## Tech stack

.NET 10 · C# (`nullable enable`, `ImplicitUsings enable`) · xUnit · `System.Xml.Linq` (GPX).
Plánováno: EF Core, PostgreSQL + PostGIS, Docker.

## Licence

Osobní učební projekt, zatím bez licence (výchozí „všechna práva vyhrazena").
Klidně se inspiruj; než cokoli převezmeš, ozvi se.
