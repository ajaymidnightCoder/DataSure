using DataSure.Contracts.AdminService;
using DataSure.Contracts.HelperServices;
using DataSure.Models.AdminModel;
using DataSure.Models.NotificationModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Data;
using System.Text;

namespace DataSure.Components.Elements.Input
{
    public partial class SubEntityInput
    {

        [Inject]
        private IEntitiyConfigService entityConfigService { get; set; }
        [Inject]
        private IValidationService validationService { get; set; }
        [Inject]
        private IFileOperationService fileOperationService { get; set; }
        [Inject]
        private INotificationService NotificationService { get; set; }

        private EntityConfigModel? selectedEntity;
        private List<EntityConfigModel> entities = new();
        private List<Dictionary<string, string>> fileData = new(); // Holds parsed file data for display
        private DataTable importedDataTable { get; set; }

        [Parameter]
        public string FileName { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            entities = await entityConfigService.GetListByFileName<EntityConfigModel>(FileName);
            selectedEntity = entities.FirstOrDefault();
        }

        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            string notificationMsg = string.Empty;
            if (!string.IsNullOrEmpty(selectedEntity?.FileName))
            {
                var properties = await entityConfigService.GetListByFileName<PropertyConfigModel>(selectedEntity?.FileName);

                var file = e.File;
                if (file != null)
                {
                    NotificationService.ClearMessages();

                    /* Done
                    * check the uploaded file is csv
                    * convert into datatable
                    * check if required columns are present
                     */

                    var fileExtension = Path.GetExtension(file.Name);

                    if (fileExtension.ToLower() != ".csv")
                    {
                        NotificationService.NotifyUser("Only csv is supported, aborting operation.", MessageType.Error, 100);
                    }

                    importedDataTable = await fileOperationService.ConvertCsvToDataTableAsync(file);

                    if (importedDataTable?.Rows?.Count == 0)
                    {

                    }

                    var headerErrorList = await validationService.ValidateImportedHeadersAsync(importedDataTable, properties);

                    if (headerErrorList?.Count > 0)
                    {
                        NotificationService.NotifyUser("Please resolve the errors and try again.", MessageType.Error, 100);
                    }
                    else
                    {

                        NotificationService.NotifyUser("CSV headers validated.", MessageType.Success, 5);

                        var propertyValidationPassed = await validationService.ValidateDataTableAsync(importedDataTable, properties);
                        
                        
                        if (propertyValidationPassed)
                        {
                            // Notify UI that validation is done
                            NotificationService.NotifyUser("Properties validation Completed!", MessageType.Success, 90);



                        }
                        else
                        {
                            int errorCount = importedDataTable.AsEnumerable().Where(row => row.Field<bool?>("HasError") == true).Count();
                            //int totalCount = dataTable.Rows.Count;

                            // make progress to 100 as further operations will be skipped till validaiton are taken care of
                            int progressCompleted = 100;
                            NotificationService.NotifyUser($"Validation errors found for {errorCount} validation errors found.", MessageType.Error, progressCompleted);
                            NotificationService.UpdateProgress(progressCompleted);

                            await Task.Delay(100);

                            // convert datatable to csv and download to local
                            var csvContent = fileOperationService.ConvertDataTableToCsv(importedDataTable);
                            var bytes = Encoding.UTF8.GetBytes(csvContent);
                            var stream = new MemoryStream(bytes);
                            string fileName = $"validation_{file.Name}";
                            await fileOperationService.DownloadFileAsync(fileName: fileName, stream: stream, toastNotificationMessage: $"Validation errors downlaoded {fileName}");
                        }
                    }


                    /*
                     * Validate DataTable agaisnt the PropertyConfig, add errors column and add error if present
                     * If errors present, convert datatable to csv and download the file
                     * If no error, insert into table in SQLLite
                     */

                }
            }
        }

        private void TabClicked(EntityConfigModel entityConfig)
        {
            selectedEntity = entityConfig;
        }

    }
}
