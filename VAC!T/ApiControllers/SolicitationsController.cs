using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SolicitationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<VAC_TUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public SolicitationsController(ApplicationDbContext context, UserManager<VAC_TUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        // GET: api/Solicitations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SolicitationDTOComplete>>> GetAllSolicitationsAsync([FromQuery] string? searchName,
            [FromQuery] string? searchCompany, [FromQuery] string? searchCandidate, [FromQuery] bool? searchSelectedYes, [FromQuery] bool? searchSelectedNo)
        {
            if (_context.Solicitation == null)
            {
                return NotFound("Database not connected");
            }

            var solicitation = from s in _context.Solicitation select s;

            if (!string.IsNullOrEmpty(searchName))
            {
                solicitation = solicitation.Where(s => s.JobOffer.Name.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(searchCompany))
            {
                solicitation = solicitation.Where(s => s.JobOffer.Company.Name.Contains(searchCompany));
            }

            if (!string.IsNullOrEmpty(searchCandidate))
            {
                solicitation = solicitation.Where(s => s.User.Name.Contains(searchCandidate));
            }

            if (searchSelectedYes == true)
            {
                solicitation = solicitation.Where(s => s.Selected == true);
            }

            if (searchSelectedNo == true)
            {
                solicitation = solicitation.Where(s => s.Selected == false);
            }

            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("ROLE_CANDIDATE"))
            {
                solicitation = solicitation.Where(x => x.User == user).Include(x => x.JobOffer.Company);
            }
            if (User.IsInRole("ROLE_ADMIN"))
            {
                solicitation = solicitation.Include(x => x.JobOffer.Company).Include(x => x.User);
            }
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                solicitation = solicitation.Where(x => x.JobOffer.Company.User == user).Include(x => x.JobOffer.Company).Include(x => x.User);
            }
            var result = await _mapper.ProjectTo<SolicitationDTOComplete>(solicitation).ToListAsync();
            return Ok(result);

        }

        //public async Task<IActionResult> Solicitate(int jobOfferId) what do i do for this?
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    var jobOffer = _context.JobOffer.Find(jobOfferId);
        //    if (_context.Solicitation.Where(x => x.JobOffer == jobOffer && x.User == user).Any())
        //    {
        //        var solicitation = _context.Solicitation.Where(x => x.JobOffer == jobOffer && x.User == user).First();
        //        if (solicitation != null)
        //        {
        //            _context.Solicitation.Remove(solicitation);
        //        }
        //        await _context.SaveChangesAsync();
        //        return Redirect("/JobOffers/Details/" + jobOffer.Id);
        //    }
        //    else
        //    {
        //        var solicitation = new Solicitation { User = user, JobOffer = jobOffer, Date = DateTime.Now };
        //        _context.Add(solicitation);
        //        await _context.SaveChangesAsync();
        //        return Redirect("/JobOffers/Details/" + jobOffer.Id);
        //    }
        //}

        // Posts: api/Solicitations/4
        [HttpPost("{jobOfferId}")]
        public async Task<ActionResult> PostSolicitateAsync(int jobOfferId)
        {
            if (_context.Solicitation == null)
            {
                return NotFound("Database not connected");
            }

            var user = await _userManager.GetUserAsync(User);
            var jobOffer = await _context.JobOffer.FindAsync(jobOfferId);
            if (user == null)
            {
                return NotFound("user doesn't exist");
            }
            if (jobOffer == null)
            {
                return NotFound("jobOffer doesn't exist");
            }
            var solicitation = new Solicitation { User = user, JobOffer = jobOffer, Date = DateTime.Now };
            _context.Add(solicitation);
            await _context.SaveChangesAsync();
            var newSolicitation = _mapper.Map<SolicitationDTOComplete>(solicitation);
            return Ok(newSolicitation);
        }

        // Delete: api/Solicitations
        [HttpDelete("{jobOfferId}")]
        public async Task<ActionResult> DeleteSolicitateAsync(int jobOfferId)
        {
            if (_context.Solicitation == null)
            {
                return NotFound("Database not connected");
            }

            var user = await _userManager.GetUserAsync(User);
            var jobOffer = await _context.JobOffer.FindAsync(jobOfferId);
            if (user == null)
            {
                return NotFound("user doesn't exist");
            }
            if (jobOffer == null)
            {
                return NotFound("jobOffer doesn't exist");
            }

            if (_context.Solicitation.Where(x => x.JobOffer == jobOffer && x.User == user).Any())
            {
                var solicitation = _context.Solicitation.Where(x => x.JobOffer == jobOffer && x.User == user).First();
                if (solicitation != null)
                {
                    _context.Solicitation.Remove(solicitation);
                }
                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return NotFound("User has not applied to this jobOffer");
            }
        }


        // Put: api/Solicitations/4
        [HttpPut("{id}")]
        public async Task<ActionResult> PutSolicitationSelectAsync(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized();
            }
            var solicitationEntity = await _context.Solicitation.FindAsync(id);
            if (solicitationEntity == null)
            {
                Problem("Entity set 'ApplicationDbContext.Solicitation'  is null.");
            }
            if (solicitationEntity.Selected == true)
            {
                solicitationEntity.Selected = false;
                await _context.SaveChangesAsync();
            }
            else
            {
                solicitationEntity.Selected = true;
                await _context.SaveChangesAsync();
            }
            var updatedSolicitation = _mapper.Map<SolicitationDTOSelect>(solicitationEntity);
            return Ok(updatedSolicitation);
        }
    }
}
