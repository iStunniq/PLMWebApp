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
    public class SalesReport
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Display(Name = "From:")]
        public DateTime MinDate { get; set; }
        [Display(Name = "To:")]
        public DateTime MaxDate { get; set; }
        public int ReservationAmount { get; set; }
        public double Overhead { get; set; }
        [Display(Name = "Initial Expenses")]
        public double? BaseCosts { get; set; }
        [Display(Name = "Gross Income")]
        public double? GrossIncome { get; set; }
        [Display(Name = "Net Profit")] 
        public double? NetIncome { get; set; }
        public DateTime GenerationDate { get; set; } = DateTime.Now;
    }
}
