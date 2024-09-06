# InMemoryDB Namespace


Jednoduchá in-memory databáze napsaná v C# jako zápočtový projekt do Programování II. Databáze má polymorfní strukturu a může obsahovat libovolný počet sloupců(omezení pamětí) různého druhu.Databáze nativně podporuje základní datové typy C#, avšak je možné ji snadno rozšířit tak, aby pracovala s jakýmkoli typem implementujícím IComparable interface. Databáze podporuje indexování a binární vyhledávání v logaritmickém čase. # Inicializace Databáze se inicializuje pomocí Db db = new Db(); Dojde k inicializaci prázdné tabulky neobsahující žádné sloupce ani data. # Struktura Strukturu databáze lze libovolně měnit, dokud je prázdná, poté tento pokus vyústí v Exception. Sloupce lze přidávat pomocí metody `AddColumn T (string name)`, jež přiřadí sloupci jak název tak datový typ, ten je pro daný sloupec již neměnný, avšak sloupec lze později odstranit pomocí metod RemoveColumn a RemoveColumnAt. Pro vytvoření indexu slouží metoda `MakeIndex T (string column_name)`, která přijímá název sloupce. Automaticky se tak namapuje binární vyhledávací strom (třída BST) na daný index a při každém přidání prvku se hodnota v daném sloupci zařadí do stromu spolu s odkazem na konkrétní řádek. # Runtime Pro přidání slouží metoda Insert, celkový počet řádků vyjadřuje vlastnost Count. Pro vyhledávání existují dvě základní metody SelectOneWhere a SelectAllWhere, které vrací výsledky, jež se na daném sloupci shodují s hledanou hodnotou.SelectOneWhere vrací tzv.RecordWrapper, který zabaluje Record tak, aby se k jeho polím dalo dynamicky přistupovat jako k vlastnostem (avšak neobsahuje jakoukoli komplilační kontrolu existence sloupců). SelectAllWhere vrací všechny výsledky, které se shodují na daném sloupci s hledanou hodnotou ve formě pod-tabulky: opět třídy Db, která však obsahuje jen hledané řádky.Při tomto výběru se automaticky vybudují všechny indexy pro pod-tabulku znovu a umožňují stejné binární vyhledávání jako na mateřské tabulce. ## Binární vyhledání O binární vyhledávání se starají binární vyhledávací stromy, jež se budují během přidávání prvků. Třída BST představuje jednoduchý nevyvážený strom, avšak lze ji nahradit jakoukoli třídou, jež implementuje interface ITree a je potomkem třídy BST.Tedy ji lze jednoduše vyměnit např.za AVL strom a zajistit si tak lepší složitost v nejhorším případě (v původní implementaci až lineární složitost). ## Více-sloupcové vyhledávání Více sloupcového vyhledávání lze dosáhnout pomocí SelectAllWhere, jež vrací tabulku, na které se opět dá spustit vyhledávání.Obdobně se dá postupovat při přidání dalších vyhledávácích metod. ## Rozšiřitelnost Databázi lze snadno rozšířit o další datové typy, stačí pouze rozšířit třídu FieldConvertor o funkci GetField: public static ParentField GetField(bool val) { return new Fiel bool (val); } Rozšíření o další možnosti Selektování lze jednoduše, pokud stačí lineární čas, avšak pro práci s binárním vyhledáváním je třeba vzít do úvahy slovník _indexes a pracovat s vyhledávacími stromy, to je asi nejvíce problematická část, co se rozšíření kódu týče, celková implementace stromů je pro další vývojáře možná zbytečně komplikovaná a specifická. Slovník _indexes obsahuje jako klíč pozici sloupce v tabulce a jako hodnotu příslušící vyhledávací strom uložený v polymorfním kontejneru obsahujícím mateřský typ BTS. Pro implementaci dalších operací by také bylo třeba upravit BTS, avšak lze k tomu použít standardní metody práce s BVS. Při mazání je nutno synchronizovat mazání v tabulce a indexovacích stromech, elegantním řešením by byl další "neviditelný" sloupec, jež by označoval, zda-li byl záznam smazán, a tabulka by se čas od času pročistila od těchto záznamů. Výhodou tohoto řešení je, že by se nemusely všechny prvky posouvat tak často. Změna vyhledávacího stromu je však jednoduchá, stačí změna v souboru Db.cs, nový strom musí být potomkem třídy BTS z BTS.cs a musí implementovat základní rozhraní ITree. # Příklad Příkladový zdrojový kód naleznete v souboru Program.cs



## Classes
<table>
<tr>
<td><a href="98994abe-26d5-edd7-b45e-66432979d475">BooleanColumn</a></td>
<td> </td></tr>
<tr>
<td><a href="a3853ea2-4fee-619e-3239-92fbf306e5a8">Column(T)</a></td>
<td>Generic class for column (internally a list of T values with added functionality for transformations and stuff).</td></tr>
<tr>
<td><a href="072256a6-4e86-2a0a-723b-934e64bcdb43">Db</a></td>
<td> </td></tr>
<tr>
<td><a href="15d1f56f-3dc8-30e2-1769-44c8b9a97dea">Db.RecordWrapper</a></td>
<td>Tato třída uzavírá Record a přidává mu dynamické rozhraní pro přístup k hodnotám dle jmen sloupců v databází.</td></tr>
<tr>
<td><a href="46a67b2d-bfd0-833f-4eb7-7ea9c7c08d2c">Field(T)</a></td>
<td>Třída pole pro konkrétní hodnotu.</td></tr>
<tr>
<td><a href="5461e5eb-5405-4cba-b818-6e7fd22b84dd">ParentField</a></td>
<td>Mateřská třída pole umožňující polymorfismus.</td></tr>
<tr>
<td><a href="dd104f96-249b-6ed8-8b7f-52cffe66f83b">Program</a></td>
<td>Program pro demonstraci databáze.</td></tr>
</table>

## Interfaces
<table>
<tr>
<td><a href="b44a0f71-593a-e4aa-9359-31fd8f274602">IColumn(T)</a></td>
<td>Column represents an entire column from table - list of values of the same type accross all rows in the database.</td></tr>
<tr>
<td><a href="d216a1ac-6f71-a87f-e312-ebec07c90547">ITree(T)</a></td>
<td>Interface pro konkrétní vyhledávací strom.</td></tr>
</table>