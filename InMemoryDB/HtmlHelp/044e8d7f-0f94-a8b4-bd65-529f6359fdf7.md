# InMemoryDB Namespace


\[Missing &lt;summary&gt; documentation for "N:InMemoryDB"\]



## Classes
<table>
<tr>
<td><a href="98994abe-26d5-edd7-b45e-66432979d475">BooleanColumn</a></td>
<td> </td></tr>
<tr>
<td><a href="a3853ea2-4fee-619e-3239-92fbf306e5a8">Column(T)</a></td>
<td> </td></tr>
<tr>
<td><a href="072256a6-4e86-2a0a-723b-934e64bcdb43">Db</a></td>
<td>Hlavní třída databáze.</td></tr>
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