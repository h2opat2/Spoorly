# Spoorly — Skripta

Učební text projektu. Píše ho Claude jako výklad teorie **před** tím, než ji Honza
aplikuje v kódu. Není to řešení konkrétních úloh (ty píše Honza sám) — je to referenční
výklad konceptů, ke kterému se dá vracet. Česky, technické termíny anglicky.

Souvisí s [DENIK.md](DENIK.md) (co jsme kdy udělali) — skripta jsou *proč a jak*, deník je *kdy a co*.

## Index

- [1. Testování (xUnit)](#1-testování-xunit)
- [2. OOP: rozhraní a strategy pattern (`IActivityParser`)](#2-oop-rozhraní-a-strategy-pattern-iactivityparser)
- [3. Perzistence: EF Core + PostgreSQL / PostGIS](#3-perzistence-ef-core--postgresql--postgis)

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

---

# 2. OOP: rozhraní a strategy pattern (`IActivityParser`)

## 2.1 Kam tahle kapitola míří

Dosud je `GpxReader` **statická třída se statickou metodou** `Load(path)`. Funguje to,
protože formát je jen jeden (GPX). Cíl Fáze 2 je schovat konkrétní čtečku za **rozhraní**
`IActivityParser`, aby zbytek aplikace nezávisel na tom, *jaký* formát se právě parsuje.

Rovnou na férovku, protože jinak by to byl kult cargo programování: **s jediným formátem
je tahle abstrakce technicky předčasná** (YAGNI — „you aren't gonna need it"). Kdyby šlo
o produkt, řekl bych „nech statickou třídu, dokud nepřijde druhý formát". Tady je ale
**cílem naučit se ten pattern** a připravit půdu pro TCX/FIT — takže ho zavedeme *vědomě*
a s pochopením, kdy se vyplatí a kdy je to jen zbytečná vrstva. Ta upřímnost je součást
učení: většina škody z OOP vzniká z abstrakcí přidaných „pro jistotu".

## 2.2 Proč rozhraní: závislost na kontraktu, ne na implementaci

Rozhraní (`interface`) je **čistý kontrakt** — seznam metod bez těla. Kdo ho používá,
ví *co* umí, ne *jak*. To odpojuje volajícího od konkrétní třídy:

```csharp
public interface IActivityParser
{
    Activity Parse(string path);   // co se vrací a jak se to jmenuje = otevřená otázka, viz 2.6
}
```

Tři věci, které tím získáš:

1. **Zaměnitelnost.** `GpxParser`, `TcxParser`, `FitParser` — každý implementuje stejný
   kontrakt. Kód, který parser používá, se nemění, když přibude formát.
2. **Testovatelnost.** Do konzumenta můžeš podstrčit fake parser (přesně ten „test double"
   z [1.7](#17-test-doubles--a-proč-je-teď-nepotřebuješ)) bez sahání na disk.
3. **Explicitní hranice.** Rozhraní pojmenuje, co je „parser aktivity", a odřízne to od
   detailů XML/XLinq.

## 2.3 Strategy pattern jednou větou

**Strategy** = rodina zaměnitelných algoritmů za společným rozhraním; konkrétní se vybírá
za běhu. Tady je „algoritmus" = parser formátu, „rozhraní" = `IActivityParser`, „výběr za
běhu" = podle přípony souboru. Nic víc v tom není — je to prostě „polymorfismus s úmyslem".

Mimochodem: **už jsi jednu strategii použil.** `Func<TrackPoint, TrackPoint, double>`,
který posíláš do `Slope` a `Compute`, je strategy pattern v lehké, funkcionální podobě —
zaměnitelný algoritmus vzdálenosti předaný jako parametr. Rozhraní je jen jeho „těžší",
objektová varianta pro případ, kdy strategie má víc metod nebo vlastní stav.

## 2.4 `interface` vs `abstract class` — co zvolit

| | `interface` | `abstract class` |
|---|---|---|
| Nese stav (pole)? | ne | ano |
| Sdílená implementace? | jen `default` metody (nezneužívat) | ano, běžně |
| Kolik jich třída může mít? | víc | jen jednu (dědičnost) |
| Vztah | „umí tohle" (schopnost) | „je tohle" (identita/rodina) |

Pro parser chceš **`interface`**: parsery nesdílejí stav ani společný základ, jen slibují
stejnou schopnost. `abstract class` by dávala smysl, až kdyby víc parserů sdílelo netriviální
kód (např. společné mapování na model) — a i pak se dnes spíš preferuje kompozice.

## 2.5 Výběr parseru podle formátu — factory / registry

Někdo musí rozhodnout „tenhle `.gpx` → `GpxParser`". Tři úrovně, od nejjednodušší:

1. **`switch` na příponě** přímo v místě volání — nejlevnější, dokud jsou formáty 1–2.
2. **Factory metoda** `IActivityParser ForFile(string path)` — schová `switch` na jedno místo.
3. **Registry** (`IDictionary<string, IActivityParser>`) — parsery se registrují, výběr je
   lookup. Elegantní pro plugin-styl, ale pro dva formáty přehnané.

**Doporučení pro tvou fázi:** začni u (1) nebo (2). Registry je krásný, ale je to zase ta
abstrakce navíc — přijde, až bude formátů víc, ne dřív.

## 2.6 Otevřené designové otázky (rozhodneš ty)

Tohle je jádro Fáze 2 — nejsou to detaily, ale volby, které utvářejí API. Rozmysli je
**před** psaním kódu, ať víš, *proč* to děláš tak a ne jinak:

1. **Návratový typ.** Dnes `GpxReader.Load` vrací `Gpx`. Ale `Gpx` je název *formátu* —
   jako obecný výstup parseru zní divně (`TcxParser` vrací `Gpx`?!). Přejmenovat kořen
   modelu na formátově neutrální `Activity`? To je čistší, ale sáhne to do víc míst.
   Tvoje volba, tvoje doména — jak bys pojmenoval „jednu načtenou aktivitu"?
2. **`Parse` vs `Load`, a co dostane na vstup.** Cesta ke souboru (`string path`), nebo
   `Stream`? `Stream` je testovatelnější (nemusíš na disk) a univerzálnější (síť, ZIP),
   ale trochu ukecanější u volajícího. Kompromis: rozhraní bere `Stream`, statická
   pohodlná metoda `FromFile(path)` ho otevře.
3. **Jak konzument získá parser?** Předá se mu hotový `IActivityParser` (dependency
   injection v malém), nebo si ho vytáhne z factory sám? DI je čistší a testovatelnější.
4. **Zpětná kompatibilita.** Necháš `GpxReader` jako tenkou statickou fasádu nad novým
   `GpxParser` (ať se nerozbije `Console`), nebo přepíšeš i volající?

## 2.7 Idiomatické C# k tomuhle

- **Pojmenování:** rozhraní s prefixem `I` (`IActivityParser`) — konvence .NET.
- **Implementace může zůstat `sealed`** (`public sealed class GpxParser : IActivityParser`),
  pokud nečekáš dědění — jasnější záměr a drobná optimalizace.
- **Statická vs instanční:** rozhraní vyžaduje instanční metody. `GpxReader` byl statický;
  `GpxParser` bude instanční třída (bezstavová, klidně jich může být víc instancí).
- **Primary constructor** (`public sealed class Foo(IActivityParser parser)`) je moderní,
  stručný způsob, jak přijmout závislost přes DI.

## 2.8 Jak to aplikovat na Spoorly

**Co** vzniká; **jak** (tělo) píšeš ty, já reviduju a ptám se na *proč*:

1. Rozhodni otevřené otázky z [2.6](#26-otevřené-designové-otázky-rozhodneš-ty) — hlavně
   návratový typ a vstup. To ovlivní všechno další.
2. Definuj `IActivityParser` v `Spoorly.Core/Io/` (nebo `Parsing/`).
3. Přepiš `GpxReader` → `GpxParser : IActivityParser`. Logika parsingu se nemění, jen
   se ze statické stane instanční a schová se za kontrakt.
4. Zajisti výběr parseru (factory `ForFile`, zatím `switch` na příponě).
5. Uprav `Spoorly.Console`, ať jede přes rozhraní — ověříš, že abstrakce sedí v praxi.
6. Test: fake `IActivityParser` do konzumenta (double bez disku) + že `GpxParser` pořád
   parsuje `data/test1.gpx` stejně jako dřív (regrese).

## Další čtení (nepovinné prohloubení)

- Microsoft Learn — `learn.microsoft.com/dotnet/csharp/fundamentals/types/interfaces`.
- Refactoring Guru — Strategy pattern (jazykově neutrální, s C# příklady): `refactoring.guru/design-patterns/strategy`.
- Kniha: Robert C. Martin — *Agile Principles, Patterns, and Practices in C#* (dependency inversion, ISP).
- Pozor na over-engineering: Martin Fowler, „Yagni" — `martinfowler.com/bliki/Yagni.html`.

---

# 3. Perzistence: EF Core + PostgreSQL / PostGIS

> Tahle kapitola je delší schválně — je to první krok mimo čistou doménu do
> infrastruktury a zároveň nejcennější kus pro reálné uplatnění v GIS. Ber to jako
> mapu, ne jako něco k přečtení na jeden zátah.

## 3.1 Kam kapitola míří

Dosud žije všechno v paměti: načteš `.gpx`, spočítáš metriky, vypíšeš, konec. Ve Fázi 3
přidáme **trvalé uložení** — aktivity půjdou uložit do databáze a číst zpět. Nové vrstvy:

- **PostgreSQL** — relační databáze,
- **PostGIS** — její geoprostorové rozšíření (typy `geometry`/`geography`, prostorové dotazy, indexy),
- **EF Core** — ORM, který mapuje C# objekty na tabulky a překládá LINQ na SQL,
- **Docker** — poprvé, zatím jen pro rozjetí databáze.

Proč zrovna tohle a proč to stojí za důkladnost: kombinace **.NET + PostGIS** je v praxi
běžná (backend GIS aplikací, tracking, logistika) a znalost prostorové databáze je přesně
ten průnik tvojí geodetické minulosti a nové .NET role. Tady se ty dva světy potkávají.

## 3.2 Proč databáze, a proč relační / Postgres

Soubor `.gpx` je fajn pro *výměnu* dat, ale mizerný pro *dotazování* („dej mi všechny běhy
delší než 10 km z června"). Databáze přidává: dotazy, indexy, transakce (ACID), souběžný
přístup, integritu (cizí klíče). Relační model sedí na tvá data, protože mají jasnou
strukturu a vztahy (aktivita → trasy → segmenty → body).

**Proč Postgres** (a ne SQLite/SQL Server): je open-source, zdarma, robustní a hlavně má
**PostGIS** — nejlepší open-source geoprostorové rozšíření, jaké existuje. Pro GIS je to
jasná volba a je to i to, co briefy projektu (Docker → Hetzner) předpokládají.

## 3.3 PostGIS: geoprostorová vrstva

PostGIS přidává Postgresu geoprostorové **datové typy**, **funkce** a **indexy**. Místo
abys ukládal `lat`/`lon` jako dva `double` sloupce, uložíš celý bod nebo trasu jako
jednu **geometrii** a databáze umí počítat vzdálenosti, průniky, obálky, „co je do 5 km".

Čtyři pojmy, které musíš mít v malíku (a u každého je chyták):

1. **`geometry` vs `geography`** — dva typy, zásadní rozdíl:
   - `geometry` = **rovinná** matematika (kartézská). Rychlá, ale vzdálenost ve stupních,
     pokud jsou data v zeměpisných souřadnicích → pro délku trasy potřebuješ projekci.
   - `geography` = počítá **na elipsoidu** (WGS84). Vzdálenosti rovnou v **metrech**,
     korektní na velké vzdálenosti, ale pomalejší a podporuje míň funkcí.
   - Pro GPS tracky je `geography` lákavé (metry zdarma), ale běžnější je `geometry`
     se SRID 4326 + funkce jako `ST_DistanceSphere`, nebo projekce do metrického CRS.
     **Tohle je tvoje doména** — rozhodneš líp než já (viz [3.9](#39-kdo-počítá-vzdálenost-doména-vs-postgis)).
2. **SRID** — identifikátor souřadnicového systému. **4326** = WGS84 (lat/lon z GPS).
   Každá geometrie ho nese; míchat SRID = chyba nebo nesmysl ve výsledku.
3. **Pořadí souřadnic — klasický chyták:** PostGIS i NetTopologySuite pracují v pořadí
   **(X = longitude, Y = latitude)**. Tvůj `TrackPoint` má `Latitude` i `Longitude`, takže
   při mapování `Point(x: lon, y: lat)` — prohodit je znamená mít trasu v Antarktidě.
4. **Prostorový index (GiST)** — bez něj je „najdi vše do 5 km" plný scan. GiST index nad
   geometrickým sloupcem to zrychlí o řády; PostGIS ho běžně používá.

## 3.4 EF Core v kostce

**ORM** (Object-Relational Mapper) překládá mezi C# objekty a relačními tabulkami, abys
nepsal SQL ručně. Klíčové pojmy:

- **`DbContext`** — tvoje „relace k databázi" + brána k dotazům. Jedna třída (např.
  `SpoorlyDbContext`), krátce žijící (na request/operaci, ne globálně).
- **`DbSet<T>`** — kolekce entit mapovaná na tabulku: `DbSet<Activity> Activities`.
- **Entita** — třída mapovaná na řádek. Potřebuje **klíč** (typicky `Id`).
- **`OnModelCreating`** — fluent konfigurace mapování (vztahy, sloupce, indexy), když
  konvence nestačí.
- **LINQ → SQL** — `context.Activities.Where(a => a.Distance > 10000)` EF přeloží na SQL
  a pošle do DB. Počítá **databáze**, ne appka (tzv. *pushdown*).
- **Migrace** — verzované změny schématu (viz [3.10](#310-migrace-workflow)).

## 3.5 Jak EF Core mluví s PostGIS

Tři balíčky (zdůvodnění, proč zrovna ony — jinak preferujeme std. knihovnu):

- **`Npgsql.EntityFrameworkCore.PostgreSQL`** — EF Core provider pro Postgres (překladač
  LINQ→SQL mluvící „postgresově"). Bez něj EF Postgres neumí.
- **`Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`** — přimíchá podporu
  prostorových typů; zapneš přes `o => o.UseNetTopologySuite()`.
- **`NetTopologySuite`** (NTS) — .NET port knihovny JTS; dává ti C# typy `Point`,
  `LineString`, `Geometry`. **Tohle jsou typy, které v entitě použiješ** místo dvojice `double`.

Napojení (jen náčrt, detaily jsou tvoje úloha):

```csharp
options.UseNpgsql(connectionString, o => o.UseNetTopologySuite());
```

## 3.6 Klíčové rozhodnutí: jak uložit trasu

Tohle je **srdce kapitoly** a ryze GIS rozhodnutí — ne detail. Máš stovky až statisíce
bodů na aktivitu (velký soubor měl 34 288). Jak je uložit?

- **A) Normalizovaně (relačně).** Tabulky `Activity`, `Track`, `TrackSegment` a `TrackPoint`
  jako *řádky* (`lat`, `lon`, `ele`, `time`). Milion řádků bodů. Klasické, čitelné, ale
  těžké na objem a geoprostorově „hloupé" (databáze nevidí trasu jako čáru).
- **B) Geometry-centric (GIS způsob).** Každý segment/trasu ulož jako **jednu geometrii**
  `LINESTRING` (viz [3.7](#37-linestring-zm)). Jeden řádek = jedna čára. PostGIS pak umí
  `ST_Length`, prostorové dotazy, GiST index. Kompaktní a mocné.
- **C) Hybrid.** Metadata + souhrnné metriky relačně, samotná geometrie trasy jako
  `LINESTRING`. V praxi nejčastější.

**Doporučení k promyšlení:** pro GIS appku je **B/C správný směr** — jde o to naučit se
myslet „geometrie", ne „tabulka čísel". Ale je to tvoje volba a tvoje doména; ptám se na
tvou preferenci, nerozhoduju za tebe (přesnost/dotazovatelnost vs. jednoduchost mapování).

## 3.7 `LINESTRING ZM` — geometrie, co nese výšku i čas

`LINESTRING` je lomená čára = uspořádaná sekvence bodů. PostGIS umí ke každému vrcholu
přidat dvě extra dimenze:

- **Z** — třetí souřadnice, u tebe přirozeně **nadmořská výška** (`Elevation`).
- **M** — „measure", libovolná hodnota na vrcholu; u tracků se hodí **čas** nebo naběhaná
  vzdálenost.

Takže celý GPX segment se dá vyjádřit jako jeden `LINESTRING ZM`, kde každý vrchol drží
`(lon, lat, ele, time)`. To je elegantní a formátově čisté — ale rozmysli, jak M
(čas) reprezentovat (např. Unix epoch), a co s body **bez** výšky/času (Z/M pak nejsou
konzistentní přes celou čáru → možná fallback nebo NaN handling).

## 3.8 Records jako entity — kde to skřípe

Tvůj model je immutable `record`y (`init`-only). EF Core s tím **umí pracovat**, ale jsou tři tření:

1. **Identita vs hodnota.** EF sleduje entity podle **klíče** (identita řádku), ale
   `record` má **hodnotovou rovnost** (dva různé řádky se stejnými hodnotami jsou si `==`).
   To může plést změnové sledování. Entity proto často bývají spíš `class` s `Id`,
   zatímco `record` se hodí na hodnotové objekty bez identity.
2. **Klíč.** Doména `TrackPoint` žádné `Id` nemá — pro DB ho typicky potřebuje. Buď ho
   přidáš, nebo body neukládáš jako entity, ale jako součást geometrie (viz [3.6](#36-klíčové-rozhodnutí-jak-uložit-trasu) B).
3. **Materializace.** EF entitu vytváří a plní; `init`-only a `required` s tím dnes jdou
   dohromady, ale bezparametrický konstruktor / navigační vlastnosti občas potřebují úlevu.

**Designová otázka:** oddělit **doménové** `record`y (čisté, jak je máš) od **DB entit**
(zvlášť, s `Id`, mapované) a mezi nimi mapovat? Čistší, ale víc kódu. Nebo mapovat doménu
přímo? Rychlejší, ale DB detaily prosáknou do domény. Klasický tradeoff — probereme.

## 3.9 Kdo počítá vzdálenost: doména vs PostGIS

Zajímavý střet s Fází 1: **vzdálenost teď počítá tvůj `Distance`** (C#). PostGIS to ale
umí taky (`ST_Length` nad `geography`). Kdo má být zdrojem pravdy?

- **Doména (C#)** — přenositelné, testovatelné bez DB, plná kontrola nad vzorcem
  (Equirectangular/Slope). Ale počítá appka a nad DB daty musíš data nejdřív vytáhnout.
- **PostGIS pushdown** — počítá databáze, blízko dat, rychlé přes miliony bodů, ale vzorec
  je „na PostGIS" a vyžaduje DB k testu.

Neexistuje jedna správná odpověď — často se **souhrn uloží** (spočítáš při importu doménou)
a **prostorové dotazy** (co je poblíž) se dělají v PostGIS. Tvoje doména, tvoje volba;
zajímá mě tvůj názor geodeta na přesnost `ST_Length(geography)` vs tvůj `Slope`.

## 3.10 Migrace: workflow

Migrace = **verzované změny schématu** jako kód (v gitu), ne ručně klikané `CREATE TABLE`.
Potřebuješ balíček `Microsoft.EntityFrameworkCore.Design` a nástroj `dotnet-ef`. Cyklus:

```bash
dotnet tool install --global dotnet-ef        # jednou
dotnet ef migrations add InitialCreate         # vygeneruje migraci z modelu
dotnet ef database update                      # aplikuje ji na DB
```

Každá změna modelu = nová migrace. Migrace jsou **verzované a přenositelné** — kdokoli
z čistého repa dostane `dotnet ef database update` stejné schéma. Nikdy needituj už
aplikovanou migraci; přidej novou.

## 3.11 Postgres + PostGIS v Dockeru

Nebudeš instalovat Postgres do systému — spustíš ho v kontejneru. Použije se image
**`postgis/postgis`** (Postgres s už zapnutým PostGIS), typicky přes `docker-compose.yml`
(služba, port 5432, volume na data, heslo z proměnné prostředí). Connection string pak
míří na `localhost:5432`.

**Bezpečnost:** heslo do DB **nepatří do gitu** — proměnná prostředí nebo user-secrets
(`dotnet user-secrets`), ne `appsettings.json` v repu. Tohle si pohlídej od začátku.

## 3.12 `DbContext`: životní cyklus, async, tracking

- **Životnost:** `DbContext` je **krátkodobý a není thread-safe**. Jedna instance na
  operaci/request, pak zahodit (`using` / DI scope). Nedrž ho globálně.
- **Async:** DB je I/O → používej `SaveChangesAsync`, `ToListAsync`, `FirstOrDefaultAsync`.
  Nevlákníš tím vlákno na čekání na disk/síť.
- **Tracking:** EF defaultně **sleduje** načtené entity (aby uměl uložit změny). Když jen
  čteš pro zobrazení, `AsNoTracking()` je rychlejší a šetří paměť.

## 3.13 Repository / Unit of Work — potřebuješ to teď?

Uslyšíš, že „nad EF Core patří repository pattern". **Ve tvé fázi zpravidla ne** — `DbContext`
*už je* Unit of Work a `DbSet` *už je* repository. Přidat další vrstvu „pro čistotu" je
často ta předčasná abstrakce z [2.1](#21-kam-tahle-kapitola-míří). Přijde, až budeš mít
konkrétní důvod (skrýt EF za doménovou hranici, víc úložišť). Zatím volej `DbContext` přímo.

## 3.14 Designové otázky (rozhodneš ty)

Jádro fáze — rozmysli **před** kódem:

1. **Model uložení** ([3.6](#36-klíčové-rozhodnutí-jak-uložit-trasu)): normalizovaně vs
   `LINESTRING` vs hybrid? Zásadní, ovlivní všechno.
2. **`geometry` vs `geography`** ([3.3](#33-postgis-geoprostorová-vrstva)) a co s tvým
   `Distance` ([3.9](#39-kdo-počítá-vzdálenost-doména-vs-postgis)).
3. **Doménové `record`y vs oddělené DB entity** ([3.8](#38-records-jako-entity--kde-to-skřípe)).
4. **Kam s `SpoorlyDbContext`** — nový projekt `Spoorly.Data`/`Spoorly.Infrastructure`, ať
   `Core` zůstane bez závislosti na EF? (Doporučuju ano — chrání čistotu domény.)
5. **Z/M v geometrii** ([3.7](#37-linestring-zm)) — reprezentace času, body bez výšky.

## 3.15 Jak to aplikovat na Spoorly

**Co** vzniká; **jak** píšeš ty, já reviduju a ptám se na *proč*. Postupně:

1. Rozhodni otázky z [3.14](#314-designové-otázky-rozhodneš-ty) — hlavně model uložení a
   kam s `DbContext`.
2. Rozjeď **PostGIS v Dockeru** (`docker-compose.yml`, image `postgis/postgis`) a ověř
   připojení (např. `psql` nebo `SELECT postgis_version();`).
3. Přidej balíčky (Npgsql EF provider + NetTopologySuite) do nového datového projektu.
4. Napiš `SpoorlyDbContext` + entitu/y podle zvoleného modelu; `UseNetTopologySuite()`.
5. První **migrace** (`InitialCreate`) → `database update`; zkontroluj schéma v DB.
6. **Import**: z načtené `Activity` (z GPX) postav geometrii/entity a ulož (`SaveChangesAsync`).
7. **Čtení**: vytáhni aktivitu zpět a ověř, že sedí s tím, co jsi uložil (kolečko round-trip).
8. Test: mapování a round-trip. (Integrační test proti DB — nová kategorie; probereme, jak
   na to bez zpomalení celé sady, např. Testcontainers.)

## Další čtení (nepovinné prohloubení)

- Npgsql EF Core provider — `www.npgsql.org/efcore/` (hlavně sekce *Spatial/NetTopologySuite*).
- PostGIS dokumentace — `postgis.net/documentation/` a „Introduction to PostGIS" workshop.
- EF Core — `learn.microsoft.com/ef/core/` (Migrations, DbContext lifetime, Spatial data).
- NetTopologySuite — `nettopologysuite.github.io/NetTopologySuite/`.
- `geometry` vs `geography` — PostGIS manuál, kapitola „When to use Geography…".
- Testcontainers pro .NET — `dotnet.testcontainers.org` (integrační testy proti reálné DB).
