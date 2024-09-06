using InMemoryDB;
using System.Diagnostics;
using Xunit.Abstractions;


namespace TestInMemoryDB
{
    public class DBTests
    {

        private readonly ITestOutputHelper output;

        public DBTests(ITestOutputHelper output)
        {
            this.output = output;
        }


        [Fact]
        public void TestColumns()
        {
            var db = new Db();
            Assert.Equal(0, db.Count); // empty db
            Assert.Equal(0, db.ColumnCount); // no collumns

            db.AddColumn<int>("FirstCol");
            db.AddColumn<string>("SecondCol");

            Assert.Equal(2, db.ColumnCount);
        }

        [Fact]
        public void TestAddingIntEntries()
        {
            var db = new Db();
            db.AddColumn<int>("id");

            for (int i = 0; i < 10; i++)
            {
                db.Insert(i);
            }

            Assert.Equal(10, db.Count);
            for (int i = 0; i < 10; i++)
            {
                Assert.Equal(i, ((dynamic)db[i]).id);
            }
        }

        [Fact]
        public void TestBSTEfficiency()
        {
            var db = new Db();
            db.AddColumn<string>("id");
            for (int i = 0; i < 10000; i++)
            {
                db.Insert(i.ToString());
            }

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            // Call the method you want to measure
            for (int i = 0; i < 100; i++)
            {
                // Call the method you want to measure
                db.SelectAllWhere("id", "9999");
            }

            stopwatch.Stop();

            TimeSpan elapsedNoIndex = stopwatch.Elapsed;

            stopwatch.Reset();

            db.Drop();
            db.AddColumn<string>("id");
            db.MakeIndex<string>("id");
            for (int i = 0; i < 9999; i++)
            {
                db.Insert(i.ToString());
            }

            stopwatch.Start();

            for (int i = 0; i < 100; i++)
            {
                // Call the method you want to measure
                db.SelectAllWhere("id", "9999");
            }

            stopwatch.Stop();

            TimeSpan elapsedIndex = stopwatch.Elapsed;

            output.WriteLine("Elapsed without index: " + elapsedNoIndex.TotalMilliseconds.ToString() + " ms");
            output.WriteLine("Elapsed with index: " + elapsedIndex.TotalMilliseconds.ToString() + " ms");

            Assert.True(elapsedIndex < elapsedNoIndex);

        }

        [Fact]
        public void TestSelect()
        {
            var db = new Db();
            db.AddColumn<int>("id");
            db.AddColumn<string>("name");
            db.AddColumn<bool>("iscool");

            db.Insert(0, "abraham", false);
            db.Insert(1, "lincoln", true);
            db.Insert(2, "john", true);

            dynamic r = db.SelectOneWhere("id", 0);
            Assert.Equal("abraham", r.name);

            r = db.SelectOneWhere("name", "lincoln");
            Assert.Equal(1, r.id);

            Assert.Equal(1, db.SelectAllWhere("id", 0).Count);
            Assert.Equal(1, db.SelectAllWhere("name", "lincoln").Count);
            Assert.Equal(2, db.SelectAllWhere("iscool", true).Count);

            r = db.SelectAllWhere("iscool", true);
            Assert.Equal(1, r[0].id);


        }

        [Fact]
        public void TestFilters()
        {

            var db = new Db();

            db.AddColumn<int>("Id");
            db.AddColumn<string>("Sender");
            db.AddColumn<string>("Receiver");
            db.AddColumn<int>("Amount");
            db.AddColumn<bool>("Verified");
            db.MakeIndex<int>("Id");

            db.Insert(0, "John", "Jimmi", 10, true);
            db.Insert(1, "Jimmi", "Ian", 6, true);
            db.Insert(2, "Jimmi", "John", 15, true);
            db.Insert(3, "Unknown", "Jimmi", 1000, false);
            db.Insert(4, "Ian", "Unknown", 1, false);

            // all payments where the receiver sent at least one verified payment with value over 12
            var verifiedPeopleOver12 = db[((dynamic)db).Verified & (((dynamic)db).Amount > 12)].Sender;
            //output.WriteLine(verifiedPeopleOver12.ToString());
            Func<string, bool> IsVerified = x => verifiedPeopleOver12.Contents.Contains(x);

            var filter = ((dynamic)db).Receiver.Transform(IsVerified) == true;
            var result = db[filter];

            output.WriteLine(result.ToString());

            Assert.Equal(2, result.Count);
            Assert.Equal("John", result[0].Sender);
            Assert.Equal("Unknown", result[1].Sender);

        }

