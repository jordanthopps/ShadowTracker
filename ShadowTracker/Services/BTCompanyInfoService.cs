﻿using Microsoft.EntityFrameworkCore;
using ShadowTracker.Data;
using ShadowTracker.Models;
using ShadowTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Services
{
    public class BTCompanyInfoService : IBTCompanyInfoService
    {
        private readonly ApplicationDbContext _context; 

        public BTCompanyInfoService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<List<BTUser>> GetAllMembersAsync(int companyId)
        {
            try
            {
                List<BTUser> result = new List<BTUser>();

                result = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        //Get Company Info By Id
        public Task<Company> GetCompanyInfoByIdAsync(int? companyId)
        {
            Company result = new();
            try
            {
                if(companyId == null)
                {
                    result = _context.Companies
                                     .Include(c => c.Members)
                                     .Include(c => c.Projects)
                                     .Include(c => c.Invites)
                                     .FirstOrDefaultAsync(c => c.Id == companyId);
                }
            }
            catch (Exception)
            {

                throw;
            }
            throw new NotImplementedException();
        }

        //Get Projects
        public async Task<List<Project>> GetAllProjectsAsync(int? companyId)
        {
            List<Project> result = new();
            try
            {
                if (companyId != null)
                {
                    result = await _context.Projects.Where(p => p.CompanyId == companyId)
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
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        //Get Tickets
        public Task<List<Ticket>> GetAllTicketsAsync(int? companyId)
        {
            throw new NotImplementedException();
        }
    }
}
