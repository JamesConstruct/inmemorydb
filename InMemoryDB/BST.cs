using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryDB
{

    internal abstract class BST {}


    internal class BST<T> : BST where T : IComparable<T>
    {

        class Node
        {

            public T value; // hodnota uzlu

            public int RecordId;    // pořadí záznamu v databázi
        
            public Node? Left;  // Levý syn

            public Node? Right; // Pravý syn


            /// <summary>
            /// Inicializuje nový uzel s hodnotou typu T (value) odkazující na záznam s daným id.
            /// </summary>
            /// <param name="value">Hodnota uzlu</param>
            /// <param name="id">Id záznamu</param>
            public Node(T value, int id) { this.value = value; RecordId = id; }


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
            public override bool Equals(object? o)
            {
                if (o == null) return false;

                if (o.GetType() != typeof(BST<T>.Node))
                    return false;

                Node n = (Node)o;

                return RecordId == n.RecordId; // Comparer<T>.Default.Compare(value, n.value) == 0 && 
            }
        }

        Node? root = null;

        public int Find(T value)
        {

            Node? current = root;

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

            }


        }

        public void Insert(T value, int id)
        {

            Node? current = root;
            Node? next = null; // nadcházející uzel ve vyhledávání

            if (current == null)    // strom je zatím prázdný
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

                        break; // konec

                    }

                    current = next; // postupujeme dál

                }

        }

        public BST()
        {

        }

    }
}
