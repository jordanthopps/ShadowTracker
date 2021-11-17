using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Models
{
    public class Ticket //This is a data class as it stores values in the db
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Title")]
        public string Title { get; set; }

        [Required]
        [StringLength(2500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Created")]
        [DataType(DataType.Date)]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Updated")]
        [DataType(DataType.Date)]
        public DateTimeOffset? Updated { get; set; }

        [DisplayName("Archival Status")]
        public bool Archived { get; set; }

        [DisplayName("Archived By Project")]
        public bool ArchivedByProject { get; set; }

        [DisplayName("Project")]
        public int ProjectId { get; set; }

        [DisplayName("Ticket Type")]
        public int TicketTypeId { get; set; }

        [DisplayName("Ticket Status")]
        public int TicketStatusId { get; set; }

        [DisplayName("Ticket Priority")]
        public int TicketPriorityId { get; set; }

        [DisplayName("Ticket Owner")]
        public string OwnerUserId { get; set; }

        [DisplayName("Ticket Developer")]
        public string DeveloperUserId { get; set; }


        //Navigational Properties
        //Parents
        public virtual Project Project { get; set; }

        public virtual TicketType TicketType { get; set; }

        public virtual TicketStatus TicketStatus { get; set; }

        public virtual TicketPriority TicketPriority { get; set; }

        public virtual BTUser OwnerUser { get; set; }

        public virtual BTUser DeveloperUser { get; set; }

        //Children
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();
        public virtual ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();
        public virtual ICollection<TicketTask> Tasks { get; set; } = new HashSet<TicketTask>();

    }
}
