using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VAC_T.DAL.Exceptions;
using VAC_T.Data;
using VAC_T.Models;

namespace VAC_T.Business
{
    public class JobOfferService
    {
        private readonly IVact_TDbContext _context;
        private readonly UserManager<VAC_TUser> _userManager;

        public JobOfferService(IVact_TDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Get a list of JobOffers, checks the user with the claimPrincipal.
        /// </summary>
        /// <param name="User">(optional) to get the user using a claimPrincipal </param>
        /// <returns>a list of entries or null </returns>
        public async Task<IEnumerable<JobOffer>> GetJobOffersAsync(ClaimsPrincipal User)
        {

            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }

            var jobOffers = from s in _context.JobOffer.Include(j => j.Company).Where(j => j.Closed == null) select s;
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                var user = await _userManager.GetUserAsync(User);
                jobOffers = jobOffers.Where(C => C.Company.User == user);
            }
            return await jobOffers.ToListAsync();
        }

        public async Task<JobOffer?> GetJobOfferAsync(int id, ClaimsPrincipal User)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var jobOffer = _context.JobOffer.Include(j => j.Company.JobOffers).Include(j => j.Solicitations.Where(x => x.User == user)).Include(j => j.Questions).Include(j => j.Answers.Where(x => x.User == user))
                .FirstOrDefaultAsync(m => m.Id == id);
            return await jobOffer;
        }

        public async Task<JobOffer> CreateJobOfferAsync(JobOffer jobOffer, ClaimsPrincipal User)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var company = await _context.Company.Where(x => x.User == user).FirstAsync();
            jobOffer.Company = company;
            jobOffer.CompanyId = company.Id;
            jobOffer.Residence = company.Residence;
            _context.JobOffer.Add(jobOffer);
            await _context.SaveChangesAsync();
            return jobOffer;
        }

        public async Task<Company> GetCompanyForJobOfferAsync(ClaimsPrincipal User)
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }

            var user = await _userManager.GetUserAsync(User);
            var company = _context.Company.Where(x => x.User == user).FirstAsync();
            return await company;
        }

        public async Task UpdateJobOfferAsync(JobOffer jobOffer)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            _context.JobOffer.Update(jobOffer);
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

        public async Task DeleteJobOfferAsync(int id)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var jobOffer = await _context.JobOffer.FindAsync(id);
            if (jobOffer == null)
            {
                return;
            }
            var appointments = await _context.Appointment.Include(a => a.JobOffer).Where(a => a.JobOffer == jobOffer).ToListAsync();
            if (!appointments.IsNullOrEmpty())
            {
                _context.Appointment.RemoveRange(appointments);
            }

            _context.JobOffer.Remove(jobOffer);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeJobOfferStatusAsync(int id)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var jobOffer = await _context.JobOffer.FindAsync(id);
            if (jobOffer == null)
            {
                return;
            }
            if (jobOffer.Closed == null)
            {
                jobOffer.Closed = DateTime.Now;
            }
            else
            {
                jobOffer.Closed = null;
            }
            _context.JobOffer.Update(jobOffer);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<JobOffer>?> GetAllJobOffersWQuestionsAsync(ClaimsPrincipal User)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                var user = await _userManager.GetUserAsync(User);
                return await _context.JobOffer.Include(j => j.Company).Where(j => j.Closed == null).Where(j => j.Questions.Count() >= 1).Where(j => j.Company.User.Id == user.Id).ToListAsync();
            }
            return await _context.JobOffer.Include(j => j.Company).Where(j => j.Closed == null).Where(j => j.Questions.Count() >= 1).ToListAsync();
        }

        public async Task<JobOffer?> GetJobOfferWQuestionsAsync(int id)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.JobOffer.Include(j => j.Questions).FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<IEnumerable<Question>> GetQuestionsForCompanyAsync(int id) // company id
        {
            if (_context.Question == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.Question.Include(q => q.Options).Include(q => q.Company).Where(q => q.CompanyId == id).ToListAsync();
        }

        public async Task SelectJobOfferQuestionsAsync(int id, int[]? selectedQuestionIds) // jobOffer id
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var jobOffer = await GetJobOfferWQuestionsAsync(id);
            if (jobOffer == null)
            {
                return;
            }
            if (selectedQuestionIds.IsNullOrEmpty())
            {
                jobOffer.Questions = null;
                _context.JobOffer.Update(jobOffer);
                await _context.SaveChangesAsync();
            }
            else
            {
                var selectedList = new List<Question>();
                foreach (int questionId in selectedQuestionIds!)
                {
                    var question = await _context.Question.FirstOrDefaultAsync(q => q.Id == questionId);
                    selectedList.Add(question);
                }
                jobOffer.Questions = selectedList;
                _context.JobOffer.Update(jobOffer);
                await _context.SaveChangesAsync();
            }
        }
    }
}
