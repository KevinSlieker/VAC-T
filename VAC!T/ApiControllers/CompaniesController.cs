using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VAC_T.Data;
using VAC_T.Models;
using VAC_T.Data.DTO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace VAC_T.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<VAC_TUser> _userManager;
        private readonly IMapper _mapper;

        public CompaniesController(ApplicationDbContext context, UserManager<VAC_TUser> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyDTO>>> GetAllCompaniesAsync([FromQuery] string? searchName)
        {
            if (_context.Company == null)
            {
                return NotFound("Database not connected");
            }
            IQueryable<Company>? companies = _context.Company;
            if (!string.IsNullOrEmpty(searchName))
            {
                companies = companies.Where(x => x.Name.Contains(searchName));
            }
            var result = await _mapper.ProjectTo<CompanyDTO>(companies).ToListAsync();
            return Ok(result);
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDTO>> GetCompanyByIdAsync(int id)
        {
            if (id == null || _context.Company == null)
            {
                return NotFound();
            }
            // Include(c => c.JobOffers) gives an error. It sais it's a loop and postman can process it.
            var company = await _context.Company.Include(J => J.JobOffers).Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }
            var result = _mapper.Map<CompanyDTO>(company);

            return Ok(result);
        }

        //public async Task<IActionResult> DetailsForEmployer()
        //{
        //    var user = await _userManager.GetUserAsync(User);

        //    if (_context.Company == null)
        //    {
        //        return NotFound();
        //    }

        //    var id = _context.Company.Include(x => x.User).Where(x => x.User == user).FirstOrDefault().Id;
        //    if (id == 0)
        //    {
        //        return NotFound();
        //    }

        //    var company = await _context.Company.Include(c => c.JobOffers)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (company == null)
        //    {
        //        return NotFound();
        //    }

        //    return View("Details", company);
        //}

        // GET: Companies/Create
        //public IActionResult Create()
        //{
        //    var company = new Company();
        //    company.LogoURL = "assets/img/company/default.png";
        //    return View(company);
        //}

        // POST: Companies/Create
        [HttpPost]
        public async Task<ActionResult> CreateAsync([FromBody] CompanyDTO company)
        {
            if (_context.Company == null)
            {
                return NotFound("Database not connected");
            }
            var companyEntity = _mapper.Map<Company>(company); 
            _context.Add(companyEntity);
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
            _context.SaveChanges();

            companyEntity.User = userCompany;
            _context.SaveChanges();
            companyEntity = _context.Company.Where(c => c.Name == companyEntity.Name).FirstOrDefault();
            int id = companyEntity.Id;
            var newCompany = _mapper.Map<CompanyDTO>(companyEntity);
            return CreatedAtAction(nameof(GetCompanyByIdAsync), new { id }, newCompany);
        }

        // GET: Companies/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _context.Company == null)
        //    {
        //        return NotFound();
        //    }

        //    var company = await _context.Company.FindAsync(id);
        //    if (company == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(company);
        //}

        // POST: Companies/Edit/5

        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(int id, [FromBody] CompanyDTOForUpdate company)
        {
            if (_context.Company == null)
            {
                return NotFound("Database not connected");
            }

            if (id != company.Id)
            {
                ModelState.AddModelError("Id", "Does not match Id in URL");
                return BadRequest(ModelState);
            }
            var companyEntity = _context.Company.FirstOrDefault(c => c.Id == id);
            if (companyEntity == null)
            {
                return NotFound($"No company with Id: {id} in the database");
            }

           _mapper.Map(company, companyEntity);

            _context.Update(companyEntity);
            await _context.SaveChangesAsync();   
            return NoContent();
        }

        // GET: Companies/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.Company == null)
        //    {
        //        return NotFound();
        //    }

        //    var company = await _context.Company
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (company == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(company);
        //}

        // POST: Companies/Delete/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            if (_context.Company == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Company'  is null.");
            }
            var company = await _context.Company.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            if (company.User == null)
            {
                return NotFound();
            }

            var userId = company.User.Id;
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            _context.Company.Remove(company);

            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool CompanyExists(int id)
        {
            return (_context.Company?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
