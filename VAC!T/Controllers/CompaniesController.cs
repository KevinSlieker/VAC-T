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

namespace VAC_T.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<VAC_TUser> _userManager;

        public CompaniesController(ApplicationDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Companies
        public async Task<IActionResult> Index(string searchName)
        {
            ViewData["searchName"] = searchName;
            var company = from c in _context.Company select c;
            if (!string.IsNullOrEmpty(searchName))
            {
                company = company.Where(x => x.Name.Contains(searchName));
            }

              return _context.Company != null ? 
                          View(await company.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Company'  is null.");
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Company == null)
            {
                return NotFound();
            }

            var company = await _context.Company.Include(c => c.JobOffers)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        public async Task<IActionResult> DetailsForEmployer()
        {
            var user = await _userManager.GetUserAsync(User);

            if (_context.Company == null)
            {
                return NotFound();
            }

            var id = _context.Company.Include(x => x.User).Where(x => x.User == user).FirstOrDefault().Id;
            if (id == 0)
            {
                return NotFound();
            }

            var company = await _context.Company.Include(c => c.JobOffers)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View("Details", company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            var company = new Company();
            company.LogoURL = "assets/img/company/default.png";
            return View(company);
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,LogoURL,WebsiteURL,Address,Postcode,Residence")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                var userCompany = new VAC_TUser
                {
                    UserName = "Employer" + company.Name.Replace(" ","") + "@mail.nl",
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
                company = _context.Company.Where(c => c.Name== company.Name).FirstOrDefault();
                int id = company.Id;
                return RedirectToAction("EditCompanyLogo", "FileUpload", new { id });
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Company == null)
            {
                return NotFound();
            }

            var company = await _context.Company.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,LogoURL,WebsiteURL,Address,Postcode,Residence")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }
            ModelState.Remove("User");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Company == null)
            {
                return NotFound();
            }

            var company = await _context.Company
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Company == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Company'  is null.");
            }
            var company = await _context.Company.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (company != null)
            {
                if (company.User != null)
                {
                    var userId = company.User.Id;
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        _context.Users.Remove(user);
                    }
                }
                _context.Company.Remove(company);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
          return (_context.Company?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
