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
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public int ReservationAmount { get; set; }
        public double Overhead { get; set; }
        public double BaseCosts { get; set; }
        public double GrossIncome { get; set; }
        public double NetIncome { get; set; }

    }
}
