namespace DataSure.Models.AdminModel
{
    public class EntityConfigModel
    {
        public required int EntityId { get; set; }
        public required string Name { get; set; }
        public required string FileName { get; set; }
        public string? TableName { get; set; }
        public string? PrimaryKey { get; set; }
    }
}