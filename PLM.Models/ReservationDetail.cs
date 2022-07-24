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
    public class ReservationDetail
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        [ValidateNever]
        public ReservationHeader ReservationHeader { get; set; }

        [Required]
        public int BatchId { get; set; }
        [ForeignKey("BatchId")]
        [ValidateNever]
        public Batch Batch { get; set; }

        public int Count { get; set; }

        public double Price { get; set; } 
    }
}
