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

            db.Insert(2, "Jimmi");
            db.Insert(5, "James");
            db.Insert(7, "Emily");

            db.SelectWhere<string>("Name", "Jimmi");

            System.Console.WriteLine(db.Count);
        }

    }

}