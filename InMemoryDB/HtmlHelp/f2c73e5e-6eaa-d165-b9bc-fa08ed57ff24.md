# TransformTo&lt;V&gt; Method


\[Missing &lt;summary&gt; documentation for "M:InMemoryDB.Column`1.TransformTo``1(System.Func{`0,``0})"\]



## Definition
**Namespace:** <a href="044e8d7f-0f94-a8b4-bd65-529f6359fdf7">InMemoryDB</a>  
**Assembly:** InMemoryDB (in InMemoryDB.exe) Version: 1.0.0

**C#**
``` C#
public Column<V> TransformTo<V>(
	Func<T, V> transform
)
where V : Object, IComparable<V>, IEquatable<V>

```
**VB**
``` VB
Public Function TransformTo(Of V As {Object, IComparable(Of V), IEquatable(Of V)}) ( 
	transform As Func(Of T, V)
) As Column(Of V)
```
**C++**
``` C++
public:
generic<typename V>
where V : Object, IComparable<V>, IEquatable<V>
Column<V>^ TransformTo(
	Func<T, V>^ transform
)
```
**F#**
``` F#
member TransformTo : 
        transform : Func<'T, 'V> -> Column<'V>  when 'V : Object and IComparable<'V> and IEquatable<'V>
```



#### Parameters
<dl><dt>  Func(<a href="a3853ea2-4fee-619e-3239-92fbf306e5a8">T</a>, V)</dt><dd>\[Missing &lt;param name="transform"/&gt; documentation for "M:InMemoryDB.Column`1.TransformTo``1(System.Func{`0,``0})"\]</dd></dl>

#### Type Parameters
<dl><dt /><dd>\[Missing &lt;typeparam name="V"/&gt; documentation for "M:InMemoryDB.Column`1.TransformTo``1(System.Func{`0,``0})"\]</dd></dl>

#### Return Value
<a href="a3853ea2-4fee-619e-3239-92fbf306e5a8">Column</a>(V)  
\[Missing &lt;returns&gt; documentation for "M:InMemoryDB.Column`1.TransformTo``1(System.Func{`0,``0})"\]

## See Also


#### Reference
<a href="a3853ea2-4fee-619e-3239-92fbf306e5a8">Column(T) Class</a>  
<a href="044e8d7f-0f94-a8b4-bd65-529f6359fdf7">InMemoryDB Namespace</a>  
