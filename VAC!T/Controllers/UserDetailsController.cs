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
    public class UserDetailsController : Controller
    {
        private readonly IVact_TDbContext _context;
        private UserManager<VAC_TUser> _userManager;

        public UserDetailsController(IVact_TDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: UserDetailsModels/Details/5
        public async Task<IActionResult> Details(string id)
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

            return View(new UserDetailsModel() { Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                BirthDate = user.BirthDate,
                Address = user.Address,
                Postcode = user.Postcode,
                Residence = user.Residence,
                ProfilePicture = user.ProfilePicture,
                Motivation = user.Motivation,
                CV = user.CV,
                Role = (await _userManager.GetRolesAsync(user)).First(),
                });
        }

        private bool UserDetailsModelExists(string id)
        {
          return (_context.UserDetailsModel?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IActionResult> Index(string searchEmail, string searchName)
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized();
            }

            ViewData["searchEmail"] = searchEmail;
            ViewData["searchName"] = searchName;

            var users = from s in _context.Users select s;
            
            if (!string.IsNullOrEmpty(searchEmail))
            {
                users = users.Where(u => u.Email!.Contains(searchEmail));
            }

            if (!string.IsNullOrEmpty(searchName))
            {
                users = users.Where(u => u.Name!.Contains(searchName));
            }

            return _context.Users != null ?
                          View(await users.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Users'  is null.");
        }

        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null || _context.Users == null)
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Users'  is null.");
            }
            var user = await _context.Users.FirstOrDefaultAsync(c => c.Id == id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
