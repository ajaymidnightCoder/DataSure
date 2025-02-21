using DataSure.Components.Layout;
using DataSure.Contracts.AdminService;
using DataSure.Contracts.DatabaseServices;
using DataSure.Models.AdminModel;
using Microsoft.AspNetCore.Components;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataSure.Components.Elements.EntityConfig
{
    public partial class PropertyGrid
    {

        protected bool IsLoading { get; set; } = true;  // Track loading state

        [Parameter]
        public EntityConfigModel entityConfig { get; set; }

        [Inject]
        protected IEntitiyConfigService entityConfigService { get; set; }

        [Inject]
        protected ISQLiteService sqLiteService { get; set; }

        [CascadingParameter] MainLayout Layout { get; set; }

        private List<PropertyConfigModel> propertyList { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                propertyList = await entityConfigService.GetListByFileName<PropertyConfigModel>(entityConfig.FileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void AddRow()
        {
            bool isPrimary = propertyList?.Count() == 0;
            PropertyConfigModel propertyConfigModel = new()
            {
                Name = string.Empty,
                Code = string.Empty,
                IsRequired = false,
                //IsPrimaryKey = isPrimary
            };
            propertyList.Add(propertyConfigModel);
            StateHasChanged();  // Trigger re-render of the component
        }

        protected async Task Save()
        {

            JsonSerializerOptions options = new()
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

    //        propertyList.GroupBy(p => new { p.Name })
    //.Where(g => g.Count() > 1)
    //.Select(g => g.Key)
    //.ToList();
    //        if (propertyList.)

            Layout.ShowModal("Hello", "This is a global modal!", "Got It");
            var content = JsonSerializer.Serialize<List<PropertyConfigModel>>(propertyList, options);
            await entityConfigService.SaveRawFile(entityConfig.FileName, content);
            Console.Write("Save clicked");

            await sqLiteService.CreateDynamicTableAsync(entityConfig.TableName, propertyList);

            var res = await sqLiteService.GetTableNamesAsync();
        }

        private void Test()
        {
        }

        protected void Delete(PropertyConfigModel propertyConfigModel)
        {
            propertyList.Remove(propertyConfigModel);
            StateHasChanged();  // Trigger re-render of the component
        }

    }
}
