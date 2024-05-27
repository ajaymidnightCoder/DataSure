namespace DataSure.Contracts.Models.AdminModel
{
    public class EntityConfigModel
    {
        public int EntityConfigId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int ParentEntityConfigId { get; set; }
    }
}
