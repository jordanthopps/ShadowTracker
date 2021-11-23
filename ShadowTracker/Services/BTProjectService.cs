using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShadowTracker.Data;
using ShadowTracker.Models;
using ShadowTracker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Services
{
    public class BTProjectService : IBTProjectService
    {

        private readonly ApplicationDbContext _context;

        public BTProjectService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddNewProjectAsync(Project project)
        {
            try
            {
                await _context.AddAsync(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            try
            {
                bool users = (await _context.Users.)
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {

            bool result = (await _userManager)
            //bool result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;

            //return result;
        }

        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                await UpdateProjectAsync(project);
                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetAllProjectsByCompanyAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetArchivedProjectsByCompanyAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> GetDevelopersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
                Project project = await _context.Projects
                                        .Include(p => p.Tickets)
                                            .ThenInclude(t => t.TicketPriorityId)
                                        .Include(p => p.Tickets)
                                            .ThenInclude(t => t.TicketStatus)
                                        .Include(p => p.Tickets)
                                            .ThenInclude(t => t.TicketType)
                                        .Include(p => p.Tickets)
                                            .ThenInclude(t => t.DeveloperUser)
                                        .Include(p => p.Tickets)
                                            .ThenInclude(t => t.OwnerUser)
                                        .Include(p => p.Members)
                                        .Include(p => p.ProjectPriority)
                                        .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                return project;

            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> GetSubmittersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsAssignedProjectManagerAsync(string userId, int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<int> LookupProjectPriorityId(string priorityName)
        {
            throw new NotImplementedException();
        }

        public Task RemoveProjectManagerAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task RestoreProjectAsync(Project project)
        {
            try
            {
                await UpdateProjectAsync(project);
                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
