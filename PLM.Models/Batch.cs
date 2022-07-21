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
    public class Batch
    {
        public int Id { get; set; } 

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }
        [Display(Name = "Expiration Date")]
        public DateTime Expiry { get; set; }
        [Display(Name = "Base Price")]
        public double BasePrice { get; set; }
        [Range(0,int.MaxValue, ErrorMessage="Please Enter a Non-negative value")]
        public int Stock { get; set; }

    }
}
