using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Title")]
        public string Title { get; set; }

        [DisplayName("Notification Type Id")]
        public int NotificationTypeId { get; set; }

        [Required]
        [DisplayName("Message")]
        public string Message { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Has been viewed")]
        public bool Viewed { get; set; }

        [DisplayName("Ticket")]
        public int? TicketId { get; set; }

        [DisplayName("Project")]
        public int? ProjectId { get; set; }

        [DisplayName("Recipient")]
        public string RecipientId { get; set; }

        //Sender
        public string SenderId { get; set; }

        //Navigational
        public virtual NotificationType NotificationType { get; set; }
        public virtual Project Project { get; set; }
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser Recipient { get; set; }
        public virtual BTUser Sender { get; set; }
    }
}
