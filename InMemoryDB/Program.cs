using System.IO;


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

            // Jednoduchá tabulka, výběr dle jména a součet

            var db = new InMemoryDB.Db();

            db.AddColumn<int>("Id");
            db.AddColumn<string>("Name");
            db.AddColumn<double>("Balance");
            db.MakeIndex<string>("Name");

            db.Insert(2, "Johhny", 3.45);
            db.Insert(5, "James", 1.23);
            db.Insert(7, "Emily", 0.55);
            db.Insert(12, "Rodrigez", 2.4);
            db.Insert(13, "Alpharomeo", 1.02);
            db.Insert(1, "Ziki", 1.0);
            db.Insert(44, "Emily", 100.00);

            dynamic r = db.SelectOneWhere("Name", "Ziki");
            System.Console.WriteLine("Ziki, balance: " + r.Balance);
            System.Console.WriteLine("Typ hodnoty: " + r.Balance.GetType());

            // Selekce více řádků
            Db sub_table = db.SelectAllWhere("Name", "Emily");
            System.Console.WriteLine("Počet vybraných: " + sub_table.Count);
            System.Console.WriteLine("Součet vybraných: " + sub_table.GetSum<double>("Balance"));


            sub_table = sub_table.SelectAllWhere("Id", 44);
            System.Console.WriteLine("Počet vybraných: " + sub_table.Count);
            System.Console.WriteLine("Balance: " + ((dynamic)sub_table.First()).Balance);

            System.Console.WriteLine("Celkový součet: " + db.GetSum<double>("Balance"));

            System.Console.WriteLine("Celkový počet: " + db.Count);


            // složitější tabulka

            db.Drop();

            db.AddColumn<int>("Id");
            db.AddColumn<string>("Sender");
            db.AddColumn<string>("Receiver");
            db.AddColumn<int>("Amount");
            db.AddColumn<bool>("Verified");
            db.MakeIndex<int>("Id");

            db.Insert(0, "John", "Jimmi", 10, true);
            db.Insert(1, "Jimmi", "Ian", 6, true);
            db.Insert(2, "Jimmi", "John", 15, true);
            db.Insert(3, "Unknown", "Jimmi", 10000, false);
            db.Insert(4, "Ian", "Unknown", 1, false);

            Console.WriteLine(db);

            int income = db.SelectAllWhere("Receiver", "Jimmi").SelectAllWhere("Verified", true).GetSum<int>("Amount");
            int outcome = db.SelectAllWhere("Sender", "Jimmi").SelectAllWhere("Verified", true).GetSum<int>("Amount");
            // int to_self = db.SelectAllWhere("Sender", "Jimmi").SelectAllWhere("Receiver", "Jimmi").SelectAllWhere("Verified", "true").GetSum<int>("Amount");

            Console.WriteLine("Jimmi's balance = " + (income - outcome));

            // demonstrace filtrů

            Console.WriteLine("Filters:");

            // vyber platby s id 0, jež jsou ověřené, anebo vyber platby které odesílá "Unknown"
            var filter = ( ((dynamic)db).Id == 0 & ((dynamic)db).Verified ) | ((dynamic)db).Sender == "Unknown";
            var result = db[filter];
            Console.WriteLine(result);

            // vyber všechny platby, kde příjemce odeslal alespoň jednu ověřenou platbu
            var verifiedPeople = db[((dynamic)db).Verified].Sender;
            Func<string, bool> IsVerified = x => verifiedPeople.Contents.Contains(x);

            var filter2 = ((dynamic)db).Receiver.TransformTo(IsVerified) == true;
            var result2 = db[filter2];

            // Console.WriteLine(verifiedPeople);
            Console.WriteLine(result2);

        }

    }

}