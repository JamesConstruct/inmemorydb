# InMemoryDB Namespace


\[Missing &lt;summary&gt; documentation for "N:InMemoryDB"\]



## Classes
<table>
<tr>
<td><a href="InMemoryDB/Help/072256a6-4e86-2a0a-723b-934e64bcdb43">Db</a></td>
<td>Hlavní třída databáze.</td></tr>
<tr>
<td><a href="InMemoryDB/Help/4fbc5763-f72d-71a7-e56d-5031feba9090">Db.FieldConvertor</a></td>
<td>Tato třída převádí typ pole konrkétní hodnoty na obecného předka ParentField. Zde je třeba přidat funkce pro další datové typy v případě rozšiřování.</td></tr>
<tr>
<td><a href="InMemoryDB/Help/15d1f56f-3dc8-30e2-1769-44c8b9a97dea">Db.RecordWrapper</a></td>
<td>Tato třída uzavírá Record a přidává mu dynamické rozhraní pro přístup k hodnotám dle jmen sloupců v databází.</td></tr>
<tr>
<td><a href="InMemoryDB/Help/46a67b2d-bfd0-833f-4eb7-7ea9c7c08d2c">Field(T)</a></td>
<td>Třída pole pro konkrétní hodnotu.</td></tr>
<tr>
<td><a href="InMemoryDB/Help/5461e5eb-5405-4cba-b818-6e7fd22b84dd">ParentField</a></td>
<td>Mateřská třída pole umožňující polymorfismus.</td></tr>
<tr>
<td><a href="InMemoryDB/Help/dd104f96-249b-6ed8-8b7f-52cffe66f83b">Program</a></td>
<td>Program pro demonstraci databáze.</td></tr>
</table>

## Interfaces
<table>
<tr>
<td><a href="InMemoryDB/Help/d216a1ac-6f71-a87f-e312-ebec07c90547">ITree(T)</a></td>
<td>Interface pro konkrétní vyhledávací strom.</td></tr>
</table>