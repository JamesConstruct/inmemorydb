using System.IO;
using InMemoryDB;

namespace InMemoryDB
{

    /// <summary>
    /// Program pro demonstraci databáze.
    /// </summary>
    public class Program
    {

        /// <summary>
        /// Hlavní funkce programu.
        /// </summary>
        public static void Main()
        {

            var db = new InMemoryDB.Db();

            db.AddColumn<int>("Id");
            db.AddColumn<string>("Name");
            db.AddColumn<double>("Balance");
            db.MakeIndex<string>("Name");

            db.Insert(2, "Jimmi", 3.45);
            db.Insert(5, "James", 1.23);
            db.Insert(7, "Emily", 0.55);
            db.Insert(12, "Rodrigez", 2.4);
            db.Insert(13, "Alpharomeo", 1.02);
            db.Insert(1, "Ziki", 1.0);

            dynamic r = db.SelectWhere("Name", "Ziki");

            // System.Console.WriteLine(((InMemoryDB.Db.Field<double>)r.Fields.ElementAt(2)).Value);
            System.Console.WriteLine(r.Name);
            System.Console.WriteLine(r.Balance.GetType());

            System.Console.WriteLine(db.GetSum<double>("Balance"));

            System.Console.WriteLine(db.Count);

        }

    }

}