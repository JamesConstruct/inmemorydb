namespace InMemoryDB
{

    /// <summary>
    /// Interface pro konkrétní vyhledávací strom.
    /// </summary>
    public interface ITree<T>
    {
        /// <summary>
        /// Vyhledá první prvek s danou hodnotou ve stromě a vrátí jeho id
        /// </summary>
        /// <param name="value">Hodnota, kterou hledáme.</param>
        /// <returns>Int Id daného záznamu.</returns>
        public int Find(T value);

        /// <summary>
        /// Vyhledá všechny prvky s danou hodnotou ve stromě a vrátí seznam jejich id.
        /// </summary>
        /// <param name="value">Hodnota, kterou hledáme.</param>
        /// <returns>List of ints Id daných záznamů.</returns>
        public List<int> FindAll(T value);

        /// <summary>
        /// Umožňuje vložit nový uzel do stromu s hodnotou value odkazující na záznam s daným Id.
        /// </summary>
        /// <param name="value">Hodnota uzlu pro vyhledávání.</param>
        /// <param name="id">Id na které odkazuje.</param>
        public void Insert(T value, int id);
    }

    /// <summary>
    /// Obecná třída vyhledávacích stromů. Kvůli polymorfismu.
    /// </summary>
    internal abstract class BST { }


    /// <summary>
    /// Vyhledávací strom daného typu - s hodnotami typu T.
    /// </summary>
    /// <typeparam name="T">Všechny uzlu stromu mají hodnotu tohoto typu.</typeparam>
    internal class BST<T> : BST, ITree<T> where T : IComparable<T>
    {

        /// <summary>
        /// Třída uzlu ve vyhledávácím stromě.
        /// </summary>
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


            public static bool operator <(Node a, T b)
            {
                return Comparer<T>.Default.Compare(a.value, b) < 0;
            }

            public static bool operator >(Node a, T b)
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

        /// <summary>
        /// Najde první uzel s danou hodnotou a vrátí Id záznamu, na který ukazuje.
        /// </summary>
        /// <param name="value">Hodnota, kterou hledáme.</param>
        /// <returns>Id záznamu, na který uzel ukazuje.</returns>
        /// <exception cref="Exception">Vyhodí exception, pokud daná hodnota ve stromě (tedy v databázi) neexistuje.</exception>
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

        /// <summary>
        /// Najde všechny uzly s danou hodnotou a vrátí list Id záznamů, které jim odpovídají. Vrací prázdný seznam, pokud nic nenajde.
        /// </summary>
        /// <param name="value">Hodnota, kterou hledáme.</param>
        /// <returns>List Id záznamu, na který uzly ukazují.</returns>
        public List<int> FindAll(T value)
        {

            Node? current = root;

            List<int> results = new();

            while (true)
            {

                if (current == null)
                    return results; // throw new Exception("Zaznam nebyl nalezen.");

                if ((Node)current == value)
                    while ((Node)current == value)
                    {
                        results.Add(current.RecordId);
                        current = current.Right;

                        if (current == null)
                            return results;
                    }

                else if (current > value)
                    current = current.Left;

                else
                    current = current.Right;

            }


        }

        /// <summary>
        /// Vloží nový uzel odkazující na záznam do stromu.
        /// </summary>
        /// <param name="value">Hodnota pro vyhledávání.</param>
        /// <param name="id">Id záznamu, na který má uzel ukazovat (jež koresponduje s hodnotou).</param>
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


        /// <summary>
        /// Vytvoří nový prázdný vyhledávací stromu typu T.
        /// </summary>
        public BST()
        {

        }

        /// <summary>
        /// Kořen stromu. Může být null, když je strom prázdný.
        /// </summary>
        Node? root = null;


    }
}
