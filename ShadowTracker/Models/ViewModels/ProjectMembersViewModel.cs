
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ShadowTracker.Models.ViewModels
{
    public class ProjectMembersViewModel
    {
        public Project Project { get; set; }

        public MultiSelectList Members { get; set; }

        public List<string> SelectedUsers { get; set; }
    }
}