        [Fact]
        public void TestDBToString()
        {
            var db = new Db();

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

            var expected = "|------------|------------|------------|------------|------------|\n| Id         | Sender     | Receiver   | Amount     | Verified   |\n|============|============|============|============|============|\n| 0          | John       | Jimmi      | 10         | True       |\n|------------|------------|------------|------------|------------|\n| 1          | Jimmi      | Ian        | 6          | True       |\n|------------|------------|------------|------------|------------|\n| 2          | Jimmi      | John       | 15         | True       |\n|------------|------------|------------|------------|------------|\n| 3          | Unknown    | Jimmi      | 10000      | False      |\n|------------|------------|------------|------------|------------|\n| 4          | Ian        | Unknown    | 1          | False      |\n|------------|------------|------------|------------|------------|\n";

            Assert.Equal(expected, db.ToString());


        }

        [Fact]
        public void TestEnumeration()
        {
            List<int> list = new();
            var db = new Db();
            db.AddColumn<int>("num");
            var rand = new Random();

            for (int i = 0; i < 1000; i++)
            {
                int n = rand.Next(1, 1000);
                list.Add(n);
                db.Insert(n);
            }

            int index = 0;
            foreach (var r in db)
            {
                Assert.Equal(list[index], ((dynamic)r).num);
                index++;
            }

        }

        [Fact]
        public void TestDrop()
        {
            var db = new Db();
            db.AddColumn<int>("Test");
            db.AddColumn<string>("A thing");
            db.Insert(2, "test");

            db.Drop();

            Assert.Equal(0, db.Count);
            Assert.Equal(0, db.ColumnCount);
        }

        [Fact]
        public void TestInvalidColumnAddition()
        {
            var db = new Db();
            db.AddColumn<int>("num");
            db.Insert(0);

            Action act = () => db.AddColumn<string>("Name");
            Exception exception = Assert.Throws<Exception>(act);
            Assert.Equal("Database is not empty!", exception.Message);
        }

        [Fact]
        public void TestInvalidSelectOne()
        {
            var db = new Db();
            db.AddColumn<int>("num");
            db.Insert(0);

            Action act = () => db.SelectOneWhere("invalid", 12);
            Exception exception = Assert.Throws<Exception>(act);
            Assert.Equal("Invalid column!", exception.Message);

            act = () => db.SelectOneWhere("num", 12);
            exception = Assert.Throws<Exception>(act);
            Assert.Equal("Not found!", exception.Message);

        }

        [Fact]
        public void TestInvalidSelectAll()
        {
            var db = new Db();
            db.AddColumn<int>("num");
            db.Insert(0);

            Action act = () => db.SelectAllWhere("invalid", 12);
            Exception exception = Assert.Throws<Exception>(act);
            Assert.Equal("Invalid column!", exception.Message);

            dynamic r = db.SelectAllWhere("num", 12);
            Assert.Equal(0, r.Count);

        }

        [Fact]
        public void TestInvalidInsert()
        {
            var db = new Db();
            db.AddColumn<int>("num");

            Action act = () => db.Insert("not a number");
            Exception exception = Assert.Throws<Exception>(act);
            Assert.Equal("Field type mismatch at index 0", exception.Message);

        }


    }
}