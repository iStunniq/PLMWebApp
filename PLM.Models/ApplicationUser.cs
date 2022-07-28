using Microsoft.AspNetCore.Identity;
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
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        [Display(Name = "Activity Status")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Date Of Inactivity")]
        public string DateOfInactivity { get; set; }

        public string RoleName { get; set; }
        [Range(0, 2, ErrorMessage = "User can only have up to 2 warnings")]
        public int Warnings { get; set; } = 0;
        public string? Warning1 { get; set; }
        public string? Warning2 { get; set; }
    }
}
