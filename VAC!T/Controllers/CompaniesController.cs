using Microsoft.AspNetCore.Mvc;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Models;

namespace VAC_T.Controllers
{
    public class CompaniesController : Controller
    {
        private CompanyService _service;

        public CompaniesController(CompanyService service)
        {
            _service = service;
        }

        // GET: Companies
        public async Task<IActionResult> Index(string? searchName)
        {
            ViewData["searchName"] = searchName;

            try {
                var companies = await _service.GetCompaniesAsync(searchName);

                return View(companies);

            } 
            catch (InternalServerException) 
            {
                return Problem("Entity set 'ApplicationDbContext.Company' is null.");
            }
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var company = await _service.GetCompanyAsync(id);
                if (company == null)
                {
                    return NotFound();
                }

                return View(company);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Company' is null.");
            }
        }

        public async Task<IActionResult> DetailsForEmployer()
        {
            try
            {
                var company = await _service.GetCompanyForUserAsync(User);
                if (company == null)
                {
                    return NotFound();
                }

                return View("Details", company);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Company' is null.");
            }
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized("Not the correct roles.");
            }
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
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized("Not the correct roles.");
            }
            if (ModelState.IsValid)
            {
                try { 
                    company = await _service.CreateCompanyWithUserAsync(company);

                    int id = company.Id;
                    return RedirectToAction("EditCompanyLogo", "FileUpload", new { id });
                }
                catch (InternalServerException)
                {
                    return Problem("Entity set 'ApplicationDbContext.Company' is null.");
                }

            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try { 
                var company = await _service.GetCompanyAsync(id);
                if (company == null)
                {
                    return NotFound();
                }
                return View(company);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Company' is null.");
            }

        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,LogoURL,WebsiteURL,Address,Postcode,Residence")] Company company)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            if (id != company.Id)
            {
                return NotFound();
            }
            ModelState.Remove("User");
            if (ModelState.IsValid)
            {
                try
                {
                    if (!await _service.DoesCompanyExistsAsync(id))
                    {
                        return NotFound();
                    }
                    await _service.UpdateCompanyAsync(company);
                    return RedirectToAction(nameof(Index));
                }
                catch (InternalServerException)
                {
                    return Problem("Entity set 'ApplicationDbContext.Company' is null.");
                }
            }
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized("Not the correct roles.");
            }
            try { 
                var company = await _service.GetCompanyAsync(id);
                if (company == null)
                {
                    return NotFound();
                }

                return View(company);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Company' is null.");
            }

        }

        // POST: Companies/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                await _service.DeleteCompanyAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Company' is null.");
            }
        }
    }
}
