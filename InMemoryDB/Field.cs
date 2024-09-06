﻿namespace InMemoryDB
{

    /// <summary>
    /// Mateřská třída pole umožňující polymorfismus.
    /// </summary>
    public abstract class ParentField
    {

        /// <summary>
        /// Returns the string representation of the field.
        /// </summary>
        /// <returns></returns>
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


        /// <summary>
        /// Compares two fields based on their underlying value.
        /// </summary>
        /// <param name="a">First field to compare.</param>
        /// <param name="b">Second field to compare.</param>
        /// <returns>True if field a has the same value as the field b, otherwise false.</returns>
        public static bool operator ==(Field<T> a, Field<T> b)
        {
            return EqualityComparer<T>.Default.Equals(a.Value, b.Value);
        }
        /// <summary>
        /// Compares two fields based on their underlying value.
        /// </summary>
        /// <param name="a">First field to compare.</param>
        /// <param name="b">Second field to compare.</param>
        /// <returns>True if field a has a different value from the field b, otherwise false.</returns>
        public static bool operator !=(Field<T> a, Field<T> b)
        {
            return !EqualityComparer<T>.Default.Equals(a.Value, b.Value);
        }

        /// <summary>
        /// Compares the field with a given object.
        /// </summary>
        /// <param name="o">Object to compare with.</param>
        /// <returns>True if the object o is of the same type and has the same value.</returns>
        public override bool Equals(object? o)
        {
            if (o == null) return false;
            if (o.GetType() != typeof(Field<T>)) return false;
            return this == o;
        }
        /// <inheritdoc/>

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

        /// <summary>
        /// Converts the underlying value to string and returns it.
        /// </summary>
        /// <returns>String representation of the underlying value.</returns>
        public override string ToString()
        {
            return Value.ToString()!;
        }

    }
}
