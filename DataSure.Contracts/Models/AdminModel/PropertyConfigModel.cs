using DataSure.Contracts.Models.Enum;

namespace DataSure.Contracts.Models.AdminModel
{
    public class PropertyConfigModel
    {
        public int EntityConfigId { get; set; }
        public string Name { get; set; }
        public DataTypeEnum DataType { get; set; }
        public bool IsCustomDataType { get; set; }
        public string? Regex { get; set; }
        
    }
}
