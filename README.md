# Zápočtový program, programování II.

Jednoduchá **in-memory** databáze napsaná v C# jako zápočtový projekt do Programování v C#.
Databáze má polymorfní strukturu a může obsahovat libovolný počet sloupců (omezení pamětí) různého druhu. Databáze nativně podporuje základní datové typy C#, avšak
je možné ji snadno rozšířit tak, aby pracovala s jakýmkoli typem implementujícím IComparable interface. Databáze podporuje indexování a binární vyhledávání
v logaritmickém čase.

## Inicializace

Databáze se inicializuje pomocí Db db = new Db();
Dojde k inicializaci prázdné tabulky neobsahující žádné sloupce ani data.


## Struktura

Strukturu databáze lze libovolně měnit, dokud je prázdná, poté tento pokus vyústí v Exception. Sloupce lze přidávat pomocí metody `AddColumn<T>(string name)`,
jež přiřadí sloupci jak název tak datový typ, ten je pro daný sloupec již neměnný, avšak sloupec lze později odstranit pomocí metod RemoveColumn a RemoveColumnAt.
Pro vytvoření indexu slouží metoda `MakeIndex<T>(string column_name)`, která přijímá název sloupce. Automaticky se tak namapuje binární vyhledávací strom (třída BST)
na daný index a při každém přidání prvku se hodnota v daném sloupci zařadí do stromu spolu s odkazem na konkrétní řádek.


## Runtime

Pro přidání slouží metoda Insert, celkový počet řádků vyjadřuje vlastnost Count. Pro vyhledávání existují dvě základní metody SelectOneWhere a SelectAllWhere,
které vrací výsledky, jež se na daném sloupci shodují s hledanou hodnotou. SelectOneWhere vrací tzv. RecordWrapper, který zabaluje Record tak, aby se k jeho polím
dalo dynamicky přistupovat jako k vlastnostem (avšak neobsahuje jakoukoli komplilační kontrolu existence sloupců). SelectAllWhere vrací všechny výsledky, které se
shodují na daném sloupci s hledanou hodnotou ve formě pod-tabulky: opět třídy Db, která však obsahuje jen hledané řádky. Při tomto výběru se automaticky vybudují
všechny indexy pro pod-tabulku znovu a umožňují stejné binární vyhledávání jako na mateřské tabulce. 


## Binární vyhledání

O binární vyhledávání se starají binární vyhledávací stromy, jež se budují během přidávání prvků. Třída BST představuje jednoduchý nevyvážený strom, avšak lze ji
nahradit jakoukoli třídou, jež implementuje interface ITree a je potomkem třídy BST. Tedy ji lze jednoduše vyměnit např. za AVL strom a zajistit si tak lepší
složitost v nejhorším případě (v původní implementaci až lineární složitost).


## Více-sloupcové vyhledávání

Více sloupcového vyhledávání lze dosáhnout pomocí SelectAllWhere, jež vrací tabulku, na které se opět dá spustit vyhledávání. Obdobně se dá postupovat při
přidání dalších vyhledávácích metod.

### Přístup k položkám

Přistupovat k položkám se dá pomocí indexeru, metod `First()` a `Last()`. Z tabulky lze získat jeden záznam pomocí metody `SelectOneWhere<T>(string col, T val)` nebo pod-tabulku pomocí `SelectAllWhere<T>(string col, T val)` (viz. dokumentace ohledně třídy Db). Získané záznamy jsou obalené wrapperem, který umožňuje dynamický přístup k položkám dle názvu sloupce (tedy například `db[0].firstname`).

### Filtry

Databáze podporuje dynamickou extrakci sloupců, jejich porovnávání / transformování a zpětné vyhledávání v databázi pomocí `BooleanColumn`. 
Příklad:

```C#
var db = new Db();
db.AddColumn<bool>("id");
db.AddColumn<string>("username");
db.AddColumn<bool>("active");
db.MakeIndex("id");

// add data

var activeUsers = db[((dynamic)db).active].username;    // usernames of active users
var activeJohns = db[activeUsers.username == "john"];   // table containing records only about users named "john" who are active
```

Filtry umožňují chaining pomocí logických operátorů a aplikaci různých porovnání či transformací (i do jiného typu dat).

```c#
Func<string, int> nameLength = x => x.Length;
var longnameUsers = db[((dynamic)db).username.TransformTo<int>(nameLength) > 5];    // data o uživatelých se jménem delším než 5 znaků
```


## Rozšiřitelnost

Databázi lze snadno rozšířit o další datové typy, jediné podmínky pro typ T jsou IComparable<T>, IEquatable<T> pokud je třeba binární vyhledávací strom nebo filtrování pomocí filtrů či hledání v databázi.


Rozšíření o další možnosti Selektování lze jednoduše, pokud stačí lineární čas, avšak pro práci s binárním vyhledáváním je třeba vzít do úvahy slovník _indexes a pracovat
s vyhledávacími stromy, to je asi nejvíce problematická část, co se rozšíření kódu týče, celková implementace stromů je pro další vývojáře možná zbytečně komplikovaná a
specifická. Slovník _indexes obsahuje jako klíč pozici sloupce v tabulce a jako hodnotu příslušící vyhledávací strom uložený v polymorfním kontejneru obsahujícím mateřský
typ BTS.

Pro implementaci dalších operací by také bylo třeba upravit BTS, avšak lze k tomu použít standardní metody práce s BVS. Při mazání je nutno synchronizovat mazání v tabulce a
indexovacích stromech, elegantním řešením by byl další "neviditelný" sloupec, jež by označoval, zda-li byl záznam smazán, a tabulka by se čas od času pročistila od těchto
záznamů. Výhodou tohoto řešení je, že by se nemusely všechny prvky posouvat tak často.

Změna vyhledávacího stromu je však jednoduchá, stačí změna v souboru Db.cs, nový strom musí být potomkem třídy BTS z BTS.cs a musí implementovat základní rozhraní ITree.

## Příklad

Příkladový zdrojový kód naleznete v souboru [Program.cs](https://github.com/JamesConstruct/inmemorydb/blob/main/InMemoryDB/Program.cs).

## Docs

[Dokumentace Zde](https://github.com/JamesConstruct/inmemorydb/blob/main/InMemoryDB/HtmlHelp/Home.md).

## Tests

Testy jsou napsané v testovacím frameworku xUnit a testují především API databáze a různé kombinované dotazy a datové manipulace.