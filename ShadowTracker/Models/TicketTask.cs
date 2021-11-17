using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Models
{
    public class TicketTask
    {
        public int Id { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; } //FK

        [DisplayName("Task Status")]
        public int TaskStatusId { get; set; } //FK

        [DisplayName("Task Type")]
        public int TaskTypeId { get; set; } //FK

        [DisplayName("Task Name")]
        public string Name { get; set; }

        [DisplayName("Task Description")]
        public string Description { get; set; }

        //Navigational Properties
        public virtual Ticket Ticket { get; set; }
        public virtual TicketStatus TaskStatus { get; set; }
        public virtual TicketType TaskType { get; set; }

        public virtual ICollection<TicketAttachment> TicketAttachments { get; set; } = new HashSet<TicketAttachment>();
    }
}
