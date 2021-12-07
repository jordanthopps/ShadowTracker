using ShadowTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IBTTicketHistoryService
{
    Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId);
    Task AddHistoryAsync(int ticketId, string model, string userId);
    Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId);
    Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId);
}