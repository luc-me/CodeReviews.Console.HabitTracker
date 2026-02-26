using Microsoft.Data.Sqlite;
using System.Globalization;


namespace HabitLogger
{
    class Program
    {
        static string connectionString = "Data Source=habits.db";
        static void Main(string[] args)
        {
            //create table
            using SqliteConnection connection = new(connectionString);
            
            connection.Open();
            var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = @"CREATE TABLE IF NOT EXISTS Habits (
            HabitId INTEGER PRIMARY KEY AUTOINCREMENT, 
            Name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS HabitRecords (
            RecordId INTEGER PRIMARY KEY AUTOINCREMENT, 
            Date DATE,
            Quantity INTEGER,
            HabitId INTEGER NOT NULL,
            FOREIGN KEY (HabitId) REFERENCES Habits(HabitId)
            ON DELETE CASCADE
            ON UPDATE CASCADE
            );";
            tableCommand.ExecuteNonQuery();

            connection.Close();

            Menu();
        }

        static void Menu()
        {
            Console.Clear();
            bool endApp = false;
            while (!endApp)
            {
                Console.WriteLine(@"
------------Main Menu------------
        0 - Quit Program
        1 - Add a habit
        2 - View all records
        3 - Insert record
        4 - Delete habit
        5 - Delete record
        6 - Update habit
        7 - Update record
                ");
                string? op = Console.ReadLine();
                switch (op)
                {
                    case "0":
                        endApp = true;
                        break;
                    case "1":
                        AddHabit();
                        break;
                    case "2":
                        ViewAll();
                        break;
                    case "3":
                        Insert();
                        break;
                    case "4":
                        DeleteHabit();
                        break;
                    case "5":
                        Delete();
                        break;
                    case "6":
                        UpdateHabit();
                        break;
                    case "7":
                        Update();
                        break;
                    default:
                        Console.WriteLine("That's not a valid option");
                        break;
                }
            }
            
        }

        private static void DeleteHabit()
        {
            ViewAll();
            
            bool validID = false;
            int habitId = 0;
            using SqliteConnection connection = new(connectionString);
            connection.Open();
            while (!validID)
            {
                string nameHabit = GetString("Insert the name of the habit you want to delete or 0 to return to main menu");
                var checkID = connection.CreateCommand();
                checkID.CommandText = $"SELECT HabitId FROM Habits WHERE Name=@Name";
                checkID.Parameters.Add("@Name", SqliteType.Text, 100).Value = nameHabit;
                var id = checkID.ExecuteScalar();
                if (id == null)
                    Console.WriteLine("That habit doesn't exist");
                else
                {
                    habitId = Convert.ToInt32(id);
                    validID = true;
                }
            }
          
            var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = $"DELETE FROM Habits WHERE HabitId = @HabitId";
            tableCommand.Parameters.Add("@HabitId", SqliteType.Integer, 100).Value = habitId;

            tableCommand.ExecuteNonQuery();
            connection.Close();
        }

        private static void AddHabit()
        {
            bool validID = false;
            int habitId = 0;
            string nameHabit="";
            using SqliteConnection connection = new(connectionString);
            connection.Open();
            while (!validID)
            {
                nameHabit = GetString("Insert the name of the habit you want to add or 0 to return to main menu");

                var checkID = connection.CreateCommand();
                checkID.CommandText = $"SELECT HabitId FROM Habits WHERE Name=@Name";
                checkID.Parameters.Add("@Name", SqliteType.Text, 100).Value = nameHabit;

                var id = checkID.ExecuteScalar();
                if (id != null)
                    Console.WriteLine("That habit already exists");
                else
                {
                    habitId = Convert.ToInt32(id);
                    validID = true;
                }
            }
            
            var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = $"INSERT INTO Habits (Name) VALUES (@Name)";
            tableCommand.Parameters.Add("@Name", SqliteType.Text, 100).Value = nameHabit;
            tableCommand.ExecuteNonQuery();

            connection.Close();
        }

        private static void ViewAll()
        {
            using SqliteConnection connection = new(connectionString);

            connection.Open();
            var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = @"SELECT 
                Habits.HabitId,
	            Habits.Name,
	            HabitRecords.RecordId,
	            HabitRecords.Date,
	            HabitRecords.Quantity
            FROM Habits
            LEFT JOIN HabitRecords
            On Habits.HabitId = HabitRecords.HabitId
            ORDER BY HabitRecords.RecordId
            ";
            SqliteDataReader reader = tableCommand.ExecuteReader();

            List<Habit> habits = new();
            List<Record> records = new();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    if (!habits.Any(Habit => Habit.HabitId == reader.GetInt32(0)))
                    {
                        habits.Add(new Habit
                        {
                            HabitId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                        });
                    }
                    
                    if (!reader.IsDBNull(2))
                    {
                        records.Add(new Record
                        {
                            HabitId = reader.GetInt32(0),
                            RecordId = reader.GetInt32(2),
                            Date = DateTime.ParseExact(reader.GetString(3), "dd-MM-yy", new CultureInfo("en-US")),
                            Quantity = reader.GetInt32(4)
                        });
                    }
                }
            }
            else
                Console.WriteLine("No rows found");

