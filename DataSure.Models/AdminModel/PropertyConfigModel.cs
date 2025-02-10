using DataSure.Models.Enum;

namespace DataSure.Models.AdminModel
{

    public class PropertyConfigModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public DataTypeEnum DataType { get; set; }
        public bool IsRequired { get; set; }
        public int? LengthInChar { get; set; }
    }

}