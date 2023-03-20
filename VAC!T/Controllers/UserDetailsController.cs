using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAC_T.DAL.Exceptions;
using VAC_T.Business;
using VAC_T.Models;

namespace VAC_T.Controllers
{
    public class UserDetailsController : Controller
    {
        private UserManager<VAC_TUser> _userManager;
        private UserDetailsService _service;

        public UserDetailsController(UserManager<VAC_TUser> userManager, UserDetailsService service)
        {
            _userManager = userManager;
            _service = service;
        }

        public async Task<IActionResult> Index(string searchEmail, string searchName)
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized("Not the correct roles.");
            }

            ViewData["searchEmail"] = searchEmail;
            ViewData["searchName"] = searchName;

            try
            {
                var users = await _service.GetUsersAsync(searchName, searchEmail);
                return View(users);

            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Users' is null.");
            }
        }

        // GET: UserDetailsModels/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }

            try
            {
                var user = await _service.GetUserDetailsAsync(id);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                return View(new UserDetailsModel()
                {
                    Id = user.Id,
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
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Users' is null.");
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                var user = await _service.GetUserDetailsAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Users' is null.");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                await _service.DeleteUserAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Users' is null.");
            }
        }
    }
}
