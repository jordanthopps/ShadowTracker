using Microsoft.AspNetCore.Http;
using ShadowTracker.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using static ShadowTracker.Extensions.MaxFileSizeAttribute;

namespace ShadowTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        public int TicketId { get; set; } //FK (non-nullable data type)

        [DisplayName("Ticket Task")]
        public int? TicketTaskId { get; set; } //FK

       
        public string UserId { get; set; } //FK

        [DisplayName("Attachment Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Attachment Description")]
        public string Description { get; set; }

        //Byte Array Image Properties
        //This property represents a physical file chosen by the user
        [NotMapped]
        [DisplayName("Select a file")]
        [DataType(DataType.Upload)]
        //[MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpeg", ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
        public IFormFile ImageFormFile { get; set; }

        //This represents the byte data not the physical file
        [DisplayName("File Name")]
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
        [DisplayName("Attachment Extension")]
        public string FileContentType { get; set; }

        

        //Navigational
        public virtual Ticket Ticket { get; set; } //Parent

        public virtual BTUser User { get; set; } //Parent
    }
}
