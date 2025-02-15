using DataSure.Models.AdminModel;
using System.Data;

namespace DataSure.Contracts.HelperServices
{
    public interface IValidationService
    {
        Task<List<string>?> VerifyImportedHeadersAsync(DataTable dataTable, List<PropertyConfigModel> propertyConfigs);
    }
}
