namespace DataSure.Models.AdminModel
{
    public class SubEntityConfigModel : EntityConfigModel
    {
        public bool IsMain { get; set; }
        public string? TableName { get; set; }
    }
}