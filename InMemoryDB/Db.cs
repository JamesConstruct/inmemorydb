using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Collections;
using System.Reflection;
using System.ComponentModel.Design;



namespace InMemoryDB
{

    using Record = List<ParentField>;   // Record je souhrn polí.


    /// <summary>
    /// 
    /// Jednoduchá **in-memory** databáze napsaná v C# jako zápočtový projekt do Programování v C#.
    /// Databáze má polymorfní strukturu a může obsahovat libovolný počet sloupců (omezení pamětí) různého druhu. Databáze nativně podporuje základní datové typy C#, avšak
    /// je možné ji snadno rozšířit tak, aby pracovala s jakýmkoli typem implementujícím IComparable interface. Databáze podporuje indexování a binární vyhledávání
    /// v logaritmickém čase.
    /// 
    /// ## Inicializace
    /// 
    /// Databáze se inicializuje pomocí Db db = new Db();
    /// Dojde k inicializaci prázdné tabulky neobsahující žádné sloupce ani data.
    /// 
    /// 
    /// ## Struktura
    /// 
    /// Strukturu databáze lze libovolně měnit, dokud je prázdná, poté tento pokus vyústí v Exception. Sloupce lze přidávat pomocí metody `AddColumn<T>(string name)`,
    /// jež přiřadí sloupci jak název tak datový typ, ten je pro daný sloupec již neměnný, avšak sloupec lze později odstranit pomocí metod RemoveColumn a RemoveColumnAt.
    /// Pro vytvoření indexu slouží metoda `MakeIndex<T>(string column_name)`, která přijímá název sloupce. Automaticky se tak namapuje binární vyhledávací strom (třída BST)
    /// na daný index a při každém přidání prvku se hodnota v daném sloupci zařadí do stromu spolu s odkazem na konkrétní řádek.
    /// 
    /// 
    /// ## Runtime
    /// 
    /// Pro přidání slouží metoda Insert, celkový počet řádků vyjadřuje vlastnost Count. Pro vyhledávání existují dvě základní metody SelectOneWhere a SelectAllWhere,
    /// které vrací výsledky, jež se na daném sloupci shodují s hledanou hodnotou. SelectOneWhere vrací tzv. RecordWrapper, který zabaluje Record tak, aby se k jeho polím
    /// dalo dynamicky přistupovat jako k vlastnostem (avšak neobsahuje jakoukoli komplilační kontrolu existence sloupců). SelectAllWhere vrací všechny výsledky, které se
    /// shodují na daném sloupci s hledanou hodnotou ve formě pod-tabulky: opět třídy Db, která však obsahuje jen hledané řádky. Při tomto výběru se automaticky vybudují
    /// všechny indexy pro pod-tabulku znovu a umožňují stejné binární vyhledávání jako na mateřské tabulce. 
    /// 
    /// 
    /// ## Binární vyhledání
    /// 
    /// O binární vyhledávání se starají binární vyhledávací stromy, jež se budují během přidávání prvků. Třída BST představuje jednoduchý nevyvážený strom, avšak lze ji
    /// nahradit jakoukoli třídou, jež implementuje interface ITree a je potomkem třídy BST. Tedy ji lze jednoduše vyměnit např. za AVL strom a zajistit si tak lepší
    /// složitost v nejhorším případě (v původní implementaci až lineární složitost).
    /// 
    /// 
    /// ## Více-sloupcové vyhledávání
    /// 
    /// Více sloupcového vyhledávání lze dosáhnout pomocí SelectAllWhere, jež vrací tabulku, na které se opět dá spustit vyhledávání. Obdobně se dá postupovat při
    /// přidání dalších vyhledávácích metod.
    /// 
    /// ### Přístup k položkám
    /// 
    /// Přistupovat k položkám se dá pomocí indexeru, metod `First()` a `Last()`. Z tabulky lze získat jeden záznam pomocí metody `SelectOneWhere<T>(string col, T val)` nebo pod-tabulku pomocí `SelectAllWhere<T>(string col, T val)` (viz. dokumentace ohledně třídy Db). Získané záznamy jsou obalené wrapperem, který umožňuje dynamický přístup k položkám dle názvu sloupce (tedy například `db[0].firstname`).
    /// 
    /// ### Filtry
    /// 
    /// Databáze podporuje dynamickou extrakci sloupců, jejich porovnávání / transformování a zpětné vyhledávání v databázi pomocí `BooleanColumn`. 
    /// Příklad:
    /// 
    /// ```C#
    /// var db = new Db();
    /// db.AddColumn<bool>("id");
    /// db.AddColumn<string>("username");
    /// db.AddColumn<bool>("active");
    /// db.MakeIndex("id");
    /// 
    /// // add data
    /// 
    /// var activeUsers = db[((dynamic)db).active].username;    // usernames of active users
    /// var activeJohns = db[activeUsers.username == "john"];   // table containing records only about users named "john" who are active
    /// ```
    /// 
    /// Filtry umožňují chaining pomocí logických operátorů a aplikaci různých porovnání či transformací (i do jiného typu dat).
    /// 
    /// ```c#
    /// Func<string, int> nameLength = x => x.Length;
    /// var longnameUsers = db[((dynamic)db).username.TransformTo<int>(nameLength) > 5];    // data o uživatelých se jménem delším než 5 znaků
    /// ```
    /// 
    /// 
    /// ## Rozšiřitelnost
    /// 
    /// Databázi lze snadno rozšířit o další datové typy, jediné podmínky pro typ T jsou IComparable<T>, IEquatable<T> pokud je třeba binární vyhledávací strom nebo filtrování pomocí filtrů či hledání v databázi.
    /// 
    /// 
    /// Rozšíření o další možnosti Selektování lze jednoduše, pokud stačí lineární čas, avšak pro práci s binárním vyhledáváním je třeba vzít do úvahy slovník _indexes a pracovat
    /// s vyhledávacími stromy, to je asi nejvíce problematická část, co se rozšíření kódu týče, celková implementace stromů je pro další vývojáře možná zbytečně komplikovaná a
    /// specifická. Slovník _indexes obsahuje jako klíč pozici sloupce v tabulce a jako hodnotu příslušící vyhledávací strom uložený v polymorfním kontejneru obsahujícím mateřský
    /// typ BTS.
    /// 
    /// Pro implementaci dalších operací by také bylo třeba upravit BTS, avšak lze k tomu použít standardní metody práce s BVS. Při mazání je nutno synchronizovat mazání v tabulce a
    /// indexovacích stromech, elegantním řešením by byl další "neviditelný" sloupec, jež by označoval, zda-li byl záznam smazán, a tabulka by se čas od času pročistila od těchto
    /// záznamů. Výhodou tohoto řešení je, že by se nemusely všechny prvky posouvat tak často.
    /// 
    /// Změna vyhledávacího stromu je však jednoduchá, stačí změna v souboru Db.cs, nový strom musí být potomkem třídy BTS z BTS.cs a musí implementovat základní rozhraní ITree.
    /// 
    /// ## Příklad
    /// 
    /// Příkladový zdrojový kód naleznete v souboru [Program.cs](https://github.com/JamesConstruct/inmemorydb/blob/main/InMemoryDB/Program.cs).
    /// 
    /// ## Docs
    /// 
    /// [Dokumentace Zde](https://jamesconstruct.github.io/inmemorydb).
    /// 
    /// ## Tests
    /// 
    /// Testy jsou napsané v testovacím frameworku xUnit a testují především API databáze a různé kombinované dotazy a datové manipulace.
    /// 
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }


    public class Db : DynamicObject, IEnumerable
    {

        /// <summary>
        /// Tato třída převádí typ pole konrkétní hodnoty na obecného předka ParentField. Zde je třeba přidat funkce pro další datové typy v případě rozšiřování.
        /// </summary>
        internal static class FieldConvertor  // public aby šly připisovat další Convertory
        {

            public static ParentField GetField<T>(T val) where T : IComparable<T>
            {

                return new Field<T>(val);

            }

        }


        /// <summary>
        /// Tato třída uzavírá Record a přidává mu dynamické rozhraní pro přístup k hodnotám dle jmen sloupců v databází.
        /// </summary>
        public sealed class RecordWrapper : DynamicObject
        {
            private Record _record; // záznam
            private readonly Db _db;    // databáze, ze které pochází (kvůli jménům sloupců)


            /// <summary>
            /// Vytvoří nový wrapper.
            /// </summary>
            /// <param name="record">Záznam z databáze.</param>
            /// <param name="db">Databáze, ze které záznam pochází.</param>
            public RecordWrapper(Record record, Db db)
            {
                _record = record;
                _db = db;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return _db._columns;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object? result)
            {

                if (_db._columns.Contains(binder.Name))
                {

                    int index = _db._columns.IndexOf(binder.Name);
                    result = ((dynamic)_record.ElementAt(index)).Value; // dynamický přístup k typu

                    return true;    // úspěch

                }
                else // sloupce jsme nenalezli
                {

                    result = null;

                    return false;   // neúspěch

                }

            }

            public override bool TrySetMember(SetMemberBinder binder, object? value)
            {
                /* úprava polí v db??
                if (_db._columns.Contains(binder.Name))
                {
                    _properties[binder.Name] = value;
                    return true;
                }
                else
                {
                    return false;
                } */

                return false;   // nastavovat hodnotu sloupců takto nejde
            }

            public override string ToString()
            {
                var strngfr = new Stringifier(_db._columns.Count);
                strngfr.PushHeader(_db._columns);
                strngfr.PushFields(_record);
                return strngfr.ToString();

            }
        }


        /// <summary>
        /// Vrátí první záznam v databázi.
        /// </summary>
        /// <returns>RecordWrapper obsahující první záznam v databázi.</returns>
        public RecordWrapper First()
        {
            return new RecordWrapper(_records.First(), this);
        }


        /// <summary>
        /// Vrátí poslední záznam v databázi.
        /// </summary>
        /// <returns>RecordWrapper obsahující poslední záznam v databázi.</returns>
        public RecordWrapper Last()
        {
            return new RecordWrapper(_records.Last(), this);
        }


        /// <summary>
        /// Vrátí záznam z databáze na definované pozici.
        /// </summary>
        /// <param name="i">Pozice záznamu.</param>
        /// <returns>Vrátí RecordWrapper obsahující daný záznam.</returns>
        /// <exception cref="Exception">Vrátí Exception, pokud je daná pozice neplatná.</exception>
        public RecordWrapper RecordAt(int i)
        {
            if (i < 0 || i >= _records.Count) throw new Exception("Invalid index!");

            return new RecordWrapper(_records.ElementAt(i), this);
        }

        public RecordWrapper this[int index]
        {
            get
            {
                return RecordAt(index);
            }
        } 


        /// <summary>
        /// Vyhledá záznam v databázi, kde se daný sloupec <b>rovná</b> udané hodnotě. V případě, že hledáme dle indexu vyhledává logaritmicky, jinak lineárně. Vrátí <b>první</b> nalezený
        /// výsledek, nemusí se jednat o první v pořadí, v jakém byly záznamy přidávány.
        /// </summary>
        /// <typeparam name="T">Typ hodnoty, dle které vyhledáváme.</typeparam>
        /// <param name="column">Název sloupce, dle kterého vyhledáváme.</param>
        /// <param name="val">Hodnota, kterou chceme nalézt.</param>
        /// <returns>Vrací RecordWrapper nesoucí daný záznam.</returns>
        /// <exception cref="ArgumentException">ArgumentException v případě špatného typu sloupce.</exception>
        /// <exception cref="Exception">Exception v případě, že záznam není nalezen.</exception>
        public RecordWrapper SelectOneWhere<T>(string column, T val) where T : IComparable<T>
        {

            // ověření, že sedí typ T na daný sloupec
            if (ColumnType(column) != typeof(T)) throw new ArgumentException("Invalid value type!");

            // index sloupce
            int index = ColumnIndex(column);


            // jedná-li se o indexovaný sloupce, můžeme využít logaritmického vyhledávání
            if (_indexes.ContainsKey(index))
            {

                // vyhledání pořadí v tabulce _records pomocí sestaveného vyhledávacího stromu
                int i = ((dynamic)_indexes[index]).Find(val);

                return new RecordWrapper(_records.ElementAt((int)i), this);

            }

            // musíme provést lineární vyhledávání - jedná se o neindexovaný sloupec
            else
            {

                foreach (Record rec in _records)
                {
                    if (EqualityComparer<T>.Default.Equals(((Field<T>)rec.ElementAt(index)).Value, val))
                    {

                        return new RecordWrapper(rec, this);

                    }
                }

            }


            // nic nebylo nalezeno
            throw new Exception("Not found!");


        }


        /// <summary>
        /// Vyhledá <b>všechny</b> záznamy v databázi, kde se daný sloupec <b>rovná</b> udané hodnotě. V případě, že hledáme dle indexu vyhledává logaritmicky, jinak lineárně. Vrátí všechny
        /// výsledky v nedefinovaném pořadí. Může vracet prázdnou tabulku.
        /// </summary>
        /// <typeparam name="T">Typ hodnoty, dle které vyhledáváme.</typeparam>
        /// <param name="column">Název sloupce, dle kterého vyhledáváme.</param>
        /// <param name="val">Hodnota, kterou chceme nalézt.</param>
        /// <returns>Vrací novou databázi se stejnou strukturou obsahující jen vyhledávané záznamy.</returns>
        /// <exception cref="ArgumentException">ArgumentException v případě špatného typu sloupce.</exception>
        public Db SelectAllWhere<T>(string column, T val) where T : IComparable<T>
        {

            // ověření, že sedí typ T na daný sloupec
            if (ColumnType(column) != typeof(T)) throw new ArgumentException("Invalid value type!");

            // index sloupce
            int index = ColumnIndex(column);

            Db sub_db = new Db();
            sub_db.CopyStructure(this);


            // jedná-li se o indexovaný sloupce, můžeme využít logaritmického vyhledávání
            if (_indexes.ContainsKey(index))
            {

                // vyhledání pořadí v tabulce _records pomocí sestaveného vyhledávacího stromu
                List<int> results = ((dynamic)_indexes[index]).FindAll(val);

                foreach (int i in results)
                    sub_db.Insert(_records.ElementAt((int)i));

            }

            // musíme provést lineární vyhledávání - jedná se o neindexovaný sloupec
            else
            {

                foreach (Record rec in _records)
                {
                    if (EqualityComparer<T>.Default.Equals(((Field<T>)rec.ElementAt(index)).Value, val))
                    {

                        sub_db.Insert(rec);

                    }
                }

            }


            return sub_db;


        }

        public Db this[BooleanColumn filter]
        {
            get
            {
                if (filter.Length != _records.Count)
                    throw new ArgumentException("The filter length doesn't match the number of rows in the database!");

                Db sub_db = new Db();
                sub_db.CopyStructure(this);
                for (int i = 0; i < filter.Length; i++)
                {
                    if (filter[i])
                    {
                        sub_db.Insert(_records[i]);
                    }
                }

                return sub_db;
            }
        }



        /// <summary>
        /// Vypočte kumulativní sumu všech polí v daném sloupci.
        /// </summary>
        /// <typeparam name="T">Typ hodnot, které sčítáme.</typeparam>
        /// <param name="column">Název sčítaného sloupce.</param>
        /// <returns>Vrací kumulativní sumu.</returns>
        /// <exception cref="ArgumentException">ArgumentException v případě nekorespondujícího typu sloupce a typu T.</exception>
        public T GetSum<T>(string column) where T : new()
        {

            // Ověříme typ sloupce
            if (ColumnType(column) != typeof(T)) throw new ArgumentException("Invalid value type!");

            int index = ColumnIndex(column);

            // Vytvoření prázdné hodnoty pro kumulaci
            T sum = new();

            foreach (Record rec in _records)
                sum += ((dynamic)rec.ElementAt(index)).Value;


            return sum;

        }




        /// <summary>
        /// Přidá do tabulky sloupec daného typu s daným názvem, jež musí být unikátní v rámci tabulky.
        /// </summary>
        /// <typeparam name="T">Typ hodnot sloupce.</typeparam>
        /// <param name="name">Název sloupce.</param>
        /// <exception cref="Exception">Vrací exception v případě, že tabulka není prázdná nebo název sloupce není unikátní.</exception>
        public void AddColumn<T>(string name)
        {
            // Přidávat můžeme pouze do prázdné databáze
            if (!_empty) throw new Exception("Database is not empty!");

            // Názvy sloupců musí být unikátní
            if (_columns.Contains(name)) throw new Exception("Column " + name + " already exists!");

            // Přidáme pole daného typu
            _fields.Add(typeof(T));

            // přidáme název sloupce
            _columns.Add(name);

        }



        /// <summary>
        /// Odebere sloupec v tabulce na dané pozici.
        /// </summary>
        /// <param name="index">Pozice sloupce, jež chceme odebrat.</param>
        /// <exception cref="Exception">Vrací Exception v případě, že tabulka není prázdná, nebo index není platný.</exception>
        public void RemoveColumnAt(int index)
        {
            // odebírat můžeme pouze v prázdné databázi
            if (!_empty) throw new Exception("Database is not empty!");

            // Neplatný index
            if (index > _columns.Count || index < 0) throw new Exception("Invalid index!");

            _fields.RemoveAt(index);
            _columns.RemoveAt(index);
        }

        
        /// <summary>
        /// Odebere sloupec z tabulky dle jména.
        /// </summary>
        /// <param name="name">Název sloupce, jež má být odebrán.</param>
        /// <exception cref="Exception">Vrací Exception v případě, že tabulka není prázdná, nebo neobsahuje sloupec s daným názvem.</exception>
        public void RemoveColumn(string name)
        {
            if (!_empty) throw new Exception("Database is not empty!");

            if (!_columns.Contains(name)) throw new Exception("Column " + name + " doesn't exist!");

            int index = _columns.IndexOf(name);

            RemoveColumnAt(index);
        }



        /// <summary>
        /// Vytvoří z daného sloupce sloupec pro indexaci, čímž umožní binární vyhledávání.
        /// </summary>
        /// <typeparam name="T">Typ sloupce, podle kterého chceme indexovat.</typeparam>
        /// <param name="column">Název sloupce, dle kterého chceme indexovat.</param>
        /// <exception cref="ArgumentException">Vrací ArgumentException v případě, že nesedí typ hodnoty sloupce a T.</exception>
        /// <exception cref="Exception">Vrací Exception, když databáze není prázdná.</exception>
        public void MakeIndex<T>(string column) where T : IComparable<T>
        {

            // Ověření typu sloupce a T
            if (ColumnType(column) != typeof(T)) throw new ArgumentException("Invalid value type!");

            // Je nutná prázdná tabulka
            if (!_empty)
                throw new Exception("Database is not empty!");

            // Vytvoření indexu - nového binárního stromu
            _indexes.Add(ColumnIndex(column), new BST<T>());

        }


        /// <summary>
        /// Vloží do databáze nový záznam obsahující zadané hodnoty. Typ hodnoty musí korespondovat s typem sloupce.
        /// </summary>
        /// <param name="values">Hodnoty jednoho záznamu.</param>
        /// <exception cref="Exception">Vrací Exception v případě, že je zadán špatný počet argumentů, nebo nesedí jejich typ.</exception>
        public void Insert(params object[] values)
        {

            // Pokud byla tabulka prázdná, již není
            if (_empty) _empty = false;


            // List polí, do kterého se budou vkládat pole hodnot
            List<ParentField> list = new List<ParentField>();

            // sedí počet?
            if (_fields.Count == values.Length)
                
                for (int i = 0; i < values.Length; i++)

                    // Sedí typ?
                    if (_fields.ElementAt(i) != values.ElementAt(i).GetType())

                        throw new Exception("Field type mismatch at index " + i);


                    else
                    {

                        dynamic tmp = (dynamic)values[i];

                        list.Add(FieldConvertor.GetField(tmp));

                        if (_indexes.ContainsKey(i))    // jedná se o indexovaný sloupec, musíme hodnotu vložit do BVS
                            
                            ((dynamic) _indexes[i]).Insert(tmp, (int)Count);

                    }


            else

                throw new Exception("Invalid number of fields!");


            // Vložíme record do listu záznamů
            _records.Add(new Record(list));


        }


        /// <summary>
        /// Vloží do databáze nový záznam obsahující zadané hodnoty. Typ hodnoty musí korespondovat s typem sloupce.
        /// </summary>
        /// <param name="record">Třída Record obsahující všechny hodnoty s typy korespondujícími s typem sloupců v tabulce.</param>
        /// <exception cref="Exception">Vrací Exception v případě, že je zadán špatný počet argumentů, nebo nesedí jejich typ.</exception>
        public void Insert(Record record)
        {

            // Pokud byla tabulka prázdná, již není
            if (_empty) _empty = false;

            // sedí počet?
            if (_fields.Count == record.Count)
            {

                for (int i = 0; i < record.Count; i++)

                    // Sedí typ?
                    if (_fields.ElementAt(i) != ((dynamic)record.ElementAt(i)).Value.GetType())
                    {

                        throw new Exception("Field type mismatch at index " + i);

                    }


                    else
                    {

                        if (_indexes.ContainsKey(i))    // jedná se o indexovaný sloupec, musíme hodnotu vložit do BVS

                            ((dynamic)_indexes[i]).Insert(((dynamic)record.ElementAt(i)).Value, (int)Count);

                    }


            }
            else

                throw new Exception("Invalid number of fields!");


            // Vložíme record do listu záznamů
            _records.Add(record);


        }


        /// <summary>
        /// Vytvoří prázdnou databázi.
        /// </summary>
        public Db() 
        {

        }


        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return new List<String>{ "Sender", "Receiver" }; // _columns;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {

            if (_columns.Contains(binder.Name))
            {

                int index = _columns.IndexOf(binder.Name);


                if (_fields[index] == typeof(bool))
                {

                    List<bool> list = new();

                    foreach (Record rec in _records)
                        list.Add(((dynamic)rec[index]).Value);

                    result = new BooleanColumn(list);

                    return true;

                }
                else
                {

                    var listType = typeof(List<>).MakeGenericType(_fields[index]);
                    var listInstance = (IList)Activator.CreateInstance(listType);

                    foreach (Record rec in _records)
                    {
                        listInstance.Add(((dynamic)rec[index]).Value);
                    }


                    Type genericColumn = typeof(Column<>).MakeGenericType(_fields[index]);

                    // Create an instance of the generic class
                    ConstructorInfo constructorInfo = genericColumn.GetConstructor(new Type[] { listInstance.GetType() });

                    // Create an instance of the generic class using the constructor
                    object genericColumnInstance = constructorInfo.Invoke(new object[] { listInstance });

                    result = genericColumnInstance;

                    return true;

                }

            }

            else // sloupce jsme nenalezli
            {

                result = null;

                return false;   // neúspěch

            }

        }

        public override string ToString()
        {
            var stringifier = new Stringifier(_columns.Count);
            stringifier.PushHeader(_columns);
            foreach (Record rec in _records)
                stringifier.PushFields(rec);

            return stringifier.ToString();
        }


        /// <summary>
        /// Zkopíruje strukturu jiné databáze, tedy typy a názvy sloupců a indexovací sloupce. Lze pouze, pokud je tabulka prázdná.
        /// </summary>
        /// <exception cref="Exception">Vrací Exception, když databáze není prázdná.</exception>
        /// <param name="other">Tabulka, jejíž strukturu chceme okopírovat.</param>
        public void CopyStructure(Db other)
        {
            if (!_empty) throw new Exception("Database is not empty!");

            _fields = other._fields;
            _columns = other._columns;
            _indexes = other._indexes;
        }


        /// <summary>
        /// Vyprázdní tabulku a smaže její strukturu.
        /// </summary>
        public void Drop()
        {
            _empty = true;
            _records = new();
            _indexes = new();
            _columns = new();
            _fields = new();
        }


        /// <summary>
        /// Vrátí velikost databáze, tedy počet záznamů v databázi..
        /// </summary>
        public int Count { get { return _records.Count; } }

        public int ColumnCount { get { return _columns.Count; } }


        /// <summary>
        /// Vrátí typ sloupce s daným názvem.
        /// </summary>
        /// <param name="name">Název sloupce.</param>
        /// <returns>Typ hodnot v daném sloupci.</returns>
        /// <exception cref="Exception">Vrací Exception, pokud daný sloupec v tabulce není.</exception>
        private Type ColumnType(string name)
        {
            if (!_columns.Contains(name)) throw new Exception("Invalid column!");

            return _fields.ElementAt(_columns.IndexOf(name));
        }

        /// <summary>
        /// Vrací index sloupce s daným názvem.
        /// </summary>
        /// <param name="name">Název sloupce.</param>
        /// <returns>Index sloupce = pořadí sloupce v tabulce.</returns>
        /// <exception cref="Exception">Vrací Exception, pokud daný sloupec neexistuje.</exception>
        private int ColumnIndex(string name)
        {
            if (!_columns.Contains(name)) throw new Exception("Invalid column!");

            return _columns.IndexOf(name);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<RecordWrapper> GetEnumerator()
        {
            foreach (Record rec in  _records)
            {
                yield return new RecordWrapper(rec, this);
            }
        }

        // seznam záznamů, pořadí je důležité kvůli indexování přes sloupce (pořadí záznamů drží uzly v BVS)
        private List<Record> _records = new();

        // je databáze prázdná, můžeme upravovat strukturu, je zde redundance, ale z důvodu rychlosti ověření
        private bool _empty = true;

        // seznam typů sloupců
        private List<Type> _fields = new();

        // seznam názvů sloupců, pro pohodlnou interakci s uživatelem
        private List<String> _columns = new();

        // slovník pořadí sloupce, jež slouží pro indexování, a odpovídajícího BVS
        private Dictionary<int, BST> _indexes = new();


    }
}
