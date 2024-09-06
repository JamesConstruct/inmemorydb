using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryDB
{


    /// <summary>
    /// Column represents an entire column from table - list of values of the same type accross all rows in the database.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IColumn<T>
    {
        /// <summary>
        /// All columns have accessible items (rows)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        T this[int index] { get; }

        int Length { get; }
    }


    /// <summary>
    /// Generic class for column (internally a list of T values with added functionality for transformations and stuff).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Column<T> : IColumn<T> where T : IComparable<T>, IEquatable<T>
    {

        public readonly List<T> Contents;

        /// <summary>
        /// Create a new instance of Column<typeparamref name="T"/> from list of T.
        /// </summary>
        /// <param name="contents"></param>
        public Column(List<T> contents) {
            this.Contents = contents;
        }

        /// <summary>
        /// The length of the column (how many rows it spans).
        /// </summary>
        public int Length
        {
            get
            {
                return Contents.Count;
            }
        }


        public static BooleanColumn operator == (Column<T> left, Column<T> right)
        {
            if (left.Contents.Count != right.Contents.Count) throw new ArgumentException("Columns are of a different length!");

            List<bool> new_contents = new List<bool>();

            for (int i = 0;  i < left.Contents.Count; i++)
            {
                new_contents.Add(left.Contents[i].Equals(right.Contents[i]));
            }

            return new BooleanColumn(new_contents);
        }

        public static BooleanColumn operator !=(Column<T> left, Column<T> right)
        {
            if (left.Contents.Count != right.Contents.Count) throw new ArgumentException("Columns are of a different length!");

            List<bool> new_contents = new List<bool>();

            for (int i = 0; i < left.Contents.Count; i++)
            {
                new_contents.Add(!left.Contents[i].Equals(right.Contents[i]));
            }

            return new BooleanColumn(new_contents);
        }

        /// <summary>
        /// Compare the column with a T value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="val"></param>
        /// <returns>BooleanColumn with true on the i-th position if i-th element equals val</returns>
        public static BooleanColumn operator ==(Column<T> left, T val)
        {

            List<bool> new_contents = new List<bool>();

            for (int i = 0; i < left.Contents.Count; i++)
            {
                new_contents.Add(left.Contents[i].Equals(val));
            }

            return new BooleanColumn(new_contents);
        }


        public static BooleanColumn operator !=(Column<T> left, T val)
        {

            List<bool> new_contents = new List<bool>();

            for (int i = 0; i < left.Contents.Count; i++)
            {
                new_contents.Add(!left.Contents[i].Equals(val));
            }

            return new BooleanColumn(new_contents);
        }

        public static BooleanColumn operator >(Column<T> left, T val)
        {

            List<bool> new_contents = new List<bool>();

            for (int i = 0; i < left.Contents.Count; i++)
            {
                new_contents.Add(left.Contents[i].CompareTo(val) > 0);
            }

            return new BooleanColumn(new_contents);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static BooleanColumn operator <(Column<T> left, T val)
        {

            List<bool> new_contents = new List<bool>();

            for (int i = 0; i < left.Contents.Count; i++)
            {
                new_contents.Add(left.Contents[i].CompareTo(val) < 0);
            }

            return new BooleanColumn(new_contents);
        }

        public BooleanColumn Compare(Column<T> other, Func<T, T, bool> comparator)
        {
            if (Length != other.Length) throw new ArgumentException("Columns of different lengths cannot be compared!");

            List<bool> new_contents = new List<bool>();

            for (int i = 0; i < Contents.Count; i++)
            {
                new_contents.Add(comparator(Contents[i], other[i]));
            }

            return new BooleanColumn(new_contents);
        }

        public Column<T> Transform(Func<T, T> transformator)
        {

            List<T> new_contents = new List<T>();

            for (int i = 0; i < Contents.Count; i++)
            {
                new_contents.Add(transformator(Contents[i]));
            }

            return new Column<T>(new_contents);
        }

        public static Column<T> operator &(Column<T> col, Func<T, T> transform)
        {
            return col.Transform(transform);
        }

        public Column<V> TransformTo<V>(Func<T, V> transform) where V : IComparable<V>, IEquatable<V>
        {
            List<V> new_contents = new List<V>();

            for (int i = 0; i < Contents.Count; i++)
            {
                new_contents.Add(transform(Contents[i]));
            }

            return new Column<V>(new_contents);
        }

        //public static BooleanColumn operator !(Column<T> column)
        //{
        //    List<bool> new_contents = new List<bool>();

        //    if (typeof(T) == typeof(bool)) {

        //        foreach (var element in column._contents)
        //        {

        //            new_contents.Add(!(dynamic)element);

        //        }

        //        return new BooleanColumn(new_contents);

        //    }

        //    else
        //    {
        //        throw new NotSupportedException("Only boolean columns can be negated.");
        //    }


        //}

        //public static BooleanColumn operator & (Column<T> left, Column<T> right)
        //{
        //    return new BooleanColumn(new List<bool>());
        //}

        public T this[int index]
        {
            get
            {
                return (T)this.Contents[index];
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            foreach(var el in Contents)
            {
                sb.Append(el.ToString());
                sb.Append(' ');
            }
            return sb.ToString();
        }

    }

    /// <summary>
    /// Class representing a column of only boolean values. Supports logic operators like & (and), | (or) and ! (not).
    /// </summary>
    public class BooleanColumn : IColumn<bool>
    {
        private List<bool> _contents;

        /// <summary>
        /// Create a column from list of bools.
        /// </summary>
        /// <param name="contents"></param>
        public BooleanColumn(List<bool> contents) {
            this._contents = contents;
        }


        /// <summary>
        /// Get an i-th element of the column (element corresponding to the i-th row)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool this[int index]
        {
            get
            {
                return this._contents[index];
            }
        }

        /// <summary>
        /// The length of the column i.e. how many rows it spans.
        /// </summary>
        public int Length
        {
            get
            {
                return _contents.Count;
            }
        }

        public static BooleanColumn operator | (BooleanColumn a, BooleanColumn b)
        {
            if (a.Length != b.Length) throw new ArgumentException("Columns of different lengths cannot be compared!");

            List<bool> list = new();
            for (int i = 0; i < a.Length; i++)
            {
                list.Add(a[i] || b[i]);
            }

            return new BooleanColumn(list);

        }

        public static BooleanColumn operator &(BooleanColumn a, BooleanColumn b)
        {
            if (a.Length != b.Length) throw new ArgumentException("Columns of different lengths cannot be compared!");

            List<bool> list = new();
            for (int i = 0; i < a.Length; i++)
            {
                list.Add(a[i] && b[i]);
            }

            return new BooleanColumn(list);

        }

        public static BooleanColumn operator !(BooleanColumn col)
        {
            List<bool> list = new();
            for (int i = 0; i < col.Length; i++)
                list.Add(!col[i]);

            return new BooleanColumn(list);
        }
    }

}
