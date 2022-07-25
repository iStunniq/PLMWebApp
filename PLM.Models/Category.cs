using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PLM.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [DisplayName("Display Order")]
        [Range(1, 1000, ErrorMessage = "Display Order 1-1000 only")]
        public int DisplayOrder { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public DateTime Inactivity { get; set; } = DateTime.Now;
    }
}
