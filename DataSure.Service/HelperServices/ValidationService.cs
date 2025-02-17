using DataSure.Contracts.HelperServices;
using DataSure.Models.AdminModel;
using DataSure.Models.Enum;
using DataSure.Models.NotificationModel;
using System.Collections.Concurrent;
using System.Data;

namespace DataSure.Service.HelperServices
{
    public class ValidationService(IFileOperationService fileOperationService, INotificationService notificationService) : IValidationService
    {

        public async Task<List<string>?> ValidateImportedHeadersAsync(DataTable dataTable, List<PropertyConfigModel> propertyConfigs)
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
                //notificationService.AddValidationMessage(notificationMsg);
                notificationService.NotifyUser(message: notificationMsg, MessageType.Error, 50);
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
                //notificationService.AddValidationMessage(notificationMsg);
                notificationService.NotifyUser(message: notificationMsg, MessageType.Error, 50);
            }

            return validationErrorList.Count > 0 ? validationErrorList : [];
        }

        public async Task ValidateDataTableAsync(DataTable dataTable, List<PropertyConfigModel> propertyConfigs)
        {
            string notificationMsg = string.Empty;
            int totalOperations = propertyConfigs.Count * dataTable.Rows.Count; // Total validations
            int completedOperations = 0; // Track progress
            const int minProgress = 10; // Start progress at 10%
            const int maxProgress = 90; // Cap progress at 90%
            int currentProgress = minProgress;

            var errorRows = new ConcurrentBag<DataRow>(); // Thread-safe collection

            // Notify UI that validation is starting
            notificationService.NotifyUser("Validation Started...", MessageType.Neutral, minProgress);

            Parallel.ForEach(propertyConfigs, propertyConfig =>
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    bool hasError = false;
                    var cellValue = row[propertyConfig.Code]?.ToString();
                    var errorMessages = new List<string>();

                    // Validate required fields
                    if (propertyConfig.IsRequired && string.IsNullOrWhiteSpace(cellValue))
                    {
                        hasError = true;
                        errorMessages.Add($"Column '{propertyConfig.Code}' is required.");
                    }

                    // Validate data type
                    if (!string.IsNullOrWhiteSpace(cellValue) && !IsValidDataType(cellValue, propertyConfig.DataType))
                    {
                        hasError = true;
                        errorMessages.Add($"Column '{propertyConfig.Code}' has an invalid data type.");
                    }

                    // Validate length
                    if (propertyConfig.LengthInChar.HasValue && cellValue?.Length > propertyConfig.LengthInChar.Value)
                    {
                        hasError = true;
                        errorMessages.Add($"Column '{propertyConfig.Code}' exceeds {propertyConfig.LengthInChar.Value} characters.");
                    }

                    // Mark row as having errors
                    if (hasError)
                    {
                        lock (row) // Ensure thread safety
                        {
                            row["HasError"] = true;
                            row["Error"] = string.Join("; ", errorMessages);
                        }
                        errorRows.Add(row);
                    }

                    // **Update progress safely using Interlocked**
                    int currentProgress = Interlocked.Increment(ref completedOperations) * (maxProgress - minProgress) / totalOperations + minProgress;
                    if (currentProgress <= maxProgress)
                    {
                        //notificationService.UpdateProgress(currentProgress);
                        notificationService.NotifyUser("Validation Completed!", MessageType.Success, maxProgress);
                    }
                }
            });

            // Notify UI that validation is done (progress stays at 90%)
            notificationService.UpdateProgress(maxProgress);
            notificationService.NotifyUser("Validation Completed!", MessageType.Success, maxProgress);
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
