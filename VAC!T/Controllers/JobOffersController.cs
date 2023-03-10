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
    public class JobOffersController : Controller
    {
        private readonly IVact_TDbContext _context;
        private readonly UserManager<VAC_TUser> _userManager;
        private readonly SignInManager<VAC_TUser> _signInManager;

        public JobOffersController(IVact_TDbContext context, UserManager<VAC_TUser> userManager, SignInManager<VAC_TUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: JobOffers
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("ROLE_EMPLOYER") && _signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                return _context.JobOffer != null ?
                            View(await _context.JobOffer.Include(j => j.Company).Where(C => C.Company.User == user).ToListAsync()) :
                            Problem("Entity set 'ApplicationDbContext.JobOffer'  is null.");
            }
            else
            {
                return _context.JobOffer != null ?
                            View(await _context.JobOffer.Include(j => j.Company).ToListAsync()) :
                            Problem("Entity set 'ApplicationDbContext.JobOffer'  is null.");
            }
        }

        // GET: JobOffers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.JobOffer == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(User);
            var jobOffer = await _context.JobOffer.Include(j => j.Company.JobOffers).Include(j => j.Solicitations.Where( x => x.User == user))
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jobOffer == null)
            {
                return NotFound();
            }

            return View(jobOffer);
        }

        // GET: JobOffers/Create
        public async Task<IActionResult> Create()
        {
            var jobOffer = new JobOffer();
            var user = await _userManager.GetUserAsync(User);
            var company = _context.Company.Where(x => x.User == user).First();
            jobOffer.CompanyId = company.Id;
            jobOffer.Residence = company.Residence;
            return View(jobOffer);
        }

        // POST: JobOffers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,LogoURL,Level,CompanyId,Residence")] JobOffer jobOffer)
        {
            ModelState.Remove("Company");
            if (ModelState.IsValid)
            {
                jobOffer.Company = await _context.Company.FindAsync(jobOffer.CompanyId);
                _context.JobOffer.Add(jobOffer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(jobOffer);
        }

        // GET: JobOffers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.JobOffer == null)
            {
                return NotFound();
            }

            var jobOffer = await _context.JobOffer.Include(j => j.Company).FirstOrDefaultAsync(x => x.Id == id);
            if (jobOffer == null)
            {
                return NotFound();
            }
            return View(jobOffer);
        }

        // POST: JobOffers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,LogoURL,Level,Created,Residence,CompanyId,Company")] JobOffer jobOffer)
        {
            if (id != jobOffer.Id)
            {
                return NotFound();
            }
            ModelState.Remove("Company");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.JobOffer.Update(jobOffer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobOfferExists(jobOffer.Id))
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
            return View(jobOffer);
        }

        // GET: JobOffers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.JobOffer == null)
            {
                return NotFound();
            }

            var jobOffer = await _context.JobOffer
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jobOffer == null)
            {
                return NotFound();
            }

            return View(jobOffer);
        }

        // POST: JobOffers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.JobOffer == null)
            {
                return Problem("Entity set 'ApplicationDbContext.JobOffer'  is null.");
            }
            var jobOffer = await _context.JobOffer.FindAsync(id);
            if (jobOffer != null)
            {
                _context.JobOffer.Remove(jobOffer);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobOfferExists(int id)
        {
          return (_context.JobOffer?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
