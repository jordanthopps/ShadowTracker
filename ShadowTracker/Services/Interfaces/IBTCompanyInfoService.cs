using ShadowTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShadowTracker.Services.Interfaces
{
    public interface IBTCompanyInfoService
    {

        public Task<List<BTUser>> GetAllMembersAsync(int companyId);
        public Task<Company> GetCompanyInfoByIdAsync(int? companyId);
        public Task<List<Project>> GetAllProjectsAsync(int? companyId);
        public Task<List<Ticket>> GetAllTicketsAsync(int? companyId);
    }
}
