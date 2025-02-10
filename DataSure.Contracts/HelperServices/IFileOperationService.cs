using System.Data;

namespace DataSure.Contracts.HelperServices
{
    public interface IFileOperationService
    {
        Task<List<string>> GetCsvHeadersAsync(Stream stream);
        Task<List<string>> GetExcelHeadersAsync(Stream stream);
        DataTable ConvertToDataTable(Stream stream, string fileExtension);
    }
}
