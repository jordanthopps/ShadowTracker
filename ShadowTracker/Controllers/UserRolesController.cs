using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShadowTracker.Models;
using ShadowTracker.Models.ViewModels;
using ShadowTracker.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShadowTracker.Extensions;

namespace ShadowTracker.Controllers
{
    [Authorize(Roles = "Admin")] //You have to be authenticated AND an admin.
    public class UserRolesController : Controller
    {
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly UserManager<BTUser> _userManager;

        public UserRolesController(IBTRolesService rolesService, IBTCompanyInfoService companyInfoService, UserManager<BTUser> userManager) //This is the constructor. The injection part 1
        {
            _rolesService = rolesService;
            _companyInfoService = companyInfoService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            //psuedo code: Add an instance of the ViewModel as a List
            List<ManageUserRolesViewModel> model = new List<ManageUserRolesViewModel>();
            //Get CompanyId
            int companyId = User.Identity.GetCompanyId().Value;

            //Get all company users
            List<BTUser> users = await _companyInfoService.GetAllMembersAsync(companyId);

            //Loop over the users to populate the ViewModel
            // - instantiate ViewModel
            // - use _rolesService
            // - Create multi-selects
            foreach (BTUser user in users)
            {
                ManageUserRolesViewModel viewModel = new ManageUserRolesViewModel();
                viewModel.BTUser = user;
                IEnumerable<string> selected = await _rolesService.GetUserRolesAsync(user);
                viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", selected);

                model.Add(viewModel);
            }

            //Return the model to the View

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            // Instantiate the BTUser
            BTUser btUser = (await _companyInfoService.GetAllMembersAsync(companyId)).FirstOrDefault(u => u.Id == member.BTUser.Id);

            // Get Roles for the User
            IEnumerable<string> roles = await _rolesService.GetUserRolesAsync(btUser);

            //Grab the selected role
            string userRole = member.SelectedRoles.FirstOrDefault();
            if (!string.IsNullOrEmpty(member.SelectedRoles.FirstOrDefault()))
            {
                // Remove User from their roles
                if (await _rolesService.RemoveUserFromRolesAsync(btUser, roles))
                {

                    await _rolesService.AddUserToRoleAsync(btUser, userRole);

                }
            }
            //Navigate back to the View
            return RedirectToAction(nameof(ManageUserRoles));

        }
    }
}
