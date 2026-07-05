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

## [2026-07-05] — Zahájení fáze testování: xUnit + první testy Distance

**Fáze:** Testování (Fáze A dle učebního plánu) — pořád v rámci Fáze 1 (doménová logika)
**Uděláno:** Vznikl učební text `docs/SKRIPTA.md` (kapitola Testování). Založen testovací
projekt `tests/Spoorly.Core.Tests` (net10.0, xUnit 2.9.3), reference na `Spoorly.Core`,
přidán do `Spoorly.slnx`. První testy `DistanceTests`: `Equirectangular` pro shodný bod
(→ 0) a `[Theory]` se dvěma ručně ověřenými známými vzdálenostmi.
**Rozhodnutí:**
- xUnit v2 (stabilní), NSubstitute zatím ne — čisté funkce nepotřebují test doubles.
- Tolerance u `double` volena **vědomě**: `precision: 1` místo `0`, ať sedí s přesností
  zadaných `expected` (ne „přesnost na desetinu, ověřuji na jednotky").
- Oddělené commity: testovací infrastruktura zvlášť od formátovacího úklidu `dotnet format`.
**Naučeno:** Porovnávání `double` nikdy přes `==`, `precision` = počet desetinných míst.
Minimální fixture (nastav jen to, co test potřebuje — `Elevation` u 2D výpočtu je šum).
Nezávislá kotva pro `expected` (ruční výpočet z geodetické domény) dělá test cenným.
**Otevřené otázky / další krok:** Doplnit zbytek `Distance` — nejdřív `Slope` (fallback
bez výšky, Pythagoras s výškou). Pak `TrackStatisticsCalculator` (vč. diskuse moving vs
elapsed time u `Merge`), `TrackProfileBuilder`, a integration test `GpxReader` nad fixture.
**Commity:** `800680b` (skripta), `baedabb` (test projekt + DistanceTests), `f7c3d9c` (format)

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
