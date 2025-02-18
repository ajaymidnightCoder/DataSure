using DataSure.Models.AdminModel;

namespace DataSure.Contracts.DatabaseServices
{
    public interface ISQLiteService
    {
        Task CreateDynamicTableAsync(string tableName, List<PropertyConfigModel> properties);
        Task<List<string>> GetTableNamesAsync();
    }

}