            connection.Close();
            foreach (Habit h in habits)
            {
                Console.WriteLine($"\n- {h.Name}");
                foreach (Record r in records)
                {
                    if (r.HabitId == h.HabitId)
                        Console.WriteLine($"\tRecord Id:{r.RecordId} - Date:{r.Date.ToString("dd-MM-yy")} - Quantity:{r.Quantity}");
                }
            }
        }
        
        private static void Insert()
        {
            bool validID = false;
            int habitId = 0;
            using SqliteConnection connection = new(connectionString);
            connection.Open();

            while (!validID)
            {
                string nameHabit = GetString("Insert the name of the habit you want to insert a record or 0 to return to main menu");
                var checkID = connection.CreateCommand();
                checkID.CommandText = $"SELECT HabitId FROM Habits WHERE Name=@Name";
                checkID.Parameters.Add("@Name", SqliteType.Text, 100).Value = nameHabit;
                var id = checkID.ExecuteScalar();
                if (id != null)
                {
                    validID = true;
                    habitId=Convert.ToInt32(id);
                }
                    
                else
                    Console.WriteLine("That habit doesn't exist");
            }
            
            string date = GetDate("Insert a valid date (dd-mm-yy) or 0 to return to main menu");
            int quantity = GetNumber("Insert the measure of your choice or 0 to return to main menu");
            
            var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = $"INSERT INTO HabitRecords (Date,Quantity,HabitId) VALUES (@Date,@Quantity,@HabitId)";
            tableCommand.Parameters.Add("@Date", SqliteType.Text, 64).Value = date;
            tableCommand.Parameters.Add("@Quantity", SqliteType.Integer, 64).Value = quantity;
            tableCommand.Parameters.Add("@HabitId", SqliteType.Integer, 64).Value = habitId;

            tableCommand.ExecuteNonQuery();
            connection.Close();
        }

