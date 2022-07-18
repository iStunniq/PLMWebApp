using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Display(Name = "Price")]
        [Range(1, 30000)]
        public double Price { get; set; }

        [Display(Name = "Original Price")]
        [Range(1, 10000)]
        public double? ListPrice { get; set; }

        [Display(Name = "Price for 50-99 Orders")]
        [Range(1, 30000)]
        public double? Price50 { get; set; }

        [Display(Name = "Price for 100+ Orders")]
        [Range(1, 50000)]
        public double? Price100 { get; set; }

        [ValidateNever]
        public string ImageUrl { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }

        [Required]
        [Display(Name="Brand")]
        public int BrandId { get; set; }
        [ForeignKey("BrandId")]
        [ValidateNever]
        public Brand Brand { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime Inactivity { get; set; } = DateTime.Now;

    }
}
