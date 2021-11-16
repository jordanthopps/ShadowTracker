using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        [DisplayName("First Name")] //Option 1 Syntax
        public string FirstName { get; set; }

        [Required]
        [Display(Name ="Last Name")] //Option 2 Syntax
        public string LastName { get; set; }

        [NotMapped]
        [DisplayName("Full Name")]
        public string FullName { get { return $"{FirstName} {LastName}"; } }

        //Byte Array Image Properties
        //This property represents a physical file chosen by the user
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile ImageFormFile { get; set; }

        //This represents the byte data not the physical file
        public string ImageFileName { get; set; }
        public byte[] ImageFileData { get; set; }
        public string ImageContentType { get; set; }
    }
}
