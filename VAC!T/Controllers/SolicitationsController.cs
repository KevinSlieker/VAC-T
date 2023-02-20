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
    public class SolicitationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<VAC_TUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public SolicitationsController(ApplicationDbContext context, UserManager<VAC_TUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Solicitations
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("ROLE_CANDIDATE"))
            {
                return _context.Solicitation != null ?
                   View(await _context.Solicitation.Where(x => x.User == user).Include(x => x.JobOffer.Company).ToListAsync()) :
                   Problem("Entity set 'ApplicationDbContext.Solicitation'  is null.");
            } if (User.IsInRole("ROLE_ADMIN")) 
            {
                return _context.Solicitation != null ?
                  View(await _context.Solicitation.Include(x => x.JobOffer.Company).Include(x => x.User).ToListAsync()) :
                  Problem("Entity set 'ApplicationDbContext.Solicitation'  is null.");
            } 
            else
            {
                return _context.Solicitation != null ?
                  View(await _context.Solicitation.Where(x => x.JobOffer.Company.User == user).Include(x => x.JobOffer.Company).Include(x => x.User).ToListAsync()) :
                  Problem("Entity set 'ApplicationDbContext.Solicitation'  is null.");
            }
        }

        public async Task<IActionResult> Solicitate(int jobOfferId)
        {
            var user = await _userManager.GetUserAsync(User);
            var jobOffer = _context.JobOffer.Find(jobOfferId);
            if (_context.Solicitation.Where(x => x.JobOffer == jobOffer && x.User == user).Any())
            {
                var solicitation = _context.Solicitation.Where(x => x.JobOffer == jobOffer && x.User == user).First();
                if (solicitation != null)
                {
                    _context.Solicitation.Remove(solicitation);
                }
                await _context.SaveChangesAsync();
                return Redirect("/JobOffers/Details/" + jobOffer.Id);
            }
            else
            {
                var solicitation = new Solicitation { User = user, JobOffer = jobOffer, Date = DateTime.Now };
                _context.Add(solicitation);
                await _context.SaveChangesAsync();
                return Redirect("/JobOffers/Details/" + jobOffer.Id);
            }
        }

        public async Task<IActionResult> Select(int id)
        {
            var solicitation = await _context.Solicitation.FindAsync(id); 
            if (solicitation == null)
            {
                Problem("Entity set 'ApplicationDbContext.Solicitation'  is null.");
            }
            if (solicitation.Selected == true)
            {
                solicitation.Selected = false;
                await _context.SaveChangesAsync();
            } else
            {
                solicitation.Selected = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            if (id == null || _context.Solicitation == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        //    return Redirect("/Solicitations/Index");
        //}

        // GET: Solicitations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Solicitation == null)
            {
                return NotFound();
            }

            var solicitation = await _context.Solicitation
                .FirstOrDefaultAsync(m => m.Id == id);
            if (solicitation == null)
            {
                return NotFound();
            }

            return View(solicitation);
        }

        // GET: Solicitations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Solicitations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Selected,Date")] Solicitation solicitation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(solicitation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(solicitation);
        }

        // GET: Solicitations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Solicitation == null)
            {
                return NotFound();
            }

            var solicitation = await _context.Solicitation.FindAsync(id);
            if (solicitation == null)
            {
                return NotFound();
            }
            return View(solicitation);
        }

        // POST: Solicitations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Selected,Date")] Solicitation solicitation)
        {
            if (id != solicitation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(solicitation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SolicitationExists(solicitation.Id))
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
            return View(solicitation);
        }

        // GET: Solicitations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Solicitation == null)
            {
                return NotFound();
            }

            var solicitation = await _context.Solicitation
                .FirstOrDefaultAsync(m => m.Id == id);
            if (solicitation == null)
            {
                return NotFound();
            }

            return View(solicitation);
        }

        // POST: Solicitations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Solicitation == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Solicitation'  is null.");
            }
            var solicitation = await _context.Solicitation.FindAsync(id);
            if (solicitation != null)
            {
                _context.Solicitation.Remove(solicitation);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SolicitationExists(int id)
        {
          return (_context.Solicitation?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
