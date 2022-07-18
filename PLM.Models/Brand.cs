using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Product Brand")]
        [Required]
        [MaxLength(80)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime Inactivity { get; set; } = DateTime.Now;
    }
}
