using Microsoft.AspNetCore.Components.Forms;
using System.Data;

namespace DataSure.Contracts.HelperServices
{
    public interface IFileOperationService
    {
        Task<DataTable> ConvertCsvToDataTableAsync(IBrowserFile csvFile);
    }
}
