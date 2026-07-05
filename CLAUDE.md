# Spoorly — kontext projektu

Tenhle soubor čte Claude automaticky na začátku každé session (Claude Code i Cowork,
pokud míří na tuhle složku). Drž ho krátký a vysokosignálový — je to **kontrakt chování**,
ne dokumentace. Pravidlo: pokud řádek nemění, jak se mám chovat, smaž ho.

## Přehled

- **Spoorly** je geoprostorový tracker GPS aktivit (běh, cyklo, turistika).
- **Účel je primárně učení** — je to strukturovaná cesta Honzy skrz moderní .NET,
  ne produkt na trh. Kvalita učení > rychlost dodání.
- Honza: .NET vývojář ~6 měsíců v první formální roli (dřív 10 let metrologie/CMM,
  magistr geodézie a kartografie ČVUT). Geoprostorová doména je jeho reálná expertíza —
  u výpočtů vzdáleností/nadmořky se na ni dá spolehnout.

## Tech stack

- **.NET 10**, C# (`ImplicitUsings enable`, `Nullable enable`)
- **PostgreSQL + PostGIS** (přijde ve fázi EF Core, viz níže)
- Cíl nasazení: Docker → Hetzner VPS + Caddy
- Testy: xUnit + NSubstitute (přijde ve fázi testování — zatím žádný testovací projekt není)

## Struktura repa

- `src/Spoorly.Core/` — doménová logika (žádné závislosti na I/O frameworcích):
  - `Model/` — `Gpx`, `Track`, `TrackSegment`, `TrackPoint` (records)
  - `Io/GpxReader.cs` — načtení `.gpx` (XML)
  - `Geo/Distance.cs` — vzdálenosti bodů: `Equirectangular` (2D), `Slope` (3D s převýšením)
  - `Analysis/` — `TrackStatistics(Calculator)` (souhrnné metriky) a
    `TrackProfile(Builder)` (trasa rozvinutá podél vzdálenostní osy pro výškový profil)
- `src/Spoorly.Console/` — spustitelná ukázka/scratchpad (`Program.cs`), zatím bez API
- `data/` — testovací `.gpx` soubory (`test1.gpx`, `etapa01_den02.gpx`)
- `docs/DENIK.md` — deník projektu (viz níže)
- (`src/Spoorly.Api/` — Minimal API, pozdější fáze)

## Příkazy

- Build: `dotnet build`
- Spuštění: `dotnet run --project src/Spoorly.Console`
- Testy: `dotnet test` (až vznikne testovací projekt)
- Formát: `dotnet format`
- (DB v Dockeru přidáme až ve fázi EF Core.)

## Aktuální fáze — Fáze 1: C# do hloubky přes GPX parsing

Hotové základy: GPX parsing (`GpxReader`), datový model, výpočet vzdálenosti
(`Equirectangular` + skloněná `Slope`), souhrnné statistiky a `TrackProfile`.

Fáze 1 je čistě doménová logika a C#. **Žádné API, žádná DB, žádné EF Core** — ty přijdou
později. `IActivityParser` (strategy pattern) je teprve Fáze 2 (OOP), takže teď stačí
konkrétní `GpxReader`, ale piš ho tak, aby šel později schovat za rozhraní.

Na spadnutí: testovací projekt (xUnit), případně přesnější metody vzdálenosti
(Haversine/Vincenty) pro delší úseky — Honzova doména, ptej se na jeho preferenci.

## Pracovní pravidla

- **Ve fázích základů (1–2) kód primárně VYSVĚTLUJ a KRITIZUJ, negeneruj celá řešení.**
  Cíl je, aby Honza kód napsal a pochopil sám. Když píšu kód, vždy vysvětlím *proč* a
  nabídnu alternativy. Nechci „collage coding" (kopírování z AI bez pochopení).
- Než navrhnu abstrakci, zeptej se, jestli ji fáze potřebuje. Ve Fázi 1 preferuj
  jednoduchost před generickými rozhraními.
- Preferuj standardní knihovnu před závislostmi; když navrhuju balíček, zdůvodni to.
- Ukazuj idiomatické moderní C# (records, nullable reference types, pattern matching,
  LINQ, `Span<T>` kde dává smysl), ale vysvětli tradeoff, ne jen „takhle se to dělá".
- U geoprostorových výpočtů nepodceňuj Honzovu doménu — spíš se ptej na jeho preferenci
  (přesnost vs. jednoduchost), než abych rozhodl za něj.

## Ověřování

- Po každé změně kódu spusť `dotnet build`, u logiky i `dotnet test` (až budou testy).
- Nahlas každý příkaz, který se nepodařilo spustit.

## Styl kódu

- 4mezerové odsazení, `nullable enable`, `ImplicitUsings enable`.
- Názvy anglicky (typy, metody), komentáře můžou být česky.

## Kam si psát průběh

Souhrny sessions, rozhodnutí a další kroky patří do `docs/DENIK.md` (verzováno v gitu,
čtou ho oba nástroje). Na konci session ho aktualizuj. Zápisy drž **stručné a bodové** —
jen hlavní myšlenky (viz „Formát záznamu" v deníku).
