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
    public class ReportDetail
    {
        public int Id { get; set; }

        public string ReportType { get; set; }

        public int ReportId { get; set; }
        
        public int HeaderId { get; set; }
        [ForeignKey("HeaderId")]
        [ValidateNever] 
        public ReservationHeader reservationHeader { get; set; }
    }
}
