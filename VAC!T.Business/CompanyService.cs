using Microsoft.AspNetCore.Identity;
using VAC_T.Data;
using VAC_T.Models;
using VAC_T.DAL.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace VAC_T.Business
{
    public class CompanyService
    {
        private readonly IVact_TDbContext _context;
        private UserManager<VAC_TUser> _userManager;

        public CompanyService(IVact_TDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Get a list of companies (optional) that match a search string
        /// </summary>
        /// <param name="searchString">(optional) string to match the name of the companies to </param>
        /// <returns>a list of entries or null </returns>
        public async Task<IEnumerable<Company>> GetCompaniesAsync(string? searchString)
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }

            IQueryable<Company> company = _context.Company;
            if (!string.IsNullOrEmpty(searchString))
            {
                company = company.Where(x => x.Name.Contains(searchString));
            }
            return await company.ToListAsync();
        }

        /// <summary>
        /// Get a Company by the id, including the Job offers
        /// </summary>
        /// <param name="id">The id to find</param>
        /// <returns>An IQueriable with zero or one result</returns>
        /// <exception cref="InternalServerException"></exception>
        public async Task<Company?> GetCompanyAsync(int id, ClaimsPrincipal User)
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }
            var company = from c in _context.Company.Include(c => c.User) select c;
            if (User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER"))
            {
                company = company.Include(c => c.JobOffers);
            } if (User.IsInRole("ROLE_EMPLOYER"))
            {
                var user = await _userManager.GetUserAsync(User);
                company = company.Where(c => c.User == user);
            }
            else
            {
                company = company.Include(c => c.JobOffers.Where(j => j.Closed == null));
            }
            return await company.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<bool> DoesCompanyExistsAsync(int id)
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.Company.AnyAsync(c => c.Id == id);
        }

        public async Task<Company?> GetCompanyForUserAsync(ClaimsPrincipal vactUser)
        {
            var user = await _userManager.GetUserAsync(vactUser);

            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }

            return await _context.Company.Include(x => x.User).Include(c => c.JobOffers).Where(x => x.User == user).FirstOrDefaultAsync();
        }

        public async Task<Company> CreateCompanyWithUserAsync(Company companyEntity)
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }

            _context.Company.Add(companyEntity);
            await _context.SaveChangesAsync();

            var userCompany = new VAC_TUser
            {
                UserName = "Employer" + companyEntity.Name.Replace(" ", "") + "@mail.nl",
                Email = "Employer" + companyEntity.Name.Replace(" ", "") + "@mail.nl",
                EmailConfirmed = true,
                PhoneNumber = "123456798",
                Name = "Employer" + companyEntity.Name.Replace(" ", ""),
                BirthDate = DateTime.Now,
                ProfilePicture = "assets/img/user/profile.png"
            };
            var result = await _userManager.CreateAsync(userCompany, "Employer" + companyEntity.Name.Replace(" ", "") + "123!");
            await _userManager.AddToRoleAsync(userCompany, "ROLE_EMPLOYER");
            await _context.SaveChangesAsync();

            companyEntity.User = userCompany;
            await _context.SaveChangesAsync();
            companyEntity = _context.Company.Where(c => c.Name == companyEntity.Name).First();

            return companyEntity;
        }

        public async Task UpdateCompanyAsync(Company updatedCompany)
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }

            _context.Company.Update(updatedCompany);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCompanyAsync(int id)
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }
            var company = await _context.Company.Include(c => c.User).Include(c => c.Appointments).Include(c => c.Questions).FirstOrDefaultAsync(c => c.Id == id);
            if (company == null)
            {
                return;
            }

            if (company.User != null)
            {
                _context.Users.Remove(company.User);
            }
            if (company.Appointments != null)
            {
                _context.Appointment.RemoveRange(company.Appointments);
            }
            if (company.Questions != null)
            {
                _context.Question.RemoveRange(company.Questions);
            }
            _context.Company.Remove(company);
            await _context.SaveChangesAsync();
        }
    }
}
