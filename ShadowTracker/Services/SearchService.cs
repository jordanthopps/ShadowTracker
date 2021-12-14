using Microsoft.EntityFrameworkCore;
using ShadowTracker.Data;
using ShadowTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Services
{
    public class SearchService
    {
        //TODO: Overload GetAllProjectsByCompanyAsync in IBTProjectService to include int companyId and string searchTerm
        //TODO: In BTProject Service, duplication GetAllProjectsByCompanyAsync to satisfy its interface.
        //TODO: write the controller method in SearchController
        //Bring User into the DevelopmentalLayout
        //Test search.

        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<List<Project>> GetAllProjectsByCompanyAsync(int companyId, string searchTerm)
        {
            List<Project> projects = new();

                try
                {

                    searchTerm = searchTerm.ToLower();

                    projects = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                        .Include(p => p.Members)
                                                        .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.Comments)
                                                        .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.Attachments)
                                                        .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.History)
                                                        .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.Notifications)
                                                        .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.DeveloperUser)
                                                        .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.OwnerUser)
                                                        .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.TicketStatus)
                                                        .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.TicketPriority)
                                                        .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.TicketType)
                                                        .Include(p => p.ProjectPriority)
                                                        .ToListAsync();

                //TODO: Refine search (see ShadowBlog)
                    return projects;
                }
                
            catch (Exception)
            {

                throw;
            }

        }
    }
}
