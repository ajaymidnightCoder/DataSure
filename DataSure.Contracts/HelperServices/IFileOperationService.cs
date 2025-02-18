using Microsoft.AspNetCore.Components.Forms;
using System.Data;

namespace DataSure.Contracts.HelperServices
{
    public interface IFileOperationService
    {
        Task<DataTable> ConvertCsvToDataTableAsync(IBrowserFile csvFile);
        string ConvertDataTableToCsv(DataTable dataTable);
        Task DownloadFileAsync(string fileName, MemoryStream stream);
        Task DownloadFileAsync(string fileName, MemoryStream stream, string? toastNotificationMessage = null);
    }
}
