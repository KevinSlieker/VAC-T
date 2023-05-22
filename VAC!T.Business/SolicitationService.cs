using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VAC_T.DAL.Exceptions;
using VAC_T.Data;
using VAC_T.Models;

namespace VAC_T.Business
{
    public class SolicitationService
    {
        private readonly IVact_TDbContext _context;
        private readonly UserManager<VAC_TUser> _userManager;

        public SolicitationService(IVact_TDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Solicitation>> GetSolicitationsAsync(ClaimsPrincipal User, string? searchJobOffer, string? searchCompany,
            string? searchCandidate, bool? searchSelectedYes, bool? searchSelectedNo)
        {

            if (_context.Solicitation == null)
            {
                throw new InternalServerException("Database not found");
            }

            var solicitation = from s in _context.Solicitation.Include(a => a.Appointment) select s;
            if (!string.IsNullOrEmpty(searchJobOffer))
            {
                solicitation = solicitation.Where(s => s.JobOffer.Name.Contains(searchJobOffer));
            }

            if (!string.IsNullOrEmpty(searchCompany))
            {
                solicitation = solicitation.Where(s => s.JobOffer.Company.Name.Contains(searchCompany));
            }

            if (!string.IsNullOrEmpty(searchCandidate))
            {
                solicitation = solicitation.Where(s => s.User.Name!.Contains(searchCandidate));
            }

            if (searchSelectedYes == true)
            {
                solicitation = solicitation.Where(s => s.Selected == true);
            }

            if (searchSelectedNo == true)
            {
                solicitation = solicitation.Where(s => s.Selected == false);
            }
            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("ROLE_CANDIDATE"))
            {
                solicitation = solicitation.Where(x => x.User == user).Include(x => x.JobOffer.Company);
            }
            if (User.IsInRole("ROLE_ADMIN"))
            {
                solicitation = solicitation.Include(x => x.JobOffer.Company).Include(x => x.User);
            }
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                solicitation = solicitation.Where(x => x.JobOffer.Company.User == user).Include(x => x.JobOffer.Company).Include(x => x.User);
            }
            return await solicitation.ToListAsync();
        }
        public async Task<Solicitation?> CreateSolicitationAsync(int jobOfferId, ClaimsPrincipal User)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            if (_context.Solicitation == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var jobOffer = await _context.JobOffer.FindAsync(jobOfferId);
            if (await _context.Solicitation.Where(x => x.JobOffer == jobOffer && x.User == user).AnyAsync() ==  true)
            {
                return null; // you already have a solicitation for this JobOffer
            }
            var solicitation = new Solicitation { User = user!, JobOffer = jobOffer!, Date = DateTime.Now };
            _context.Solicitation.Add(solicitation);
            await _context.SaveChangesAsync();
            return solicitation;
        }

        public async Task DeleteSolicitationAsync(int jobOfferId, ClaimsPrincipal User)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            if (_context.Solicitation == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var jobOffer = await _context.JobOffer.FindAsync(jobOfferId);
            var solicitation = await _context.Solicitation.Where(x => x.JobOffer == jobOffer && x.User == user).FirstOrDefaultAsync();
            if (solicitation == null)
            {
                return;
            }
            _context.Solicitation.Remove(solicitation);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DoesJobOfferExistsAsync(int id)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.JobOffer.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> DoesSolicitationExistWithJobOfferIdAsync(int jobOfferId, ClaimsPrincipal User)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            if (_context.Solicitation == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var jobOffer = await _context.JobOffer.FindAsync(jobOfferId);
            return await _context.Solicitation.Where(x => x.JobOffer == jobOffer && x.User == user).AnyAsync();
        }


        public async Task SelectSolicitationAsync(int id)
        {
            if (_context.Solicitation == null)
            {
                throw new InternalServerException("Database not found");
            }
            var solicitation = await _context.Solicitation.FindAsync(id);
            if (solicitation == null)
            {
                return;
            }
            if (solicitation.Selected == true)
            {
                solicitation.Selected = false;
                solicitation.DateSelectedIsTrue = null;
                _context.Solicitation.Update(solicitation);
                await _context.SaveChangesAsync();
            }
            else
            {
                solicitation.Selected = true;
                solicitation.DateSelectedIsTrue = DateTime.Now;
                _context.Solicitation.Update(solicitation);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> DoesSolicitationExistsAsync(int id)
        {
            if (_context.Solicitation == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.Solicitation.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> AreQuestionsAnsweredAsync(int id, ClaimsPrincipal User) // jobOfferId
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            return await _context.JobOffer.AnyAsync(j => j.Questions.Count() == j.Answers.Where(a => a.UserId == user.Id).Count());
        }
    }
}
