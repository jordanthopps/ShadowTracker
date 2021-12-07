using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShadowTracker.Data;
using ShadowTracker.Models;
using ShadowTracker.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShadowTracker.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BTUser> _userManager;
        private readonly ApplicationDbContext _context;


        public BTRolesService(RoleManager<IdentityRole> roleManager,
            UserManager<BTUser> userManager, 
            ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            try
            {
                bool result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;

                return result;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<string> GetRoleNameByIdAsync(string roleId)
        {
            try
            {
                IdentityRole role = _context.Roles.Find(roleId);
                string result = await _roleManager.GetRoleNameAsync(role);

                return result;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<List<IdentityRole>> GetRolesAsync() //List of Identity Role is the return type
        {
            try
            {
                List<IdentityRole> result = new List<IdentityRole>();

                result = await _context.Roles.ToListAsync();

                return result;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(BTUser user) //IEnumerable of string is the return type
        {
            try
            {
                IEnumerable<string> result = await _userManager.GetRolesAsync(user);
                return result;
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId) //List of BTUser is the return type
        {
            List<BTUser> users = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
            List<BTUser> result = users.Where(u => u.CompanyId == companyId).ToList();

            return result;
        }

        public Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> IsUserInRoleAsync(BTUser user, string roleName)
        {
            try
            {
                bool result = await _userManager.IsInRoleAsync(user, roleName); //this is a check
                return result;
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            try
            {
                bool result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded; //this REMOVE is an action, hence, parentheses
                return result;
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles)
        {
            try
            {
                bool result = (await _userManager.RemoveFromRolesAsync(user, roles)).Succeeded;
                return result;
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}
