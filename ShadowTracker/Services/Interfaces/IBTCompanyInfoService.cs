using ShadowTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShadowTracker.Services.Interfaces
{
    public interface IBTCompanyInfoService
    {

        public Task<List<BTUser>> GetAllMembersAsync(int companyId);

    }
}
