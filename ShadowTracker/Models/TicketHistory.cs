using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; } //FK non-nullable type

        [Required]
        [DisplayName("Team Member")]
        public string UserId { get; set; } //FK

        
        [DisplayName("Updated Item")]
        public string Property {get; set;}

        
        [DisplayName("Previous")]
        public string OldValue { get; set; }

        [DisplayName("Current")]
        public string NewValue { get; set; }

        [DisplayName("Date Modified")]
        [DataType(DataType.Date)] //Progrommatic Set
        public DateTimeOffset Created { get; set; } //non-nullable type

        [DisplayName("Description of Change")]
        public string Description { get; set; }

        //Navigational
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser User { get; set; }
    }
}
