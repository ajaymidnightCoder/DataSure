using DataSure.Contracts.AdminService;
using DataSure.Models.AdminModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Text.Json;

namespace DataSure.Components.Elements.EntityConfig
{
    public partial class SubEntitySelection
    {

        private bool AddSubEntityClicked = false;
        private string SubEntityName = string.Empty;

        // Reference to the input element
        private ElementReference subEntityInput;

        private string selectedFileName;

        protected bool IsLoading { get; set; } = true;  // Track loading state

        [Parameter]
        public string EntityFileName { get; set; }

        private EntityConfigModel selectedEntity { get; set; }

        protected List<EntityConfigModel> childEntities { get; set; }

        [Inject]
        protected IEntitiyConfigService entityConfigService { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                childEntities = await entityConfigService.GetListByFileName<EntityConfigModel>(EntityFileName);
                if (childEntities?.Count > 0)
                {
                    selectedFileName = childEntities.FirstOrDefault().FileName;
                    selectedEntity = childEntities.FirstOrDefault();
                }
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

        private void LinkClicked(EntityConfigModel subEntity)
        {
            selectedFileName = subEntity.FileName;
            selectedEntity = subEntity;
            Console.WriteLine("Clicked");
        }

        private void HandleKeyUp(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                AddSubEntity();
            }
            else if (e.Key == "Escape")
            {
                AddSubEntityClicked = false;
                SubEntityName = string.Empty;
            }
        }

        private void AddSubEntity()
        {
            if (!string.IsNullOrEmpty(SubEntityName))
            {
                int? newEntityId = childEntities.OrderByDescending(x => x.EntityId).FirstOrDefault()?.EntityId;
                newEntityId = newEntityId > 0 ? Convert.ToInt32(newEntityId) + 1 : 1;

                var cleanSubEntityName = SubEntityName.Replace(" ", "").ToLower();

                EntityConfigModel entityConfigModel = new()
                {
                    EntityId = Convert.ToInt32(newEntityId),
                    Name = SubEntityName,
                    FileName = $"{cleanSubEntityName}.json",
                    TableName = cleanSubEntityName
                };
                childEntities.Add(entityConfigModel);

                string content = JsonSerializer.Serialize(childEntities);
                entityConfigService.SaveRawFile(EntityFileName, content);

                entityConfigService.CreatRawFile(entityConfigModel.FileName);

                SubEntityName = string.Empty;
                AddSubEntityClicked = false;

                StateHasChanged();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (AddSubEntityClicked) // Check if AddSubEntityClicked is true
            {
                await subEntityInput.FocusAsync(); // Focus input whenever it becomes visible
            }
        }

    }
}
