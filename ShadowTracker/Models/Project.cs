using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Project Name")]
        public string Name { get; set; }

        [DisplayName("Project Description")]
        [StringLength(2500)]
        public string Description { get; set; }

        [DisplayName("Created")]
        [DataType(DataType.Date)]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Start Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset? StartDate { get; set; } //Return to and fix.

        [DisplayName("End Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset? EndDate { get; set; } //Return to and fix.

        [DisplayName("Archived")]
        public bool Archived { get; set; }

        [DisplayName("Company")]
        public int CompanyId { get; set; }

        [DisplayName("Priority")]
        public int? ProjectPriorityId { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile ImageFormFile { get; set; }

        [DisplayName("File Name")]
        public string FileName { get; set; }
        [DisplayName("Project Image")]
        public byte[] FileData { get; set; }
        [DisplayName("File Extension")]
        public string FileContentType { get; set; }


        //Navigational
        public virtual Company Company  { get; set; }
        
        public virtual ProjectPriority ProjectPriority { get; set; }

        public ICollection<BTUser> Members = new HashSet<BTUser>();
        public ICollection<Ticket> Tickets = new HashSet<Ticket>();
        public ICollection<Notification> Notifications = new HashSet<Notification>();

    }
}
