namespace DataSure.Models.AdminModel
{

    public class PropertyConfigModel
    {
        public bool Is { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DataTypeEnum DataType { get; set; }
        public bool IsRequired { get; set; }
        public int? LengthInChar { get; set; }

        // New properties
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }      
        public bool IsForeignKey { get; set; } 
        public string? ForeignKeyReference { get; set; } //Table reference for foreign key
    }

}