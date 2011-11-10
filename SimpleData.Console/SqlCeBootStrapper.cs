using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Reflection;

namespace SimpleData.Console
{
    public class SqlCeBootStrapper
    {
        private string _connectionString;
        private string _DbFile;

        public SqlCeBootStrapper(string connectionName)
        {
            _connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            _DbFile = _connectionString.Substring(_connectionString.IndexOf("=") + 1);
        }

        public void CreateDBFile()
        {
            DeleteDBFile();
            var db = new SqlCeEngine(_connectionString);
            db.CreateDatabase();
        }

        public void DeleteDBFile()
        {
            var pathToDb = GetExecutingDirectory() + @"\" + _DbFile;

            if (File.Exists(pathToDb))
                File.Delete(pathToDb);
        }

        private static string GetExecutingDirectory()
        {
            var path = Assembly.GetExecutingAssembly().GetName().CodeBase;
            return Path.GetDirectoryName(new Uri(path).LocalPath);
        }

        public void CreateSchema()
        {
            var schemaCommands = new List<string> {
                @"CREATE TABLE Users(
                Id [uniqueidentifier] NOT NULL CONSTRAINT UsersNew_PK PRIMARY KEY,
                UserName nvarchar(100) NOT NULL,
                EmailAddress nvarchar(100) NOT NULL,
                FirstName nvarchar(100) NULL,
                LastName nvarchar(100) NULL)"
            };

            using (var connection = new SqlCeConnection(_connectionString))
            {
                connection.Open();
                
                foreach (var schemaCommand in schemaCommands)
                {
                    var command = new SqlCeCommand(schemaCommand, connection);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}