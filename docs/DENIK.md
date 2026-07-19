# Spoorly — deník

Sdílená paměť projektu napříč nástroji (Claude Code, Cowork, chat) i časem.
Na konci každé session přidej záznam nahoru. Commituj do gitu — tím se z toho stává
trvalá, přenositelná historie, kterou žádný nástroj neztratí.

**Prompt na konec session:** *„Připiš do DENIK.md, co jsme dnes udělali, jaká padla
rozhodnutí a co je další krok."*

---

## Formát záznamu

Drž zápis **stručný a bodový** — jen hlavní myšlenky, ne odstavce (ideálně 3–6 řádků).
Pole, ke kterému není co říct, vynech.

```
## [RRRR-MM-DD] — krátký titulek
**Fáze:** <číslo a název>
- **Uděláno:** co vzniklo (bodově)
- **Rozhodnutí:** klíčová volba + proč (jen to podstatné)
- **Naučeno:** hlavní koncept
- **Další krok:** co dál
- **Commity:** hashe
```

---

## [2026-07-19] — PostGIS běží v Dockeru + zafixovaný návrh uložení

**Fáze:** 3 — perzistence
- **Uděláno:** `docker-compose.yml` (image `postgis/postgis:16-3.4`, volume, port 5432) + `.env` (heslo, v `.gitignore`). Docker Desktop nainstalován, DB naživo — ověřeno `PostgreSQL 16.4` + `PostGIS 3.4`, `ST_MakePoint` na reálném bodě z GPX.
- **Rozhodnutí (návrh uložení):** geometrie = **`MULTILINESTRING ZM`**, 1 záznam na aktivitu (části = segmenty s mezerami). Metriky **spočítat při importu a uložit** pro rychlé dotazy. Vzdálenost = **2D geodetická** (`ST_Length(geography)`) **oddělená od převýšení** (Z zvlášť) — 3D délka u vteřinových vzorků nemá v terénu smysl (geodetova úvaha).
- **Naučeno:** image vs kontejner; pojmenovaný volume = perzistence dat mimo kontejner; pořadí souřadnic **X=lon, Y=lat** naživo; heslo přes `.env` mimo git.
- **Další krok:** .NET strana — projekt `Spoorly.Data`, balíčky Npgsql EF provider + NetTopologySuite, `SpoorlyDbContext` + entita `Activity` (`MULTILINESTRING ZM` + souhrn), první migrace. Začneme **návrhem entity/schématu**.
- **Commity:** — (`docker-compose.yml` zatím nezacommitován)

---

## [2026-07-09] — Skripta kap. 3: EF Core + PostgreSQL/PostGIS (start Fáze 3)

**Fáze:** 3 — perzistence
- **Uděláno:** README (CZ [README.md](../README.md) + EN [README.en.md](../README.en.md), přepínač jazyka). Skripta **kap. 3 — Perzistence: EF Core + PostgreSQL/PostGIS** (podrobná, na Honzovu žádost — kariérně důležité pro GIS): PostGIS (geometry vs geography, SRID 4326, GiST, pořadí X=lon/Y=lat), EF Core (DbContext, migrace, LINQ→SQL), Npgsql + NetTopologySuite, model uložení (normalizovaně vs LINESTRING ZM vs hybrid), records vs DB entity, doména vs PostGIS pushdown, Docker `postgis/postgis`.
- **Rozhodnutí:** další fáze = perzistence (EF Core + PostGIS), Honza se těší; licenci zatím neřešíme.
- **Další krok:** rozhodnout designové otázky (3.14) — hlavně **model uložení** (LINESTRING vs relačně) a **kam s `DbContext`** (nový projekt `Spoorly.Data`, ať `Core` zůstane bez EF). Pak Docker + PostGIS, balíčky, `SpoorlyDbContext`, první migrace, import/round-trip.
- **Commity:** —

---

## [2026-07-09] — Fáze 2: strategy pattern hotový (IActivityParser + factory)

**Fáze:** 2 — OOP
- **Uděláno:** `IActivityParser` (kontrakt přes `Stream`, v `Io`), `GpxReader : IActivityParser` (instanční `Parse`, `XDocument.Load(stream)`), model přejmenován `Gpx` → `Activity`, `ActivityParserFactory.ForFile` (výběr dle přípony, `NotSupportedException` pro neznámou). Console jede přes rozhraní + factory (`File.OpenRead` + `using`). Testy factory + `GpxReader` z `MemoryStream` → 19/19.
- **Rozhodnutí:** vstup `Stream` (ne path) → testovatelnost bez disku; výběr parseru přes `switch` na příponě (registry zbytečný pro 1 formát); return typ factory = `IActivityParser` (drží abstrakci).
- **Naučeno:** `static` v rozhraní zabíjí polymorfismus (strategy = instanční metody); `Stream` je `IDisposable` → `using`; špatně umístěný typ „prosakuje" importy (přesun `IActivityParser` do `Io` smazal `using Model` ve factory).
- **Další krok:** fake `IActivityParser` do konzumenta až vznikne `ActivityService`; případně `TcxReader` jako druhý formát (ověří smysl factory). Pak dál po learning path (EF Core / API).
- **Commity:** —

---

## [2026-07-08] — Dokončení testů Calculatoru + start Fáze 2 (OOP)

