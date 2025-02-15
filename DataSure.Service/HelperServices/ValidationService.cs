using DataSure.Contracts.HelperServices;
using DataSure.Models.AdminModel;
using DataSure.Models.Enum;
using System.Data;
using System.Diagnostics;

namespace DataSure.Service.HelperServices
{
    public class ValidationService(IFileOperationService fileOperationService, INotificationService notificationService) : IValidationService
    {

        public async Task<List<string>?> VerifyImportedHeadersAsync(DataTable dataTable, List<PropertyConfigModel> propertyConfigs)
        {
            string notificationMsg = string.Empty;
            var validationErrorList = new List<string>();

            var csvHeaders = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

            var expectedColumns = propertyConfigs.Select(p => p.Code).ToList();
            var requiredColumns = propertyConfigs.Where(x => x.IsRequired).Select(p => p.Code).ToList();

            // Check if all required columns are present in csvHeaders
            var missingRequiredColumns = requiredColumns.Except(csvHeaders).ToList();
            if (missingRequiredColumns.Count != 0)
            {
                notificationMsg = $"Missing required columns: {string.Join(",", missingRequiredColumns)}";
                validationErrorList.Add(notificationMsg);
                notificationService.AddValidationMessage(notificationMsg);
            }
            else
            {
                notificationMsg = "All required columns are present.";
            }

            // Check if there are any extra columns in csvHeaders that are not in expectedColumns
            var extraColumns = csvHeaders.Except(expectedColumns).ToList();
            if (extraColumns.Count != 0)
            {
                notificationMsg = "Extra columns present: " + string.Join(", ", extraColumns);
                validationErrorList.Add(notificationMsg);
                notificationService.AddValidationMessage(notificationMsg);
            }

            return validationErrorList.Count > 0 ? validationErrorList : [];
        }

        public async Task ValidateImportedProperties(DataTable dataTable, List<PropertyConfigModel> propertyConfigs)
        {

            //// Add error-related columns to the DataTable if they don’t exist
            //if (!dataTable.Columns.Contains("HasError"))
            //    dataTable.Columns.Add("HasError", typeof(bool));

            //if (!dataTable.Columns.Contains("Error"))
            //    dataTable.Columns.Add("Error", typeof(string));

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
