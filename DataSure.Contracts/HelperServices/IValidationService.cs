using DataSure.Models.AdminModel;

namespace DataSure.Contracts.HelperServices
{
    public interface IValidationService
    {
        Task<List<string>?> VerifyImportFileColumnsAsync(Stream stream, List<PropertyConfigModel> properties, string ingestedFileName);
    }
}
