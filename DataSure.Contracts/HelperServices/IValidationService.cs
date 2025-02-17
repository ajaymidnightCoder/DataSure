using DataSure.Models.AdminModel;
using System.Data;

namespace DataSure.Contracts.HelperServices
{
    public interface IValidationService
    {
        Task<List<string>?> ValidateImportedHeadersAsync(DataTable dataTable, List<PropertyConfigModel> propertyConfigs);
        Task ValidateDataTableAsync(DataTable dataTable, List<PropertyConfigModel> propertyConfigs);
    }
}
