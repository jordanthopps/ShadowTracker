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
    public class BTCompanyInfoService : IBTCompanyInfoService
    {
        private readonly ApplicationDbContext _context; //2

        public BTCompanyInfoService(ApplicationDbContext context) //1
        {
            _context = context; //3
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
    }
}
