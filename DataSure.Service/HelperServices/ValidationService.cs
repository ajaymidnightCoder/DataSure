using DataSure.Contracts.HelperServices;
using DataSure.Models.AdminModel;
using DataSure.Models.Enum;
using System.Data;

namespace DataSure.Service.HelperServices
{
    public class ValidationService(IFileOperationService fileOperationService) : IValidationService
    {

        public async Task<List<string>?> VerifyImportFileColumnsAsync(Stream stream, List<PropertyConfigModel> propertyConfigs, string ingestedFileName)
        {
            var validationErrors = new List<string>();
            List<string> fileHeaders;

            var fileExtension = Path.GetExtension(ingestedFileName).ToLower();
            fileHeaders = fileExtension switch
            {
                ".csv" => await fileOperationService.GetCsvHeadersAsync(stream),
                ".xlsx" => await fileOperationService.GetExcelHeadersAsync(stream),
                _ => validationErrors.Append($"{fileExtension} not supported!").ToList()
            };

            if (validationErrors.Any()) return validationErrors;

            var expectedColumns = propertyConfigs.Select(p => p.Code).ToList();
            var mismatchedColumns = expectedColumns.Except(fileHeaders).ToList();
            if (mismatchedColumns.Any())
            {
                validationErrors.Add($"Columns {string.Join(", ", mismatchedColumns)} not selected for Entity.");
            }
            else if (fileHeaders.Count != expectedColumns.Count)
            {
                validationErrors.Add("The columns in the file do not match the configured columns for the selected entity.");
            }

            return validationErrors.Any() ? validationErrors : null;
        }
        
        public async Task ValidateImportedProperties(DataTable dataTable, List<PropertyConfigModel> propertyConfigs)
        {
            // Add error-related columns to the DataTable if they don’t exist
            if (!dataTable.Columns.Contains("HasError"))
                dataTable.Columns.Add("HasError", typeof(bool));

            if (!dataTable.Columns.Contains("Error"))
                dataTable.Columns.Add("Error", typeof(string));

            // Parallel processing for rows
            Parallel.ForEach(dataTable.Rows.Cast<DataRow>(), row =>
            {
                bool hasError = false;
                var errorMessages = new List<string>();

                foreach (var propertyConfig in propertyConfigs)
                {
                    // Check if the column is missing
                    if (!dataTable.Columns.Contains(propertyConfig.Code))
                    {
                        hasError = true;
                        errorMessages.Add($"Column '{propertyConfig.Code}' is missing in the imported data.");
                        break; // Stop further checks for this row if critical column is missing
                    }

                    var cellValue = row[propertyConfig.Code]?.ToString();

                    // Check if required field is missing
                    if (propertyConfig.IsRequired && string.IsNullOrWhiteSpace(cellValue))
                    {
                        hasError = true;
                        errorMessages.Add($"Column '{propertyConfig.Code}' is required but missing in some rows.");
                        continue; // Move to next property check for this row
                    }

                    // Validate data type
                    if (!string.IsNullOrWhiteSpace(cellValue) && !IsValidDataType(cellValue, propertyConfig.DataType))
                    {
                        hasError = true;
                        errorMessages.Add($"Column '{propertyConfig.Code}' contains an invalid data type in some rows.");
                        continue;
                    }

                    // Validate length
                    if (propertyConfig.LengthInChar.HasValue && cellValue?.Length > propertyConfig.LengthInChar.Value)
                    {
                        hasError = true;
                        errorMessages.Add($"Column '{propertyConfig.Code}' exceeds the maximum length of {propertyConfig.LengthInChar.Value} characters.");
                    }
                }

                // Update row with error information if any errors were found
                if (hasError)
                {
                    row["HasError"] = true;
                    row["Error"] = string.Join("; ", errorMessages);
                }
                else
                {
                    row["HasError"] = false;
                    row["Error"] = string.Empty; // Clear error message if no errors are found
                }
            });

            // Optional: Output or log errors for inspection
            var errorRows = dataTable.AsEnumerable().Where(r => r.Field<bool>("HasError"));
            foreach (var errorRow in errorRows)
            {
                Console.WriteLine($"Row error: {errorRow["Error"]}"); // Replace with actual logging
            }
        }

        private bool IsValidDataType(string value, DataTypeEnum dataType)
        {
            return dataType switch
            {
                DataTypeEnum.String => true,  // Any value is valid for strings
                DataTypeEnum.Int => int.TryParse(value, out _),
                DataTypeEnum.DateTime => DateTime.TryParse(value, out _),
                //DataTypeEnum.Decimal => decimal.TryParse(value, out _),
                //DataTypeEnum => bool.TryParse(value, out _),
                _ => true
            };
        }


    }
}
