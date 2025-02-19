using DataSure.Contracts.DatabaseServices;
using DataSure.Models.AdminModel;
using Microsoft.Data.Sqlite;
using System.Text;

namespace DataSure.Service.DatabaseServices
{
    public class SQLiteService : ISQLiteService
    {
        private readonly string _connectionString;

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
                DataTypeEnum.Custom => "TEXT",
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

            string primaryKeyColumn = properties.FirstOrDefault(p => p.IsPrimaryKey)?.Code; // Get the primary key column
            var foreignKeys = new List<string>();  // To store foreign key constraints

            foreach (var prop in properties)
            {
                string columnType = GetSQLiteType(prop.DataType, prop.LengthInChar);
                string primaryKey = prop.Code == primaryKeyColumn ? "PRIMARY KEY" : "";
                string required = prop.IsRequired ? "NOT NULL" : "";
                string unique = prop.IsUnique ? "UNIQUE" : "";

                // Handle Foreign Key
                if (prop.IsForeignKey && !string.IsNullOrEmpty(prop.ForeignKeyReference))
                {
                    foreignKeys.Add($"FOREIGN KEY ({prop.Code}) REFERENCES {prop.ForeignKeyReference}({prop.Code}) ON DELETE CASCADE");
                }

                sb.Append($"{prop.Code} {columnType} {required} {primaryKey} {unique}, ");
            }

            // Append Foreign Key Constraints
            foreach (var fk in foreignKeys)
            {
                sb.Append($"{fk}, ");
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