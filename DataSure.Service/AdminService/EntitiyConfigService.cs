using DataSure.Contracts.AdminService;
using DataSure.Models.AdminModel;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataSure.Service.AdminService
{
    public class EntitiyConfigService : IEntitiyConfigService
    {

        private string RawFilePath { get; set; }

        public EntitiyConfigService()
        {
            RawFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources\\Raw\\{0}");
        }

        private static async Task<string> GetRawFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return default;
            }
            string content = await File.ReadAllTextAsync(filePath);
            return content;
        }

        public async Task<List<T>> GetListByFileName<T>(string fileName)
        {
            string filePath = string.Format(RawFilePath, fileName);
            var content = await GetRawFile(filePath);

            var list = new List<T>();
            if (!string.IsNullOrWhiteSpace(content))
            {
                JsonSerializerOptions options = new()
                {
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };
                list = JsonSerializer.Deserialize<List<T>>(content, options);
            }
            return list;
        }

        public async Task SaveRawFile(string fileName, string content)
        {
            string filePath = string.Format(RawFilePath, fileName);
            await File.WriteAllTextAsync(filePath, content);
        }

        public bool CreatRawFile(string fileName)
        {
            string filePath = string.Format(RawFilePath, fileName);
            FileStream fs = File.Create(filePath);
            fs.Close();
            return File.Exists(filePath);
        }

        private static string GenerateCreateTableSql(string tableName, List<PropertyConfigModel> properties)
        {
            var columns = new List<string>();

            foreach (var property in properties)
            {
                string columnType = property.DataType switch
                {
                    DataTypeEnum.String => $"VARCHAR({property.LengthInChar ?? 255})",
                    DataTypeEnum.Integer => "INTEGER",
                    //DataTypeEnum. => "BOOLEAN",
                    // Add other data types as needed
                    _ => throw new NotImplementedException($"Data type {property.DataType} not supported.")
                };

                string columnDefinition = $"{property.Code} {columnType} {(property.IsRequired ? "NOT NULL" : "NULL")}";
                columns.Add(columnDefinition);
            }

            string columnsSql = string.Join(", ", columns);
            return $"CREATE TABLE {tableName} ({columnsSql});";
        }

        private string GetSqlDataType(DataTypeEnum dataType, int? length)
        {
            return dataType switch
            {
                DataTypeEnum.String => length.HasValue ? $"VARCHAR({length.Value})" : "TEXT",
                DataTypeEnum.Integer => "INTEGER",
                // Add other cases as needed
                _ => "TEXT",
            };
        }

        public async Task SaveSubEntityProperties(List<PropertyConfigModel> propertyList, EntityConfigModel entityConfigModel)
        {
            JsonSerializerOptions options = new()
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            var content = JsonSerializer.Serialize<List<PropertyConfigModel>>(propertyList, options);
            await SaveRawFile(entityConfigModel.FileName, content);

            GenerateCreateTableSql(entityConfigModel.TableName, propertyList);

        }

        private async Task BulkInsertDataAsync(string tableName, DataTable dataTable)
        {
            // Construct the SQL INSERT statement for bulk insert
            foreach (DataRow row in dataTable.Rows)
            {
                var columnNames = string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                var columnValues = string.Join(", ", row.ItemArray.Select(val => $"'{val}'"));

                var insertCommand = $"INSERT INTO {tableName} ({columnNames}) VALUES ({columnValues})";
                //await _context.Database.ExecuteSqlRawAsync(insertCommand);
            }
        }

    }
}