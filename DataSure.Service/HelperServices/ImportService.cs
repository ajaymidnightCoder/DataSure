using DataSure.Contracts.AdminService;
using DataSure.Contracts.HelperServices;
using DataSure.Models.AdminModel;

namespace DataSure.Service.HelperServices
{
    public class ImportService(IEntitiyConfigService entityConfigService, IValidationService validationService) : IImportService
    {

        //public async Task IngestToDB(EntityConfigModel entityConfig, Stream inputStream, string ingestedFileName)
        //{
        //    // Step 1: Load the configuration properties from file
        //    var propertyConfigs = await entityConfigService.GetListByFileName<PropertyConfigModel>(entityConfig.FileName);

        //    var validationErrors = await validationService.VerifyImportFileColumnsAsync(inputStream, propertyConfigs, ingestedFileName);

        //    if (validationErrors?.Any() == false)
        //    {
        //        //await InsertDataAsync(entityConfig.TableName, );
        //    }
        //}

        //public async Task InsertDataAsync(string tableName, List<Dictionary<string, string>> records)
        //{
        //    foreach (var record in records)
        //    {
        //        var columns = string.Join(", ", record.Keys);
        //        var parameters = string.Join(", ", record.Keys.Select(k => $"@{k}"));

        //        var insertSql = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

        //        //var sqlParameters = record.Select(kv => new SqlParameter($"@{kv.Key}", kv.Value ?? (object)DBNull.Value)).ToArray();

        //        //await dbContext.Database.ExecuteSqlRawAsync(insertSql, sqlParameters);
        //    }
        //}
        public Task IngestToDB(EntityConfigModel entityConfig, Stream inputStream, string ingestedFileName)
        {
            throw new NotImplementedException();
        }
    }
}