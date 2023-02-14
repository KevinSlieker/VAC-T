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
        private readonly ApplicationDbContext _context;
        private UserManager<VAC_TUser> _userManager;

        public UserDetailsController(ApplicationDbContext context, UserManager<VAC_TUser> userManager)
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
                CV = user.CV
                });
        }

        private bool UserDetailsModelExists(string id)
        {
          return (_context.UserDetailsModel?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
