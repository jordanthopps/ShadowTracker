﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Models.ViewModels
{
    public class AddProjectWithPMViewModel
    {

        public Project Project { get; set; }

        public SelectList PMList { get; set; }

        public string PmId { get; set; }

        public SelectList Priority { get; set; }
    }
}
