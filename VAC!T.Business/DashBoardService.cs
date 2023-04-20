using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VAC_T.DAL.Exceptions;
using VAC_T.Data;
using VAC_T.Models;

namespace VAC_T.Business
{
    public class DashBoardService
    {
        private readonly IVact_TDbContext _context;
        private readonly UserManager<VAC_TUser> _userManager;

        public DashBoardService(IVact_TDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Solicitation>> GetSolicitationsAsync(ClaimsPrincipal User)
        {
            if (_context.Solicitation == null)
            {
                throw new InternalServerException("Database not found");
            }

            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                var company = await _context.Company.Where(c => c.User == user).FirstAsync();
                return await _context.Solicitation.Where(s => s.JobOffer.Company == company).ToListAsync();
            }
            if (User.IsInRole("ROLE_CANDIDATE"))
            {
                return await _context.Solicitation.Where(s => s.User == user).ToListAsync();
            }
            return await _context.Solicitation.ToListAsync();
        }

        public async Task<Company> GetCompanyAsync(ClaimsPrincipal User)
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            return await _context.Company.Where(c => c.User == user).Include(c => c.JobOffers).Include(c => c.Appointments).Include(c => c.RepeatAppointments).FirstAsync();
        }

    }
}
