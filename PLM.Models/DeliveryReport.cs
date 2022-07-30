using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using PLM.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.Models
{
    public class DeliveryReport
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Display(Name = "From:")]
        public DateTime MinDate { get; set; }
        [Display(Name = "To:")]
        public DateTime MaxDate { get; set; }
        public int ReservationAmount { get; set; }
        public DateTime GenerationDate { get; set; }
    }
}
