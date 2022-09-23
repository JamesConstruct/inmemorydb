using System.IO;

/// <summary>
/// Test123
/// </summary>
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
            db.Insert(44, "James", 100.00);

            dynamic r = db.SelectOneWhere("Name", "Ziki");

            // System.Console.WriteLine(((InMemoryDB.Db.Field<double>)r.Fields.ElementAt(2)).Value);
            System.Console.WriteLine(r.Name);
            System.Console.WriteLine(r.Balance.GetType());


            Db sub_table = db.SelectAllWhere("Name", "James");
            System.Console.WriteLine(sub_table.Count);
            System.Console.WriteLine(sub_table.GetSum<double>("Balance"));
            sub_table = sub_table.SelectAllWhere("Id", 5);
            System.Console.WriteLine(sub_table.Count);
            System.Console.WriteLine(((dynamic)sub_table.First()).Balance);

            System.Console.WriteLine(db.GetSum<double>("Balance"));

            System.Console.WriteLine(db.Count);

        }

    }

}