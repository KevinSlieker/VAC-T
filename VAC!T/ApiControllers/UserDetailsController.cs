using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAC_T.DAL.Exceptions;
using VAC_T.Business;
using VAC_T.Data.DTO;
using VAC_T.Models;

namespace VAC_T.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserDetailsController : Controller
    {
        private UserManager<VAC_TUser> _userManager;
        private readonly IMapper _mapper;
        private readonly UserDetailsService _service;

        public UserDetailsController(UserManager<VAC_TUser> userManager, IMapper mapper, UserDetailsService service)
        {
            _userManager = userManager;
            _mapper = mapper;
            _service = service;
        }

        // Get: api/UserDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailsDTO>>> GetAllUsersAsync([FromQuery] string? searchEmail, [FromQuery] string? searchName)
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized("Not the correct roles.");
            }

            try
            {
                var users = await _service.GetUsersAsync(searchName, searchEmail);
                var result = _mapper.Map<List<UserDetailsDTO>>(users);
                return Ok(result);

            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Users' is null.");
            }
        }

        // Get: api/UserDetails/(Users id)
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUserDetailsByIdAsync(string id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }

            try
            {
                var user = await _service.GetUserDetailsAsync(id);
                var userDetails = _mapper.Map<UserDetailsDTO>(user);
                userDetails.Role = (await _userManager.GetRolesAsync(user)).First();
                return Ok(userDetails);

            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Users' is null.");
            }
        }


        // Delete: api/UserDetails/(Users id)
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUserAsync(string id)
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                if (!await _service.DoesUserExistsAsync(id))
                {
                    return NotFound();
                }
                await _service.DeleteUserAsync(id);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Users' is null.");
            }
        }
    }
}
