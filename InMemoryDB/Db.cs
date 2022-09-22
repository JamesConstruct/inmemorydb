using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryDB
{
    internal class Db
    {

        public class Record
        {
            public int Id { get; set; }

            public List<ParentField> Fields { get; set; }

            public Record(List<ParentField> fields)
            {
                Fields = fields;
            }

        }

        // Mateřská třída pole, umožňuje polymorfismus
        public abstract class ParentField { }

        // konkrétní třídy pole pro daný typ hodnoty
        public class Field<T> : ParentField where T : IComparable<T>
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

        ParentField GetField<T>(T val)
        {

            throw new NotImplementedException("Type " + typeof(T) + " is not implemented yet!");
        }

        ParentField GetField(int val) {

            return new Field<int>(val);
        }

        ParentField GetField(string val)
        {

            return new Field<string>(val);
        }

        ParentField GetField(double val)
        {

            return new Field<double>(val);
        }

        ParentField GetField(float val)
        {

            return new Field<float>(val);
        }

        ParentField GetField(char val)
        {

            return new Field<char>(val);
        }

        /*public class Select
        {
            public Select Where<T>(string column, T value)
            {
                ColumnType(ref column);
            }
        }*/

        public void SelectWhere<T>(string column, T val) where T : IComparable<T>
        {
            if (ColumnType(column) != typeof(T)) throw new ArgumentException("Invalid value type!");
            
            int index = _columns.IndexOf(column);

            foreach (Record rec in _records)
            {
                if (EqualityComparer<T>.Default.Equals(((Field<T>)rec.Fields.ElementAt(index)).Value, val))
                {
                    Console.WriteLine("Got him!");
                }
            }
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



        public void Insert(params object[] values) // params object[] values nelze použít, protože object nemá vynucený IEqutable interface
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

                        list.Add(GetField(tmp));

                    }


            else

                throw new Exception("Invalid number of fields!");


            _records.Add(new Record(list));

        }


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
