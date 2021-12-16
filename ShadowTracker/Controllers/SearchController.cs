using Microsoft.AspNetCore.Mvc;
using ShadowTracker.Data;
using ShadowTracker.Extensions;
using ShadowTracker.Models;
using ShadowTracker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Controllers
{
    public class SearchController : Controller
    {
        private readonly SearchService _search;

        public SearchController(SearchService search)
        {
            _search = search;
        }

        public async Task<IActionResult> SearchProjects(string searchTerm)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            var projects = await _search.GetAllProjectsByCompanyAsync(companyId, searchTerm);
            return View("Search", searchTerm);
        }
    }
}