        private static void Delete()
        {
            ViewAll();
            bool validID = false;
            while (!validID)
            {
                int id = GetNumber("Insert the Id of the record you want to delete or 0 to return to main menu");
                using SqliteConnection connection = new(connectionString);
                connection.Open();

                var tableCommand = connection.CreateCommand();
                tableCommand.CommandText = $"DELETE FROM HabitRecords WHERE RecordId = @RecordId";
                tableCommand.Parameters.Add("@RecordId", SqliteType.Integer, 64).Value = id;

                int rowNumber = tableCommand.ExecuteNonQuery();
                if (rowNumber == 0)
                    Console.WriteLine($"The record with Id: {id} doesn't exist");
                else
                    validID = true;
            }
            
        }
        private static void UpdateHabit()
        {
            ViewAll();
            bool validID = false;
            int habitId=0;
            string name;
            using SqliteConnection connection = new(connectionString);
            connection.Open();
            while (!validID)
            {
                name = GetString("Insert the name of the habit you want to update or 0 to return to main menu");

                var checkID = connection.CreateCommand();
                checkID.CommandText = $"SELECT HabitId FROM Habits WHERE Name = @Name";
                checkID.Parameters.Add("@Name", SqliteType.Text).Value = name;

                var id = checkID.ExecuteScalar();
                if (id == null)
                    Console.WriteLine($"The habit {name} doesn't exist");
                else
                {
                    habitId = Convert.ToInt32(id);
                    validID = true;
                }

            }
            name = GetString("Insert the name of the habit you want to add");

            var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = $"UPDATE Habits SET Name =@Name WHERE HabitId = @HabitId";
            tableCommand.Parameters.Add("@Name", SqliteType.Text, 100).Value = name;
            tableCommand.Parameters.Add("@HabitId", SqliteType.Integer, 64).Value = habitId;

            tableCommand.ExecuteNonQuery();
            connection.Close();
        }
        private static void Update()
        {
            ViewAll();
            bool validID = false;
            int recordId=0;
            using SqliteConnection connection = new(connectionString);
            connection.Open();
            while (!validID)
            {
                int id = GetNumber("Insert the Id of the record you want to update or 0 to return to main menu");

                var checkID = connection.CreateCommand();
                checkID.CommandText = $"SELECT RecordId FROM HabitRecords WHERE RecordId = @RecordId";
                checkID.Parameters.Add("@RecordId", SqliteType.Integer, 64).Value = id;

                var tempId = checkID.ExecuteScalar();
                if (tempId == null)
                    Console.WriteLine($"The record with Id: {id} doesn't exist");
                else
                {
                    validID = true;
                    recordId = id;
                }
            }
            
            string date = GetString("Insert a valid date (dd-mm-yy) or 0 to return to main menu");
            int quantity = GetNumber("Insert the number of glasses or other measure of your choice or 0 to return to main menu");
            
            var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = $"UPDATE HabitRecords SET Date = @Date, Quantity = @Quantity WHERE RecordId = @RecordId";
            tableCommand.Parameters.Add("@Date", SqliteType.Text, 64).Value = date;
            tableCommand.Parameters.Add("@Quantity", SqliteType.Integer, 64).Value = quantity;
            tableCommand.Parameters.Add("@RecordId", SqliteType.Integer, 64).Value = recordId;

            tableCommand.ExecuteNonQuery();
            connection.Close();
        }
        static string GetString(string message)
        {
            string? input = null;
            while (input == null)
            {
                Console.WriteLine(message);
                input = Console.ReadLine();
            }
            if (input == "0")
            {
                using SqliteConnection connection = new(connectionString);
                connection.Close();
                Menu();
            }

            return input;
        }
        static string GetDate(string message)
        {
            bool isValid = false;
            bool validFormat = false;
            DateTime date=DateTime.Now;
            while (!isValid || !validFormat)
            {
                string input = GetString(message);
                validFormat = DateTime.TryParseExact(input, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out date);
                if (date <= DateTime.Today)
                    isValid = true;
                if (!isValid || !validFormat)
                    Console.WriteLine("The date isn't valid");
            }
            

            return date.ToString("dd-MM-yy", new CultureInfo("en-US"));
        }

        static int GetNumber(string message)
        {
            string? input = null;
            int number = 0;
            bool valid = false;
            while (input == null || !valid)
            {
                Console.WriteLine(message);
                input = Console.ReadLine();
                valid = Int32.TryParse(input, out number);
            }
            if (input == "0")
            {
                using SqliteConnection connection = new(connectionString);
                connection.Close();
                Menu();
            }
            return number;
        }
       

        class Habit
        {
            internal int HabitId { get; set; }
            internal string? Name { get; set; }
           
        }
        class Record
        {
            internal int? RecordId { get; set; }
            internal int? HabitId { get; set; }
            internal DateTime Date { get; set; }
            internal int? Quantity { get; set; }
        }
    }
}
