using DataSure.Contracts.DatabaseServices;
using DataSure.Models.AdminModel;
using Microsoft.Data.Sqlite;
using System.Text;

namespace DataSure.Service.DatabaseServices
{
    public class SQLiteService : ISQLiteService
    {
        private readonly string _connectionString;

        //string dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");

        public SQLiteService(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }

        private string GetSQLiteType(DataTypeEnum dataType, int? length)
        {
            return dataType switch
            {
                DataTypeEnum.String => "TEXT",
                DataTypeEnum.Integer => "INTEGER",
                DataTypeEnum.Decimal => "REAL",
                DataTypeEnum.Boolean => "INTEGER",
                DataTypeEnum.DateTime => "TEXT",
                _ => "TEXT"
            };
        }

        public async Task CreateDynamicTableAsync(string tableName, List<PropertyConfigModel> properties)
        {
            var sb = new StringBuilder();

            // Drop table if it already exists
            sb.AppendLine($"DROP TABLE IF EXISTS {tableName};");

            // Create new table
            sb.Append($"CREATE TABLE IF NOT EXISTS {tableName} (");
            sb.Append("Id INTEGER PRIMARY KEY AUTOINCREMENT, ");

            foreach (var prop in properties)
            {
                string columnType = GetSQLiteType(prop.DataType, prop.LengthInChar);
                string required = prop.IsRequired ? "NOT NULL" : "";
                sb.Append($"{prop.Code} {columnType} {required}, ");
            }

            sb.Length -= 2; // Remove trailing comma
            sb.Append(");");

            string query = sb.ToString();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = query;
            await command.ExecuteNonQueryAsync();
        }

        //this needs to be removed, for testing only
        public async Task<List<string>> GetTableNamesAsync()
        {
            var tableNames = new List<string>();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tableNames.Add(reader.GetString(0));
            }

            return tableNames;
        }

    }
}