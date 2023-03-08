using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VAC_T.Data;
using VAC_T.Data.DTO;
using VAC_T.Models;

namespace VAC_T.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<VAC_TUser> _userManager;
        private readonly IMapper _mapper;

        public UserDetailsController(ApplicationDbContext context, UserManager<VAC_TUser> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }


        [HttpGet("{id}")]
        public async Task<ActionResult> GetUserDetailsByIdAsync(string id)
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
            var userDetails = _mapper.Map<UserDetailsDTO>(user);
            userDetails.Role = (await _userManager.GetRolesAsync(user)).First();
            return Ok(userDetails);
        }

        //private bool UserDetailsModelExists(string id)
        //{
        //    return (_context.UserDetailsModel?.Any(e => e.Id == id)).GetValueOrDefault();
        //}

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailsDTO>>> GetAllUsersAsync([FromQuery] string? searchEmail, [FromQuery] string? searchName)
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized();
            }

            if (_context.Users != null)
            {
                NotFound("Database not connected");
            }


            var users = from s in _context.Users select s;

            if (!string.IsNullOrEmpty(searchEmail))
            {
                users = users.Where(u => u.Email!.Contains(searchEmail));
            }

            if (!string.IsNullOrEmpty(searchName))
            {
                users = users.Where(u => u.Name!.Contains(searchName));
            }

            var result = await _mapper.ProjectTo<UserDetailsDTO>(users).ToListAsync();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
