# Spoorly — deník

Sdílená paměť projektu napříč nástroji (Claude Code, Cowork, chat) i časem.
Na konci každé session přidej záznam nahoru. Commituj do gitu — tím se z toho stává
trvalá, přenositelná historie, kterou žádný nástroj neztratí.

**Prompt na konec session:** *„Připiš do DENIK.md, co jsme dnes udělali, jaká padla
rozhodnutí a co je další krok."*

---

## Formát záznamu

```
## [RRRR-MM-DD] — krátký titulek
**Fáze:** <číslo a název>
**Uděláno:** co konkrétně vzniklo/změnilo se
**Rozhodnutí:** volby a *proč* (tohle je nejcennější — ať to future-Honza pochopí)
**Naučeno:** koncept/pattern, který teď dává větší smysl
**Otevřené otázky / další krok:** co dál
**Commity:** hashe nebo stručně
```

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
