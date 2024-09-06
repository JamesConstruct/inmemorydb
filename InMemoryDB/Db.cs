using System.Collections;
using System.Dynamic;
using System.Reflection;



namespace InMemoryDB
{

    using Record = List<ParentField>;   // Record je souhrn polí.


    /// <summary>
    /// 
    /// Jednoduchá **in-memory** databáze napsaná v C# jako zápočtový projekt do Programování v C#.
    /// Databáze má polymorfní strukturu a může obsahovat libovolný počet sloupců (omezení pamětí) různého druhu. Databáze nativně podporuje základní datové typy C#, avšak
    /// je možné ji snadno rozšířit tak, aby pracovala s jakýmkoli typem implementujícím IComparable a IEquitable interface. Databáze podporuje indexování a binární vyhledávání
    /// v logaritmickém čase, stejně jako pokročilé možnosti filtrování záznamů.
    /// 
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }


    /// <summary>
    /// The main in-memory database object.
    /// </summary>
    public class Db : DynamicObject, IEnumerable
    {

        /// <summary>
        /// This class converts values of specific type to the abstract ParentField.
        /// </summary>
        internal static class FieldConvertor  // public aby šly připisovat další Convertory
        {

            public static ParentField GetField<T>(T val) where T : IComparable<T>
            {

                return new Field<T>(val);

            }

        }


        /// <summary>
        /// Class that wraps around the Record and allows for dynamic access of values by column names (based on the columns of the underlying database).
        /// </summary>
        public sealed class RecordWrapper : DynamicObject
        {
            private Record _record; // záznam
            private readonly Db _db;    // databáze, ze které pochází (kvůli jménům sloupců)


            /// <summary>
            /// Creates the new wrapper around the given record from the given database.
            /// </summary>
            /// <param name="record">Record from a database.</param>
            /// <param name="db">Database where the record belongs.</param>
            public RecordWrapper(Record record, Db db)
            {
                _record = record;
                _db = db;
            }

            /// <summary>
            /// Returns the names of the columns of the parent database (columns defined for this record).
            /// </summary>
            /// <returns>The column names.</returns>
            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return _db._columns;
            }

            /// <summary>
            /// Gives dynamic access to the values of the record by the names of the columns.
            /// </summary>
            /// <param name="binder">Name of the column.</param>
            /// <param name="result">Value of the column in the current record. The value is of the same type as was defined during the column addition in the database structure.</param>
            /// <returns>True if success, false otherwise.</returns>
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

            /// <summary>
            /// Tries to set the column value, but that is currently not supported.
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="value"></param>
            /// <returns></returns>
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

            /// <summary>
            /// Converts the record to string including the header of the table and the values of the record.
            /// </summary>
            /// <returns>Stringified record with header.</returns>
            public override string ToString()
            {
                var strngfr = new Stringifier(_db._columns.Count);
                strngfr.PushHeader(_db._columns);
                strngfr.PushFields(_record);
                return strngfr.ToString();

            }
        }


        /// <summary>
        /// Returns the first row in the database.
        /// </summary>
        /// <returns>RecordWrapper with the first record from the database.</returns>
        public RecordWrapper First()
        {
            return new RecordWrapper(_records.First(), this);
        }


        /// <summary>
        /// Returns the last record in the database.
        /// </summary>
        /// <returns>RecordWrapper with the last record from the database.</returns>
        public RecordWrapper Last()
        {
            return new RecordWrapper(_records.Last(), this);
        }


        /// <summary>
        /// Returns the row on the specified position in the database.
        /// </summary>
        /// <param name="i">Position of the record.</param>
        /// <returns>RecordWrapper with the specified row.</returns>
        /// <exception cref="Exception">Throws Exception, if the given position is invalid.</exception>
        public RecordWrapper RecordAt(int i)
        {
            if (i < 0 || i >= _records.Count) throw new Exception("Invalid index!");

            return new RecordWrapper(_records.ElementAt(i), this);
        }

        /// <summary>
        /// Returns the RecordWrapper of record on the given position in the database.
        /// </summary>
        /// <param name="index">Index of the record to return.</param>
        /// <returns>RecordWrapper around the record on the given position.</returns>
        public RecordWrapper this[int index]
        {
            get
            {
                return RecordAt(index);
            }
        }


        /// <summary>
        /// Finds a single record in the database, where its value <b>equals</b> the given value. In case of indexed columns, the search is logarithmic with respect to the number of
        /// rows in the database. Otherwise, the search is linear. Returns the <b>first found</b> that satisfies the given condition, which might not be the first one added to the dabase.
        /// </summary>
        /// <typeparam name="T">Type of the column used to search the database.</typeparam>
        /// <param name="column">Name of the column used to search the database.</param>
        /// <param name="val">Searched value</param>
        /// <returns>Returns RecordWrapper with the found record.</returns>
        /// <exception cref="ArgumentException">ArgumentException in case of invalid type of the column.</exception>
        /// <exception cref="Exception">Exception in case no record is found.</exception>
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
        /// Finds a single record in the database, where its value <b>equals</b> the given value. In case of indexed columns, the search is logarithmic with respect to the number of
        /// rows in the database. Otherwise, the search is linear. Returns a new database with all the found records. The returned table can be empty. 
        /// </summary>
        /// <typeparam name="T">Type of the column used for searching.</typeparam>
        /// <param name="column">Name of the column used for searching.</param>
        /// <param name="val">The searched value.</param>
        /// <returns>Returns a new Database object with identical structure containing only the found records.</returns>
        /// <exception cref="ArgumentException">ArgumentException in case of invalid type of the column.</exception>
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


        /// <summary>
        /// Applies a boolean filter to the database and returns a new database containing only selected rows. That is, if i-th value in the BooleanColumn is true, the i-th
        /// row is part to the returned database.
        /// </summary>
        /// <param name="filter">BooleanColumn used to filter the database, has to be of the same length as the number of entries in the database.</param>
        /// <returns>New filtered database.</returns>
        /// <exception cref="ArgumentException">Throws ArgumentException if the Length of the filter doesn't match the size of the database.</exception>
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
        /// Calculates the sum of all records in the given column.
        /// </summary>
        /// <typeparam name="T">Type of summed values.</typeparam>
        /// <param name="column">The name of the summed column.</param>
        /// <returns>Returns the cummulative sum of the entire column.</returns>
        /// <exception cref="ArgumentException">ArgumentException in case of mismatch between the type of the column and provided type T.</exception>
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
        /// Adds a column of type T and the given name to the database. The name of the column has to be unique. The table has to be empty to add a new column.
        /// </summary>
        /// <typeparam name="T">Type of the values in the new column.</typeparam>
        /// <param name="name">The name of the new column.</param>
        /// <exception cref="Exception">Throws exception in case of non-unique name of the column or non-empty table.</exception>
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
        /// Removes the column on the given position in the database (position corresponds to the order of addition of the columns). The database has to be empty.
        /// </summary>
        /// <param name="index">The position of the column to remove.</param>
        /// <exception cref="Exception">Throws an Exception in case of non-empty database or invalid column index.</exception>
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
        /// Removes the column of the given name from the database. The database has to be empty.
        /// </summary>
        /// <param name="name">The name of the column to remove.</param>
        /// <exception cref="Exception">Throws an Exception in case of non-empty database or invalid column name.</exception>
        public void RemoveColumn(string name)
        {
            if (!_empty) throw new Exception("Database is not empty!");

            if (!_columns.Contains(name)) throw new Exception("Column " + name + " doesn't exist!");

            int index = _columns.IndexOf(name);

            RemoveColumnAt(index);
        }



        /// <summary>
        /// Turns the specified column to an index, allowing for binary search. The database has to be empty.
        /// </summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="column">Name of the column.</param>
        /// <exception cref="ArgumentException">Throws ArgumentException in case of the column type and provided type T mismatch.</exception>
        /// <exception cref="Exception">Throws an Exception if the database isn't empty.</exception>
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
        /// Inserts a new record to the database containing the given values. These values have to correspond to the types of their respective columns.
        /// </summary>
        /// <param name="values">Values of one record.</param>
        /// <exception cref="Exception">Throws an Exception in case of invalid number of values or mismatch of the types.</exception>
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

                            ((dynamic)_indexes[i]).Insert(tmp, (int)Count);

                    }


            else

                throw new Exception("Invalid number of fields!");


            // Vložíme record do listu záznamů
            _records.Add(new Record(list));


        }


        /// <summary>
        /// Inserts the given record to the database. The types of fields in this record have to correspond to the types of the columns in the database.
        /// </summary>
        /// <param name="record">Record with matching field types.</param>
        /// <exception cref="Exception">Throws an Exception in case of invalid number of fields or in case of type mismatch.</exception>
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
        /// Creates an empty database.
        /// </summary>
        public Db()
        {

        }

        /// <summary>
        /// Returns the names of the created columns in the database.
        /// </summary>
        /// <returns>Column names</returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _columns;
        }

        /// <summary>
        /// Gives get-only access to the database columns. These are returned as Column with the specific type of the column.
        /// </summary>
        /// <param name="binder">Name of the database column.</param>
        /// <param name="result">Object to return the Column class to.</param>
        /// <returns>True if success, false otherwise.</returns>
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
                    var listInstance = (IList)Activator.CreateInstance(listType)!;

                    foreach (Record rec in _records)
                    {
                        listInstance.Add(((dynamic)rec[index]).Value);
                    }


                    Type genericColumn = typeof(Column<>).MakeGenericType(_fields[index]);

                    // Create an instance of the generic class
                    ConstructorInfo constructorInfo = genericColumn.GetConstructor(new Type[] { listInstance.GetType() })!;

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

        /// <summary>
        /// Converts the table to a string, including a header and all the rows. Every row is converted to a string field by field (every Field is 
        /// converted to a string representation of its value). The table is formated to a fixed size, overly large values are shortened using "...".
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var stringifier = new Stringifier(_columns.Count);
            stringifier.PushHeader(_columns);
            foreach (Record rec in _records)
                stringifier.PushFields(rec);

            return stringifier.ToString();
        }


        /// <summary>
        /// Copies the structure of a different database to itself, including the types and names of columns and which columns are indexed. Only possible with an empty
        /// database (the copied database does not have to be empty).
        /// </summary>
        /// <exception cref="Exception">Throws an Exception in case of non-empty database.</exception>
        /// <param name="other">Database to copy the structure from.</param>
        public void CopyStructure(Db other)
        {
            if (!_empty) throw new Exception("Database is not empty!");

            _fields = other._fields;
            _columns = other._columns;
            _indexes = other._indexes;
        }


        /// <summary>
        /// Empties the table (deletes all entries) and drops the internal structure, including columns and indexes.
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
        /// Number of entries (rows) in the database.
        /// </summary>
        public int Count { get { return _records.Count; } }

        /// <summary>
        /// The number of columns in the database.
        /// </summary>
        public int ColumnCount { get { return _columns.Count; } }


        /// <summary>
        /// Returns the type of the column with the given name.
        /// </summary>
        /// <param name="name">Name of the column.</param>
        /// <returns>Type of values in the specified column.</returns>
        /// <exception cref="Exception">Throws an Exception in case of absence of the specified column in the database.</exception>
        private Type ColumnType(string name)
        {
            if (!_columns.Contains(name)) throw new Exception("Invalid column!");

            return _fields.ElementAt(_columns.IndexOf(name));
        }

        /// <summary>
        /// Returns the index of the column with the given name.
        /// </summary>
        /// <param name="name">Name of the column.</param>
        /// <returns>Index of the specified column in the database (corresponds to the order of the addition of the columns).</returns>
        /// <exception cref="Exception">Throws an Exception if column of such name doesn't exist.</exception>
        private int ColumnIndex(string name)
        {
            if (!_columns.Contains(name)) throw new Exception("Invalid column!");

            return _columns.IndexOf(name);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Creates an enumarator for the database, enumarating by the **rows** in the database.
        /// </summary>
        /// <returns>The RecordWrapper enumerator.</returns>
        public IEnumerator<RecordWrapper> GetEnumerator()
        {
            foreach (Record rec in _records)
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
