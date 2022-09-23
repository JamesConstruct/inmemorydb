using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace InMemoryDB
{

    using Record = List<ParentField>;   // Record je souhrn polí.


    /// <summary>
    /// Mateřská třída pole umožňující polymorfismus.
    /// </summary>
    internal abstract class ParentField { }


    /// <summary>
    /// Třída pole pro konkrétní hodnotu.
    /// </summary>
    /// <typeparam name="T">Typ hodnoty pole.</typeparam>
    internal class Field<T> : ParentField where T : IComparable<T>
    {
        /// <summary>
        /// Hodnota pole.
        /// </summary>
        public T Value { get; set; }

        public static bool operator ==(Field<T> a, Field<T> b)
        {
            return EqualityComparer<T>.Default.Equals(a.Value, b.Value);
        }

        public static bool operator !=(Field<T> a, Field<T> b)
        {
            return !EqualityComparer<T>.Default.Equals(a.Value, b.Value);
        }

        public override bool Equals(object? o)
        {
            if (o == null) return false;
            if (o.GetType() != typeof(Field<T>)) return false;
            return this == o;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }


        /// <summary>
        /// Vytvoří pole dané hodnoty typu T.
        /// </summary>
        /// <param name="value">Hodnota pole.</param>
        public Field(T value)
        {
            Value = value;
        }

    }


    /// <summary>
    /// Hlavní třída databáze.
    /// </summary>
    internal class Db
    {

        /// <summary>
        /// Tato třída převádí typ pole konrkétní hodnoty na obecného předka ParentField. Zde je třeba přidat funkce pro další datové typy v případě rozšiřování.
        /// </summary>
        public static class FieldConvertor
        {

            public static ParentField GetField<T>(T val)
            {

                throw new NotImplementedException("Type " + typeof(T) + " is not implemented yet!");
            }

            public static ParentField GetField(int val)
            {

                return new Field<int>(val);
            }

            public static ParentField GetField(string val)
            {

                return new Field<string>(val);
            }

            public static ParentField GetField(double val)
            {

                return new Field<double>(val);
            }

            public static ParentField GetField(float val)
            {

                return new Field<float>(val);
            }

            public static ParentField GetField(char val)
            {

                return new Field<char>(val);
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
        }



        /// <summary>
        /// Vyhledá záznam v databázi, kde se daný sloupec <b>rovná</b> udané hodnotě. V případě, že hledáme dle indexu vyhledává logaritmicky, jinak lineárně. Vrátí první nalezený
        /// výsledek, nemusí se jednat o první v pořadí, v jakém byly záznamy přidávány.
        /// </summary>
        /// <typeparam name="T">Typ hodnoty, dle které vyhledáváme.</typeparam>
        /// <param name="column">Název sloupce, dle kterého vyhledáváme.</param>
        /// <param name="val">Hodnota, kterou chceme nalézt.</param>
        /// <returns>Vrací RecordWrapper nesoucí daný záznam.</returns>
        /// <exception cref="ArgumentException">ArgumentException v případě špatného typu sloupce.</exception>
        /// <exception cref="Exception">Exception v případě, že záznam není nalezen.</exception>
        public RecordWrapper SelectWhere<T>(string column, T val) where T : IComparable<T>
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
        /// Vytvoří prázdnou databázi.
        /// </summary>
        public Db() 
        {

        }


        /// <summary>
        /// Vrátí velikost databáze, tedy počet záznamů v databázi..
        /// </summary>
        public int Count { get { return _records.Count; } }


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
