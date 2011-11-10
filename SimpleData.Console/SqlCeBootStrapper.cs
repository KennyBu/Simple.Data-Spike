using System;
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
    }
}