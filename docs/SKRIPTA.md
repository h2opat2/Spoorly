# Spoorly — Skripta

Učební text projektu. Píše ho Claude jako výklad teorie **před** tím, než ji Honza
aplikuje v kódu. Není to řešení konkrétních úloh (ty píše Honza sám) — je to referenční
výklad konceptů, ke kterému se dá vracet. Česky, technické termíny anglicky.

Souvisí s [DENIK.md](DENIK.md) (co jsme kdy udělali) — skripta jsou *proč a jak*, deník je *kdy a co*.

## Index

- [1. Testování (xUnit)](#1-testování-xunit)

---

# 1. Testování (xUnit)

## 1.1 Proč vůbec testovat

Test je kód, který spouští jiný kód a ověřuje, že se chová podle očekávání. Tři hlavní
důvody, proč se to vyplatí (v tomhle pořadí důležitosti pro tebe):

1. **Zpětná vazba k designu.** Špatně se ti něco testuje? Skoro vždy je to signál, že
   ten kód má moc závislostí nebo dělá moc věcí. Testy tě tlačí k lepší architektuře.
   (Tvoje čisté funkce v `Distance`/`Calculator` se testují triviálně — to je *důkaz*,
   že jsou dobře navržené.)
2. **Ochrana proti regresi.** Až budeš ve Fázi 2 refaktorovat `GpxReader` za rozhraní,
   testy ti řeknou „nerozbil jsi počítání vzdálenosti". Bez nich refaktoruješ naslepo.
3. **Živá dokumentace.** Dobře pojmenovaný test říká, *co* má metoda dělat, přesněji než
   komentář — a na rozdíl od komentáře nemůže „zestárnout", protože by přestal procházet.

Co testy **nejsou**: důkaz správnosti. Testují jen scénáře, které tě napadly. Proto se
soustředíme na hraniční případy (viz [1.9](#19-hraniční-případy)), kde chyby žijí.

## 1.2 Druhy testů — test pyramid

Testy se liší podle toho, jak velký kus systému berou najednou:

```
        /\        e2e / UI    — celá appka, pomalé, křehké, málo jich
       /  \       integration — víc komponent + I/O (disk, DB), střední
      /____\      unit        — jedna jednotka izolovaně, rychlé, hodně jich
```

- **Unit test** — jedna „jednotka" (typicky metoda/třída) *bez* vnějších závislostí.
  Rychlý (milisekundy), deterministický. Sem patří `Distance`, `TrackStatisticsCalculator`,
  `TrackProfileBuilder` — jsou to čisté funkce, nesahají na disk ani čas.
- **Integration test** — ověřuje spolupráci víc částí, často včetně I/O. Sem patří
  `GpxReader.Load(path)` — čte reálný soubor z disku. Je pomalejší a potřebuje fixture
  (testovací data), proto se drží stranou od unit testů.
- **e2e** — celá aplikace zvenčí. Pro Spoorly zatím není co.

**Pravidlo palce (test pyramid):** hodně rychlých unit testů dole, málo pomalých nahoře.
Když jde logika vyjádřit jako čistá funkce, dá se pokrýt unit testem — a to je levné.

## 1.3 Anatomie testu: AAA (Arrange–Act–Assert)

Každý test má tři fáze, ideálně i vizuálně oddělené:

```csharp
[Fact]
public void Add_TwoPositiveNumbers_ReturnsSum()
{
    // Arrange — připrav vstupy a systém do známého stavu
    var calculator = new Calculator();

    // Act — proveď JEDNU akci, kterou testuješ
    var result = calculator.Add(2, 3);

    // Assert — ověř výsledek
    Assert.Equal(5, result);
}
```

Zásady:
- **Jedna logická akce na test** (jeden „Act"). Když testuješ dvě věci, rozděl na dva testy —
  jinak při pádu nevíš, která část selhala.
- **Assert na jedno chování.** Víc `Assert` je OK, pokud ověřují jeden koncept (např. že
  statistika má správnou vzdálenost *i* počet bodů), ne pět nesouvisejících věcí.
- Test má být **deterministický** — stejný vstup → stejný výsledek, vždy. Žádné `DateTime.Now`,
  žádné náhodné hodnoty, žádné pořadí závislé na jiném testu.

## 1.4 Pojmenování testů

Název testu je jeho dokumentace. Osvědčený vzorec:

```
Metoda_Scénář_OčekávanéChování
```

Příklady, jak by mohly vypadat názvy pro Spoorly (jen názvy — tělo píšeš ty):
- `Equirectangular_SamePoint_ReturnsZero`
- `Compute_EmptySegment_ReturnsEmpty`
- `Build_MultipleSegments_DoesNotCountGapBetweenSegments`

Když test spadne, chceš z názvu v CI logu hned vědět, co je špatně, bez otevírání kódu.

## 1.5 xUnit prakticky: `[Fact]` vs `[Theory]`

**`[Fact]`** = jeden konkrétní případ (jeden vstup, jeden výsledek). Viz příklad v 1.3.

**`[Theory]`** = stejná logika testu pro víc sad vstupů. Když bys jinak psal pět skoro
identických `[Fact]` lišících se jen čísly, použij `[Theory]`:

```csharp
[Theory]
[InlineData(2, 3, 5)]
[InlineData(-1, 1, 0)]
[InlineData(0, 0, 0)]
public void Add_VariousInputs_ReturnsSum(int a, int b, int expected)
{
    var result = new Calculator().Add(a, b);
    Assert.Equal(expected, result);
}
```

- `[InlineData(...)]` — konstantní sady přímo u testu. Ideální pro čísla.
- `[MemberData(nameof(Source))]` — když potřebuješ složitější vstupy (objekty, pole),
  které nejdou zapsat jako konstanty do atributu. Data dodá statická property/metoda.

Pro tebe je `[Theory]` přímo stvořená na testy vzdáleností (víc dvojic souřadnic → očekávané metry).

## 1.6 Co dělá kód testovatelným

Tři vlastnosti, každou máš v projektu na čem ukázat:

- **Čistota (pure function)** — výstup závisí *jen* na vstupních argumentech, žádný vedlejší
  efekt, žádný skrytý stav. `Distance.Equirectangular(p1, p2)` je učebnicově čistá:
  dáš dva body, dostaneš číslo, pokaždé stejné. Testuje se bez jakékoli přípravy prostředí.
- **Determinismus** — žádná závislost na čase, náhodě, globálním stavu. `Calculator.Compute`
  bere čas z *dat* (`TrackPoint.Time`), ne z `DateTime.Now` → deterministické. Kdyby volalo
  `DateTime.Now`, nešlo by spolehlivě testovat trvání.
- **Dependency injection** — závislosti se předávají zvenčí, ne vytvářejí uvnitř. Všimni si,
  že `Compute` i `Build` berou `Func<TrackPoint,TrackPoint,double>? pointDistance` — můžeš
  jim podstrčit vlastní funkci vzdálenosti. To je DI přes delegát (víc ve Fázi C, kde ho
  povýšíme na rozhraní).

Naopak `GpxReader.Load` má **závislost na disku** (přečte soubor). Tu neizoluješ — proto je
to integration test s reálným fixture souborem, ne unit test.

## 1.7 Test doubles — a proč je teď NEPOTŘEBUJEŠ

„Test double" je náhrada skutečné závislosti v testu. Taxonomie (podle Fowlera/Meszarose):

- **Dummy** — objekt jen aby se vyplnil parametr, nepoužije se.
- **Stub** — vrací předpřipravené odpovědi („když se zeptáš, vrať 42").
- **Spy** — stub, který navíc zaznamenává, jak byl volán.
- **Mock** — předem naprogramovaný s očekáváním („MUSÍŠ mě zavolat přesně jednou").
- **Fake** — funkční, ale zjednodušená implementace (např. in-memory databáze).

**Klíčový poznatek:** doubles potřebuješ jen tam, kde máš **závislost, kterou chceš izolovat**
(databáze, síť, čas, cizí služba). Tvé čisté funkce žádnou takovou nemají → **žádné mocky.**
Knihovnu **NSubstitute** (z vašeho stacku) proto teď nasazovat nebudeme — přijde ve Fázi C,
až vznikne `IActivityParser` a budeš chtít otestovat kód, který ho používá, bez skutečného
parsování souboru. Časté anti-pattern začátečníků je mockovat všechno; ty máš luxus nemuset.

## 1.8 Porovnávání `double` — nikdy `==`

Desetinná čísla (`double`) jsou v počítači uložená binárně a většina „hezkých" hodnot se
nedá vyjádřit přesně. Klasika: `0.1 + 0.2 == 0.3` je **`false`** (vyjde 0.30000000000000004).
U geodetických výpočtů (odmocniny, `cos`, násobení poloměrem Země) se zaokrouhlovací chyby
hromadí, takže rovnost na přesnou hodnotu prakticky nikdy nenastane.

Proto se porovnává **s tolerancí**. xUnit má na to overloady:

```csharp
// varianta A: shoda na N desetinných míst (stabilní, dlouho dostupná)
Assert.Equal(111319.49, actual, precision: 2);

// varianta B: absolutní tolerance (novější xUnit)
Assert.Equal(expected, actual, tolerance: 0.5);

// varianta C: ručně, když chceš plnou kontrolu
Assert.True(Math.Abs(expected - actual) < 0.5, $"bylo {actual}");
```

Jak zvolit toleranci: podle **fyzikálního významu**, ne náhodně. U vzdálenosti v metrech na
trase je půl metru naprosto v pohodu; nemá smysl trvat na nanometrech, když sama
equirektangulární aproximace má větší chybu. Tolerance = „jak přesně mi na tom záleží".

## 1.9 Hraniční případy (edge cases)

Chyby nebývají v běžném průchodu, ale na okrajích. Než napíšeš test, projdi vstupní prostor
a hledej hranice. Pro Spoorly konkrétně:

- **Prázdno** — segment/trasa bez bodů. (Kód na to má `Empty` — test to má potvrdit.)
- **Jeden prvek** — jediný bod: nulová vzdálenost, min = max = jeho výška, žádné trvání.
- **Chybějící volitelná data** — bod bez `ele`, bod bez `time`. Co se má stát? (Fallback,
  `null`, přeskočení — ověř, že to odpovídá záměru.)
- **Přechody/hranice** — přechod mezi dvěma segmenty: vzdálenost přes „pauzu" se **nemá**
  započítat, ale kumulativní osa profilu pokračuje. To je přesně místo, kde snadno vznikne chyba.
- **Známé hodnoty** — tam, kde umíš výsledek spočítat ručně (1° šířky ≈ 111 km), použij to
  jako kotvu. Doménová znalost geodeta je tady tvoje výhoda.

Dobrý zvyk: ke každé „šťastné cestě" napiš aspoň jeden hraniční test.

## 1.10 Struktura test projektu v .NET

Konvence: testy jsou **samostatný projekt**, ne součást produkčního kódu (aby se testovací
frameworky nedostaly do release buildu).

- Adresář `tests/`, projekt `Spoorly.Core.Tests` (jméno = testovaný projekt + `.Tests`).
- Šablona `dotnet new xunit` založí projekt s xUnit a test runnerem.
- Projekt musí mít **referenci** na `Spoorly.Core` (jinak nevidí, co testovat).
- Přidat do [Spoorly.slnx](../Spoorly.slnx), aby byl součástí solution.
- Spouští se `dotnet test` (najde všechny testy v solution a spustí je).

Konkrétní příkazy si projdeme spolu při setupu — schválně, ať víš, co který dělá, ne že je
jen opíšeš. To je tvoje první úloha ve Fázi A.

## 1.11 Jak to aplikovat na Spoorly

Cíle jsou popsané v [plánu](../../.claude/plans) i níže — **co** testovat; **jak** (tělo testů)
píšeš ty, já reviduju. Od nejčistších (unit) po integrační:

1. `Distance.Equirectangular` — známé vzdálenosti (`[Theory]`), symetrie, stejný bod → 0. Tolerance!
2. `Distance.Slope` — s výškou (Pythagoras) i bez ní (fallback na horizontální).
3. `TrackStatisticsCalculator.Compute(segment)` — prázdný, jeden bod, gain/loss, duration,
   `AverageSpeed`, body bez výšky.
4. `Merge` / multi-segment — vzdálenosti se sčítají, mezera mezi segmenty se nepočítá.
   (Tady si řekneme o *moving time* vs *elapsed time* — je to doménové rozhodnutí.)
5. `TrackProfileBuilder.Build` — monotónní osa, hranice segmentu, prázdno, `TotalDistance`.
6. `GpxReader.Load` — **integration** nad `data/test1.gpx`: počty a hodnoty prvního bodu.

## Další čtení (nepovinné prohloubení)

- Microsoft Learn — testing hub: `learn.microsoft.com/dotnet/core/testing/`
  a „Unit testing best practices".
- xUnit dokumentace: `xunit.net/docs` → „Getting Started".
- Kniha: Vladimir Khorikov — *Unit Testing Principles, Practices, and Patterns* (Manning, C#/.NET).
- NSubstitute: `nsubstitute.github.io` (až Fáze C).
