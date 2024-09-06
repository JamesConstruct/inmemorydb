namespace InMemoryDB
{

    /// <summary>
    /// Program for demonstration of the database class.
    /// </summary>
    public class Program
    {

        /// <summary>
        /// The main showcase function.
        /// </summary>
        public static void Main()
        {

            // Simple table, demonstrating basic functionality

            Console.WriteLine("Simple table:\n*************");

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

            Console.WriteLine(db);

            // Select one row
            dynamic r = db.SelectOneWhere("Name", "Ziki");
            System.Console.WriteLine("Ziki, balance: " + r.Balance);
            System.Console.WriteLine("Type of the value: " + r.Balance.GetType());

            // Selection of multiple rows
            Db sub_table = db.SelectAllWhere("Name", "Emily");
            System.Console.WriteLine("Number of selected rows: " + sub_table.Count);
            System.Console.WriteLine("The sum of selected rows: " + sub_table.GetSum<double>("Balance"));

            // Simple calculations
            sub_table = sub_table.SelectAllWhere("Id", 44);
            System.Console.WriteLine("Number of selected rows: " + sub_table.Count);
            System.Console.WriteLine("Balance: " + ((dynamic)sub_table.First()).Balance);

            System.Console.WriteLine("Sum: " + db.GetSum<double>("Balance"));

            System.Console.WriteLine("Total count: " + db.Count);


            // More complicated table with queries

            db.Drop();

            Console.WriteLine("\n\nTransactions:\n*************");

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




            // Basic demonstration of the filters

            Console.WriteLine("\n\nFilters:\n*************");

            // select all verified payments with id 0, or select all payments from "Unknown"
            Console.WriteLine("Verified payments with ID 0 or payments from sender unknown:");
            var filter = (((dynamic)db).Id == 0 & ((dynamic)db).Verified) | ((dynamic)db).Sender == "Unknown";
            var result = db[filter];
            Console.WriteLine(result);

            // Select all payments, where the sender sent at least one verified payment
            Console.WriteLine("All payments, where the receiver sent at least one verified payment:");
            var verifiedPeople = db[((dynamic)db).Verified].Sender;
            Func<string, bool> IsVerified = x => verifiedPeople.Contents.Contains(x);

            var filter2 = ((dynamic)db).Receiver.Transform(IsVerified) == true;
            var result2 = db[filter2];

            // Console.WriteLine(verifiedPeople);
            Console.WriteLine(result2);

        }

    }

}