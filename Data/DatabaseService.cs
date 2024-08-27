using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace DashMaster.Data
{
    public class DatabaseService
    {
        private readonly string _connectionString = "Data Source=applications.db;";

        public void CreateDatabase()
        {
            using (SqliteConnection connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string createTableQuery = @"CREATE TABLE IF NOT EXISTS Applications (
                                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                Name TEXT NOT NULL,
                                                Icon TEXT,
                                                Path TEXT NOT NULL
                                            );";
                SqliteCommand command = new SqliteCommand(createTableQuery, connection);
                command.ExecuteNonQuery();
            }
        }

        // Add other database-related methods here, such as SaveApplication, LoadApplications, etc.
    }
}
