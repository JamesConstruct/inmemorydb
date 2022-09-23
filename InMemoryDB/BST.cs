using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryDB
{

    internal abstract class BST {
        //public virtual void Insert(T value, UIntPtr id) { throw new Exception(); }
        //public virtual UIntPtr Find(T value) { throw new Exception(); }
    }


    internal class BST<T> : BST where T : IComparable<T>
    {
        class Node
        {
            public T value;
            public UIntPtr RecordId;
            //public Record Rec;
            public Node? Left;
            public Node? Right;

            public Node(T value, UIntPtr id) { this.value = value; RecordId = id; }

            public static bool operator<(Node a, T b)
            {
                return Comparer<T>.Default.Compare(a.value, b) < 0;
            }

            public static bool operator>(Node a, T b)
            {
                return Comparer<T>.Default.Compare(a.value, b) > 0;
            }

            public static bool operator ==(Node a, T b)
            {
                return Comparer<T>.Default.Compare(a.value, b) == 0;
            }


            public static bool operator !=(Node a, T b)
            {
                return Comparer<T>.Default.Compare(a.value, b) != 0;
            }

            public override int GetHashCode()
            {
                return RecordId.GetHashCode(); // zahrnout zde potomky a předky?
            }

            // nezahrnuje potomky
            public override bool Equals(object o)
            {
                if (o == null) return false;

                if (o.GetType() != typeof(BST<T>.Node))
                    return false;

                Node n = (Node)o;

                return RecordId == n.RecordId; // Comparer<T>.Default.Compare(value, n.value) == 0 && 
            }
        }

        Node? root = null;

        public UIntPtr Find(T value)
        {

            Node? current = root;

            if (current.Left != null)
                Console.Write(current.Left.value + " - ");

            Console.Write(current.value);

            if (current.Right != null)
                Console.Write(" - " + current.Right.value);
            Console.WriteLine();

            while (true)
            {
                if (current == null)
                    throw new Exception("Zaznam nebyl nalezen.");
                if ((Node)current == value)
                    return current.RecordId;
                else if (current > value)
                    current = current.Left;
                else
                    current = current.Right;

                if (current.Left != null)
                    Console.Write(current.Left.value + " - ");

                Console.Write(current.value);

                if (current.Right != null)
                    Console.Write(" - " + current.Right.value);
                Console.WriteLine();
            }

        }

        public void Insert(T value, UIntPtr id)
        {

            Node? current = root;
            Node? next = null;

            if (current == null)
                root = new Node(value, id);
            else
                while (true)
                {
               
                    if (current > value)
                        next = current.Left;
                    else
                        next = current.Right;

                    if (next == null) // nalezli jsme místo, kam můžeme uzel přidat
                    {
                        if (current > value)
                            current.Left = new Node(value, id);
                        else
                            current.Right = new Node(value, id);
                        break;
                    }

                    current = next;

                }

        }

        public BST()
        {

        }
    }
}
