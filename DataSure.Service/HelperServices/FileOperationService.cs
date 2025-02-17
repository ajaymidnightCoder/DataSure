using DataSure.Contracts.HelperServices;
using Microsoft.AspNetCore.Components.Forms;
using System.Data;

namespace DataSure.Service.HelperServices
{
    public class FileOperationService : IFileOperationService
    {

        public async Task<List<string>> GetCsvHeadersAsync(Stream stream)
        {
            using var reader = new StreamReader(stream);
            var headerLine = await reader.ReadLineAsync();
            return headerLine?.Split(',').Select(h => h.Trim()).ToList() ?? new List<string>();
        }

        public async Task<DataTable> ConvertCsvToDataTableAsync(IBrowserFile csvFile)
        {
            var dataTable = new DataTable();
            try
            {

                using var stream = csvFile.OpenReadStream();
                using var reader = new StreamReader(stream);
                var headerLine = await reader.ReadLineAsync();

                var headers = headerLine?.Split(',').Select(h => h.Trim()).ToList() ?? [];

                if (headers == null) 
                    throw new Exception("CSV file is empty or has invalid headers.");

                headers.ForEach(header => dataTable.Columns.Add(header));

                string rowLine;
                while ((rowLine = await reader.ReadLineAsync()) != null)  // No EndOfStream check
                {
                    if (string.IsNullOrWhiteSpace(rowLine))
                        continue; // Skip empty lines

                    var rows = rowLine.Split(',').Select(cell => cell.Trim()).ToArray();
                    dataTable.Rows.Add(rows);
                }

            }
            catch (Exception ex)
            {
                // Handle exception (log it, rethrow it, etc.)
            }
            return dataTable;
        }


    }
}
