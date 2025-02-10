using DataSure.Models.AdminModel;

namespace DataSure.Contracts.HelperServices
{
    public interface IImportService
    {
        Task IngestToDB(EntityConfigModel entityConfig, Stream inputStream, string ingestedFileName);
    }
}
