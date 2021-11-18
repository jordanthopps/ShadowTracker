using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Models
{
    public class NotificationType
    {

        public int Id { get; set; }

        [Required]
        [DisplayName("Notification Type")]
        public string Name { get; set; }
    }
}
