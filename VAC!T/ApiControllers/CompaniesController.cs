using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VAC_T.Data;
using VAC_T.Models;

namespace VAC_T.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<VAC_TUser> _userManager;

        public CompaniesController(ApplicationDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetAllCompaniesAsync()
        {
            if (_context.Company != null)
            {
                NotFound("Database not connected");
            }            

            return await _context.Company.ToListAsync();
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetCompanyByIdAsync(int? id)
        {
            if (id == null || _context.Company == null)
            {
                return NotFound();
            }
            // Include(c => c.JobOffers) gives an error. It sais it's a loop and postman can process it.
            var company = await _context.Company
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return company;
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
        public async Task<ActionResult> CreateAsync([FromBody] Company company)
        {
            if (_context.Company == null)
            {
                return NotFound("Database not connected");
            }

            _context.Add(company);
            await _context.SaveChangesAsync();
            var userCompany = new VAC_TUser
            {
                UserName = "Employer" + company.Name.Replace(" ", "") + "@mail.nl",
                Email = "Employer" + company.Name.Replace(" ", "") + "@mail.nl",
                EmailConfirmed = true,
                PhoneNumber = "123456798",
                Name = "Employer" + company.Name.Replace(" ", ""),
                BirthDate = DateTime.Now,
                ProfilePicture = "assets/img/user/profile.png"
            };
            var result = await _userManager.CreateAsync(userCompany, "Employer" + company.Name.Replace(" ", "") + "123!");
            await _userManager.AddToRoleAsync(userCompany, "ROLE_EMPLOYER");
            _context.SaveChanges();

            company.User = userCompany;
            _context.SaveChanges();
            company = _context.Company.Where(c => c.Name == company.Name).FirstOrDefault();
            int id = company.Id;
            return CreatedAtAction(nameof(GetCompanyByIdAsync), new { id }, null);
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
        public async Task<ActionResult> PutAsync(int id, [FromBody] Company company)
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
            _context.Update(company);
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
