using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryDB
{

    internal class BST<T> where T : IEqualityComparer<T>
    {
        class Node
        {
            public T value;
            public UIntPtr RecordId;
            public Node? Left;
            public Node? Right;

            public Node(T value) { this.value = value; }

            public static bool operator<(Node a, T b)
            {
                // todo
                return true;
            }

            public static bool operator>(Node a, T b)
            {
                // todo
                return false;
            }

            public static bool operator ==(Node a, T b)
            {
                // todo
                return false;
            }


            public static bool operator !=(Node a, T b)
            {
                // todo
                return false;
            }

        }

        Node? root = null;

        public UIntPtr Find(T value)
        {

            Node? current = root;

            while (true)
            {
                if (current == null)
                    throw new Exception("Zaznam nebyl nalezen.");
                if (current == value)
                    return current.RecordId;
                else if (current > value)
                    current = current.Left;
                else
                    current = current.Right;

            }

        }

        public void Insert(T value, UIntPtr recordId)
        {

            Node? current = root;

            while (current != null)
            {
               
                if (current > value)
                    current = current.Left;
                else
                    current = current.Right;

                // jak asi vypadá indexování a hledání dle čísel????? wtf

            }

            current = new Node(value);
        }

    }
}
