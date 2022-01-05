using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShadowTracker.Data;
using ShadowTracker.Extensions;
using ShadowTracker.Models;
using ShadowTracker.Models.Enums;
using ShadowTracker.Models.ViewModels;
using ShadowTracker.Services.Interfaces;

namespace ShadowTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTLookupService _lookupService;
        private readonly IBTFileService _fileService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTTicketService _ticketService;

        public ProjectsController(UserManager<BTUser> userManager, IBTProjectService projectService, IBTRolesService rolesService, IBTLookupService lookupService, IBTFileService fileService, IBTNotificationService notificationService, IBTTicketService ticketService)
        {
            _userManager = userManager;
            _projectService = projectService;
            _rolesService = rolesService;
            _lookupService = lookupService;
            _fileService = fileService;
            _notificationService = notificationService;
            _ticketService = ticketService;
        }


        //Get: My Projects
        public async Task<IActionResult> MyProjects()
        {
            //Get Current User Id
            string userId = _userManager.GetUserId(User);

            List<Project> model = await _projectService.GetUserProjectsAsync(userId);

            return View(model);
            
        }
        
        //Get: All Projects
        public async Task<IActionResult> AllProjects()
        {
            //Get Current User Id
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> model = await _projectService.GetAllProjectsByCompanyAsync(companyId);

            return View(model);
            
        }

        //GET: Archived Projects
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> model = await _projectService.GetArchivedProjectsByCompanyAsync(companyId);

            return View(model);
        }


        //GET: UnAssigned Projects
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> model = await _projectService.GetUnassignedProjectsAsync(companyId);

            return View(model);
        }

        [Authorize(Roles="Admin")]
        [HttpGet]
        public async Task<IActionResult> AssignPM(int projectId)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AssignPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(projectId, companyId);
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager),companyId),"Id","FullName");

            return View(model);
        }

        [Authorize(Roles="Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {
            if(!string.IsNullOrEmpty(model.PMId))
            {
                await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                return RedirectToAction(nameof(Details), new { id = model.Project.Id });
            }

            return RedirectToAction(nameof(AssignPM), new { projectId = model.Project.Id });
        }

        [HttpGet]
        [Authorize(Roles="Admin, ProjectManager")]
        public async Task<IActionResult> AssignMembers(int projectId)
        {
            ProjectMembersViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            model.Project = await _projectService.GetProjectByIdAsync(projectId, companyId);

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);

            List<BTUser> members = developers.Concat(submitters).ToList();

            List<string> projectMembers = model.Project.Members.Select(p => p.Id).ToList();
            model.Members = new MultiSelectList(members, "Id", "FullName", projectMembers);

            return View(model);
        }

        [Authorize(Roles="Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if (model.SelectedUsers != null)
            {
                List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id)).Select(p => p.Id).ToList();

                //Remove current members
                foreach (string member in memberIds)
                {
                    await _projectService.RemoveUserFromProjectAsync(member, model.Project.Id);
                }

                //Add selected members
                foreach (string member in model.SelectedUsers)
                {
                    await _projectService.AddUserToProjectAsync(member, model.Project.Id);
                }
            }

            //go to project details
            return RedirectToAction(nameof(Details), new { id = model.Project.Id });
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            //Get Company Id
            int companyId = User.Identity.GetCompanyId().Value;

            // Add ViewModel
            AddProjectWithPMViewModel model = new();

            //Load model/SelectLists with data
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");
            model.Priority = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name" );

            return View(model);
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {
            BTUser btUser = await _userManager.GetUserAsync(User);
            if (model != null)
            {
                int companyId = User.Identity.GetCompanyId().Value;
                try
                {
                    if(model.Project.ImageFormFile != null)
                    {
                        model.Project.FileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.FileName = model.Project.ImageFormFile.FileName;
                        model.Project.FileContentType = model.Project.ImageFormFile.ContentType;
                    }
                    model.Project.CompanyId = companyId;
                    model.Project.Created = DateTimeOffset.Now;

                    await _projectService.AddNewProjectAsync(model.Project);

                    //Add PM if one was chosen
                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);
                    }

                    //Save/Send Notification
                    Notification notification = new()
                    {
                        ProjectId = model.Project.Id,
                        NotificationTypeId = (await _lookupService.LookupNotificationTypeId(nameof(BTNotificationType.Ticket))).Value,
                        Title = "Project Created",
                        Message = $"Project : {model.Project.Name}, was assigned by {btUser.FullName}",
                        SenderId = btUser.Id
                    };

                    await _notificationService.AddNotificationAsync(notification);
                    await _notificationService.SendEmailNotificationsByRoleAsync(notification, companyId, nameof(BTRoles.Admin));

                    return RedirectToAction(nameof(AllProjects));
                }
                catch (Exception)
                {

                    throw;
                }

            }

            ViewData["ProjectPriorityId"] = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Id", model.Project.ProjectPriorityId);
            return View(model.Project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;
            //Add ViewModel instance "AddProjectWithPMViewModel"
            AddProjectWithPMViewModel model = new();

            //Get Project based on Id
            model.Project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            //Load model/Selectlists with data
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), "Id", "FullName");
            model.Priority = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");


            if (model.Project == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
                try
                {
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.FileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.FileName = model.Project.ImageFormFile.FileName;
                        model.Project.FileContentType = model.Project.ImageFormFile.ContentType;
                    }

                    await _projectService.UpdateProjectAsync(model.Project);

                    //Add PM if one was chosen
                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);
                    }

                    return RedirectToAction("AllProjects");
                            }
                catch (DbUpdateConcurrencyException)
                {

                    if (!await ProjectExists(model.Project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["ProjectPriorityId"] = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Id", model.Project.ProjectPriorityId);
            return View(model.Project);
        }

        // GET: Projects/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;
            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Archive/5 
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirm(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Project project = await _projectService.GetProjectByIdAsync(id, companyId);
            project.Archived = true;
            await _projectService.ArchiveProjectAsync(project);
            foreach (Ticket ticket in project.Tickets)
            {
                ticket.ArchivedByProject = true;
                ticket.Archived = true;
                await _ticketService.ArchiveTicketAsync(ticket);
            }
           
            return RedirectToAction(nameof(AllProjects));
        }

        private async Task<bool> ProjectExists(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            return (await _projectService.GetAllProjectsByCompanyAsync(companyId)).Any(p => p.Id == id);
        }
        
        // GET: Projects/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;
            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5 
        [HttpPost, ActionName("Restore")] //Annotated action name b/c the method name below does not match the name of the associated view.
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirm(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Project project = await _projectService.GetProjectByIdAsync(id, companyId);

            project.Archived = false;

            await _projectService.RestoreProjectAsync(project);
            return RedirectToAction(nameof(AllProjects));
        }
    }
}
