﻿using DataSure.Contracts.HelperServices;
using Microsoft.AspNetCore.Components.Forms;
using System.Data;
using System.Text;

namespace DataSure.Service.HelperServices
{
    public class FileOperationService 
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

        public string ConvertDataTableToCsv(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return string.Empty; // No data to export

            var csvBuilder = new StringBuilder();

            // 1. Write column headers
            var columnNames = dataTable.Columns.Cast<DataColumn>().Select(col => EscapeCsvValue(col.ColumnName));
            csvBuilder.AppendLine(string.Join(",", columnNames));

            // 2. Write row data
            foreach (DataRow row in dataTable.Rows)
            {
                var rowValues = dataTable.Columns.Cast<DataColumn>()
                    .Select(col => EscapeCsvValue(row[col]?.ToString()));
                csvBuilder.AppendLine(string.Join(",", rowValues));
            }

            return csvBuilder.ToString(); // Return CSV as a string
        }

        // 3. Helper to Escape Special CSV Characters
        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\""; // Return empty quotes for blank values

            // If value contains a comma, quote, or newline, wrap in double quotes
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\""); // Escape double quotes
                return $"\"{value}\"";
            }

            return value;
        }

    }
}
