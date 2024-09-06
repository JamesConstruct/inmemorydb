using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryDB
{

    /// <summary>
    /// Mateřská třída pole umožňující polymorfismus.
    /// </summary>
    public abstract class ParentField {

        public override abstract string ToString();

    }


    /// <summary>
    /// Třída pole pro konkrétní hodnotu.
    /// </summary>
    /// <typeparam name="T">Typ hodnoty pole.</typeparam>
    public class Field<T> : ParentField where T : IComparable<T>
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

        public override string ToString()
        {
            return Value.ToString();
        }

    }
}
