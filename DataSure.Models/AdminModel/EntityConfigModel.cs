using System.ComponentModel.DataAnnotations;

namespace DataSure.Models.AdminModel
{
    public class EntityConfigModel
    {
        public required int EntityId { get; set; }

        [Required(ErrorMessage = "Entity Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Primary Key is required.")]
        public string PrimaryKey { get; set; } = string.Empty;

        public required string FileName { get; set; }
    }
}