using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.Models.ViewModels
{
    public class HomeVM
    {
        public IEnumerable<Product> Products { get; set; }

        public int? Alert { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem>? CategoryList { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem>? BrandList { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }

    }
}
