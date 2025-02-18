using DataSure.Contracts.HelperServices;
using DataSure.Models.AdminModel;
using DataSure.Models.NotificationModel;
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

        public async Task<bool> ValidateDataTableAsync(DataTable dataTable, List<PropertyConfigModel> propertyConfigs)
        {
            int totalOperations = propertyConfigs.Count * dataTable.Rows.Count;
            int completedOperations = 0;
            const int minProgress = 10;
            const int maxProgress = 90;
            int validatedPassed = 1; // 1 = true, 0 = false (using int for thread safety)

            // Add error tracking columns if they don't exist
            if (!dataTable.Columns.Contains("HasError"))
                dataTable.Columns.Add("HasError", typeof(bool));

            if (!dataTable.Columns.Contains("Error"))
                dataTable.Columns.Add("Error", typeof(string));

            // Notify UI that validation is starting
            notificationService.NotifyUser("Property validation Started...", MessageType.Neutral, minProgress);
            notificationService.UpdateProgress(minProgress);

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

                    // Update row with errors
                    if (hasError)
                    {
                        Interlocked.Exchange(ref validatedPassed, 0); // Set to 0 (false) in a thread-safe way
                        lock (row) // Ensure thread safety
                        {
                            row["HasError"] = true;
                            row["Error"] = string.Join("; ", errorMessages);
                        }

                        // Notify UI about error
                        //notificationService.NotifyUser("Error Found", MessageType.Error, completedOperations);
                    }

                    // **Update progress safely using Interlocked**
                    int currentProgress = Interlocked.Increment(ref completedOperations) * (maxProgress - minProgress) / totalOperations + minProgress;
                    if (currentProgress <= maxProgress)
                    {
                        notificationService.UpdateProgress(currentProgress);
                    }
                }
            });

            return validatedPassed == 1;
            
        }

        private bool IsValidDataType(string value, DataTypeEnum dataType)
        {
            return dataType switch
            {
                DataTypeEnum.String => true,  // Any value is valid for strings
                DataTypeEnum.Integer => int.TryParse(value, out _),
                DataTypeEnum.DateTime => DateTime.TryParse(value, out _),
                //DataTypeEnum.Decimal => decimal.TryParse(value, out _),
                //DataTypeEnum => bool.TryParse(value, out _),
                _ => true
            };
        }

    }
}
