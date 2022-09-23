using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace InMemoryDB
{

    using Record = List<ParentField>;

    // Mateřská třída pole, umožňuje polymorfismus
    internal abstract class ParentField { }

    // konkrétní třídy pole pro daný typ hodnoty
    internal class Field<T> : ParentField where T : IComparable<T>
    {
        public T Value { get; set; }

        public static bool operator ==(Field<T> a, Field<T> b)
        {
            return EqualityComparer<T>.Default.Equals(a.Value, b.Value);
        }

        public static bool operator !=(Field<T> a, Field<T> b)
        {
            return !EqualityComparer<T>.Default.Equals(a.Value, b.Value);
        }

        public Field(T value)
        {
            Value=value;
        }

    }

    internal class Db
    {

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


        public sealed class RecordWrapper : DynamicObject
        {
            private Record _record;
            private readonly Db _db;

            public RecordWrapper(Record record, Db db)
            {
                _record=record;
                _db=db;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return _db._columns;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (_db._columns.Contains(binder.Name))
                {
                    int index = _db._columns.IndexOf(binder.Name);
                    dynamic tmp = _record.ElementAt(index);
                    result = tmp.Value;
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
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
                return false;
            }
        }

        public RecordWrapper SelectWhere<T>(string column, T val) where T : IComparable<T>
        {
            if (ColumnType(column) != typeof(T)) throw new ArgumentException("Invalid value type!");

            int index = _columns.IndexOf(column);

            foreach (Record rec in _records)
            {
                if (EqualityComparer<T>.Default.Equals(((Field<T>)rec.ElementAt(index)).Value, val))
                {
                    Console.WriteLine("Got him!");

                    
                    Console.WriteLine(bst.Find(val.ToString()));

                    return new RecordWrapper(rec, this);
                }
            }

            throw new Exception("Not found!");
        }

        public T GetSum<T>(string column) where T : new()
        {
            if (ColumnType(column) != typeof(T)) throw new ArgumentException("Invalid value type!");

            int index = _columns.IndexOf(column);

            T sum = new();

            foreach (Record rec in _records)
            {
                dynamic obj = rec.ElementAt(index);
                sum += obj.Value;
            }

            return sum;

        }


        // přidá sloupec daného typu s daným názvem, name musí být unikátní
        public void AddColumn<T>(string name)
        {
            if (!_empty) throw new Exception("Database is not empty!");

            if (_columns.Contains(name)) throw new Exception("Column " + name + " already exists!");

            _fields.Add(typeof(T));
            _columns.Add(name);
        }

        // Odstraní sloupce na pozici indexu
        public void RemoveColumnAt(int index)
        {
            if (!_empty) throw new Exception("Database is not empty!");

            _fields.RemoveAt(index);
            _columns.RemoveAt(index);
        }

        // Odstraní daný sloupec dle jména
        public void RemoveColumn(string name)
        {
            if (!_empty) throw new Exception("Database is not empty!");

            if (!_columns.Contains(name)) throw new Exception("Column " + name + " doesn't exist!");

            int index = _columns.IndexOf(name);

            RemoveColumnAt(index);
        }

        public void MakeIndex()
        {

        }



        public void Insert(params object[] values)
        {
            if (_empty) _empty = false;

            List<ParentField> list = new List<ParentField>();

            if (_fields.Count == values.Length)

                for (int i = 0; i < values.Length; i++)

                    if (_fields.ElementAt(i) != values.ElementAt(i).GetType())

                        throw new Exception("Field type mismatch at index " + i);

                    else
                    {

                        dynamic tmp = (dynamic)values[i];

                        list.Add(FieldConvertor.GetField(tmp));

                    }


            else

                throw new Exception("Invalid number of fields!");


            _records.Add(new Record(list));

            bst.Insert((string)values[1], (UIntPtr)Count);

        }

        public BST<string> bst = new();

        public Db() 
        {

        }

        public int Count { get { return _records.Count; } }
        
        private List<Record> _records = new();
        private bool _empty = true;  // dokud je db prázdná, můžeme upravovat sloupce, redundance??
        private List<Type> _fields = new();
        private List<String> _columns = new();

        private Type ColumnType(string name)
        {
            if (!_columns.Contains(name)) throw new Exception("Invalid column!");

            return _fields.ElementAt(_columns.IndexOf(name));
        }


    }
}