**Fáze:** přechod Fáze 1 → Fáze 2 (OOP)
- **Uděláno:** `TrackStatisticsCalculatorTests` (empty, sčítání úseků přes stub, gain/loss, min/max, duration, chybějící výška, mezera mezi segmenty) — 13/13 zelených, testovací vsuvka uzavřena. Skripta kap. **2. OOP: rozhraní a strategy pattern (`IActivityParser`)**.
- **Rozhodnutí:** další kapitola = Fáze 2 OOP (schovat `GpxReader` za `IActivityParser`). Testy dál píše Claude (Honzu nebavily), učební energie jde na doménu/C#.
- **Naučeno:** strategy pattern = zaměnitelné algoritmy za rozhraním; `Func<>` v `Slope`/`Compute` už je jeho lehká forma. YAGNI: s jedním formátem je abstrakce předčasná — zavádíme ji vědomě kvůli učení + TCX/FIT.
- **Další krok:** rozhodnout designové otázky (2.6): návratový typ (`Gpx` → `Activity`?), vstup (`path` vs `Stream`); pak `GpxParser : IActivityParser` + úprava Console.
- **Commity:** —

---

## [2026-07-05] — Testy Distance.Slope

**Fáze:** Testování (Fáze A) — v rámci Fáze 1
- **Uděláno:** `DistanceTests` pro `Slope` — stubnutá horizontální složka (izolace Pythagora) + fallback při chybějícím `Elevation` u jednoho/obou bodů.
- **Naučeno:** injektovat horizontální vzdálenost přes stub → test čistě ověří výškovou složku, nezávisle na 2D vzorci.
- **Další krok:** testy `TrackStatisticsCalculator` (empty, gain/loss, min/max, duration, chybějící elevation, merge segmentů).
- **Commity:** `4cf26e6`

---

## [2026-07-05] — Zahájení fáze testování: xUnit + první testy Distance

**Fáze:** Testování (Fáze A) — v rámci Fáze 1
- **Uděláno:** `docs/SKRIPTA.md` (kap. Testování); test projekt `tests/Spoorly.Core.Tests` (xUnit 2.9.3) + `DistanceTests` pro `Equirectangular`.
- **Rozhodnutí:** xUnit v2, bez NSubstitute (čisté funkce); `precision: 1` (tolerance dle přesnosti `expected`).
- **Naučeno:** `double` neporovnávat přes `==`; minimální fixture; nezávislá kotva pro `expected`.
- **Další krok:** `Distance.Slope` → `TrackStatisticsCalculator` (moving vs elapsed) → `TrackProfileBuilder` → `GpxReader` (integration).
- **Commity:** `800680b`, `baedabb`, `f7c3d9c`

---

## [2026-07-03] — Automatická připomínka deníku po commitu

**Fáze:** 1 — C# do hloubky přes GPX parsing
**Uděláno:** Přidán `.claude/settings.json` s `PostToolUse` hookem (matcher `Bash`,
filtr `if: Bash(git commit*)`), který po commitu vloží Claudovi připomínku, ať doplní
tenhle deník. Verzovaný v repu, takže platí pro každého, kdo v projektu jede Claude Code.
**Rozhodnutí:**
- Zvolen **připomínkový** hook (Claude napíše smysluplný záznam), ne mechanický
  `post-commit` git hook — ten by uměl jen holý řádek hash + zpráva, bez rozhodnutí
  a kontextu, což je přesně to nejcennější v deníku.
- **Loop-guard:** když zpráva commitu obsahuje „deník/denik/DENIK", hook se přeskočí —
  aby commit samotné aktualizace deníku nevyvolal další připomínku donekonečna.
- Scope = projektový `.claude/settings.json` (ne `.local`), aby konvence cestovala s repem.
**Naučeno:** Hook je jen shellový příkaz — sám neumí napsat obsah vyžadující úvahu;
`PostToolUse` ale umí vrátit `hookSpecificOutput.additionalContext` a tím „šťouchnout"
model, aby to dopsal. Bash permission-matching pro `if` filtr rozebírá i složené příkazy.
**Otevřené otázky / další krok:** Připomínka se spustí u každého commitu — pokud to bude
moc, zvážit omezení jen na commity měnící `src/`. Zpět k Fázi 1: testovací projekt (xUnit).
**Commity:** —

---

## [2026-07-03] — Založení lokálního kontextu v repu

**Fáze:** 1 — C# do hloubky přes GPX parsing
**Uděláno:** Do kořene repa přišel projektový `CLAUDE.md` (vychází z nadřazeného briefu
z Coworku, ale sladěný se skutečným stavem kódu) a tenhle deník se přesunul do `doc/`.
Repo je teď kanonický zdroj kontextu; Claude Code i Cowork míří na stejnou složku.
**Rozhodnutí:**
- Kontext držíme v repu (verzovaný v gitu), ne ve webovém Projectu ani jen v Obsidianu —
  aby ho viděly oba agentické nástroje.
- Deník bydlí v `docs/DENIK.md`.
- Ve Fázi 1 zůstáváme u konkrétního `GpxReader` bez rozhraní; `IActivityParser`
  (strategy pattern) až ve Fázi 2.
**Naučeno:** Cowork a Claude Code mají oddělené sklady paměti — spojuje je repo, ne sync.
**Otevřené otázky / další krok:**
- Doplnit testovací projekt (`tests/Spoorly.Core.Tests`, xUnit) — zatím žádné testy nejsou.
- Zvážit, jestli u vzdálenosti zůstat u Equirectangular, nebo přidat Haversine/Vincenty
  pro delší úseky (Honzova doména — přesnost vs. jednoduchost).
**Commity:** —

---

<!-- Nové záznamy přidávej sem nahoru, nad tuhle čáru. -->
