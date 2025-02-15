using Microsoft.AspNetCore.Components.Forms;
using System.Data;

namespace DataSure.Contracts.HelperServices
{
    public interface IFileOperationService
    {
        Task<List<string>> GetCsvHeadersAsync(Stream stream);
        //Task<List<string>> GetExcelHeadersAsync(Stream stream);
        Task<DataTable> ConvertToDataTableAsync(Stream stream, string fileExtension);
        Task<DataTable> ConvertCsvToDataTableAsync(IBrowserFile csvFile);
    }
}
