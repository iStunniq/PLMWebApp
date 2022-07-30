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
    public class InvReportDetail
    {
        public int Id { get; set; }

        public int ReportId { get; set; }
        
        public string DetailType { get; set; }

        public int ProductId { get; set; }

        public int BatchId { get; set; }

        public string ProductName { get; set; }

        public double ProductPrice { get; set; }

        public string ProductCategory { get; set; }

        public string ProductBrand { get; set; }

        public int ProductStock { get; set; }

        public string ProductStatus { get; set; }

        public DateTime ProductExpiry { get; set; }

        public int BatchStock { get; set; } 

        public DateTime BatchExpiry { get; set; }

        public double BatchBase { get; set; }
    } 
}
