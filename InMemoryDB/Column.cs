using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        /// <param name="left">The column for comparison.</param>
        /// <param name="val">Value to compare with.</param>
        /// <returns>BooleanColumn with true on the i-th position if the i-th element equals val</returns>
        public static BooleanColumn operator ==(Column<T> left, T val)
        {

            List<bool> new_contents = new List<bool>();

            for (int i = 0; i < left.Contents.Count; i++)
            {
                new_contents.Add(left.Contents[i].Equals(val));
            }

            return new BooleanColumn(new_contents);
        }


        /// <summary>
        /// Compares every value in the column to the given value.
        /// </summary>
        /// <param name="left">Column to compare.</param>
        /// <param name="val">Value to compare to.</param>
        /// <returns></returns>
        public static BooleanColumn operator !=(Column<T> left, T val)
        {

            List<bool> new_contents = new List<bool>();

            for (int i = 0; i < left.Contents.Count; i++)
            {
                new_contents.Add(!left.Contents[i].Equals(val));
            }

            return new BooleanColumn(new_contents);
        }

        /// <summary>
        /// Compares every value in the column to the given value.
        /// </summary>
        /// <param name="left">Column to compare.</param>
        /// <param name="val">Value to compare to.</param>
        /// <returns></returns>
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
        /// Compares every value in the column to the given value.
        /// </summary>
        /// <param name="left">Column to compare.</param>
        /// <param name="val">Value to compare to.</param>
        /// <returns>BooleanColumn</returns>
        public static BooleanColumn operator <(Column<T> left, T val)
        {

            List<bool> new_contents = new List<bool>();

            for (int i = 0; i < left.Contents.Count; i++)
            {
                new_contents.Add(left.Contents[i].CompareTo(val) < 0);
            }

            return new BooleanColumn(new_contents);
        }

        /// <summary>
        /// Compares the column with the other using the given comparator. The columns are compared element by element, meaning the result of i-th comparasion is true, provided
        /// the comparator returns true for the i-th element of the first and the i-th element of the second column.
        /// </summary>
        /// <param name="other">Column to compare with, has to be of the same length.</param>
        /// <param name="comparator">Function to compare two elements.</param>
        /// <returns>BooleanColumn</returns>
        /// <exception cref="ArgumentException">Throws ArgumentException if the columns are of a different length.</exception>
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

        // the transform function for type V can be used to transform the column into the same type
        ///// <summary>
        ///// Transf
        ///// </summary>
        ///// <param name="transformator"></param>
        ///// <returns></returns>
        //public Column<T> Transform(Func<T, T> transformator)
        //{

        //    List<T> new_contents = new List<T>();

        //    for (int i = 0; i < Contents.Count; i++)
        //    {
        //        new_contents.Add(transformator(Contents[i]));
        //    }

        //    return new Column<T>(new_contents);
        //}

        public static Column<T> operator &(Column<T> col, Func<T, T> transform)
        {
            return col.Transform(transform);
        }

        /// <summary>
        /// Transforms the Column<typeparamref name="T"/> into Column<typeparamref name="V"/> of a different underlying type V given the transformation function transform for each element.
        /// </summary>
        /// <typeparam name="V">Resulting type after the transformation.</typeparam>
        /// <param name="transform">Function that transforms each element of type T to the type V.</param>
        /// <returns>The entire transformed column.</returns>
        public Column<V> Transform<V>(Func<T, V> transform) where V : IComparable<V>, IEquatable<V>
        {
            List<V> new_contents = new List<V>();

            for (int i = 0; i < Contents.Count; i++)
            {
                new_contents.Add(transform(Contents[i]));
            }

            return new Column<V>(new_contents);
        }


        /// <summary>
        /// Indexer returning the element at the given index.
        /// </summary>
        /// <param name="index">Index of the element.</param>
        /// <returns>Element of type T at the given index.</returns>
        public T this[int index]
        {
            get
            {
                return (T)this.Contents[index];
            }
        }

        /// <summary>
        /// Converts the column to string with space between every element.
        /// </summary>
        /// <returns>String in format "element1 element2 ...."</returns>
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


        /// <summary>
        /// Compares the Column with another object. Columns are equal when the underlying lists are equal (list can be accessed as column.Content).
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns>True if the other object is also Column<typeparamref name="T"/> with equal underlying list.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            return obj is Column<T> && (this.Contents == ((Column<T>)obj).Contents);
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
