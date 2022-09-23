using System.IO;
using InMemoryDB;

namespace InMemoryDB
{
    public class Program
    {
        public static void Main()
        {

            var db = new InMemoryDB.Db();
            db.AddColumn<int>("Id");
            db.AddColumn<string>("Name");
            db.AddColumn<double>("Balance");

            db.Insert(2, "Jimmi", 3.45);
            db.Insert(5, "James", 1.23);
            db.Insert(7, "Emily", 0.55);

            dynamic r = db.SelectWhere("Name", "James");

            // System.Console.WriteLine(((InMemoryDB.Db.Field<double>)r.Fields.ElementAt(2)).Value);
            System.Console.WriteLine(r.Balance);
            System.Console.WriteLine(r.Balance.GetType());

            System.Console.WriteLine(db.GetSum<double>("Balance"));

            System.Console.WriteLine(db.Count);
        }

    }

}