using Microsoft.AspNetCore.Mvc;
using ShadowTracker.Services.Interfaces;
using System.Threading.Tasks;

namespace ShadowTracker.Controllers
{
    public class UserRolesController : Controller
    {
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyInfoService _companyInfoService;

        public UserRolesController(IBTRolesService rolesService, IBTCompanyInfoService companyInfoService) //This is the constructor. The injection part 1
        {
            _rolesService = rolesService;
            _companyInfoService = companyInfoService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            //psuedo code: Add an instance of the ViewModel as a List
            //Get CompanyId
            //Get all company users

            //Loop over the users to populate the ViewModel
            // - instantiate ViewModel
            // - use _rolesService
            // - Create multi-selects

            //Return the model to the View
        }


        [HttpPost]
        public async Task<IActionResult> ManageUserRoles([ViewModel])
        {
            // Instantiate the BTUser
            // Get Roles for the User
            // Remove User from their roles

            //Grab the selected role

            //Add User to the new role

            //Navigate back to the View


        }
    }
}
