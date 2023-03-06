using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VAC_T.Data;
using VAC_T.Models;
using VAC_T.Data.DTO;

namespace VAC_T.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobOffersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<VAC_TUser> _userManager;
        private readonly SignInManager<VAC_TUser> _signInManager;
        private readonly IMapper _mapper;

        public JobOffersController(ApplicationDbContext context, UserManager<VAC_TUser> userManager, SignInManager<VAC_TUser> signInManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        // GET: JobOffers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobOfferDTO>>> GetAllJobOffersAsync()
        {
            if (_context.JobOffer != null)
            {
                NotFound("Database not connected");
            }
            var jobOffers = _context.JobOffer.Include(j => j.Company);
            var result = await _mapper.ProjectTo<JobOfferDTO>(jobOffers).ToListAsync();
            return Ok(result);
            //if (User.IsInRole("ROLE_EMPLOYER") && _signInManager.IsSignedIn(User))
            //{
            //    var user = await _userManager.GetUserAsync(User);
            //    return _context.JobOffer != null ?
            //                View(await _context.JobOffer.Include(j => j.Company).Where(C => C.Company.User == user).ToListAsync()) :
            //                Problem("Entity set 'ApplicationDbContext.JobOffer'  is null.");
            //}
            //else
            //{
            //    return _context.JobOffer != null ?
            //                View(await _context.JobOffer.Include(j => j.Company).ToListAsync()) :
            //                Problem("Entity set 'ApplicationDbContext.JobOffer'  is null.");
            //}
        }

        // GET: JobOffers/Details/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JobOffer>> GetJobOfferByIdAsync(int? id)
        {
            if (id == null || _context.JobOffer == null)
            {
                return NotFound();
            }
            //var user = await _userManager.GetUserAsync(User);
            // .Include(j => j.Solicitations.Where(x => x.User == user))
            // .Include(j => j.Company.JobOffers)
            var jobOffer = await _context.JobOffer
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jobOffer == null)
            {
                return NotFound();
            }

            return jobOffer;
        }

        // GET: JobOffers/Create
        //public async Task<IActionResult> Create()
        //{
        //    var jobOffer = new JobOffer();
        //    var user = await _userManager.GetUserAsync(User);
        //    var company = _context.Company.Where(x => x.User == user).First();
        //    jobOffer.CompanyId = company.Id;
        //    jobOffer.Residence = company.Residence;
        //    return View(jobOffer);
        //}

        // POST: JobOffers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> CreateAsync([FromBody] JobOffer jobOffer)
        {
            if (_context.JobOffer == null)
            { 
                return NotFound();
            }
            ModelState.Remove("Company");
            jobOffer.Company = await _context.Company.FindAsync(jobOffer.CompanyId);
            _context.Add(jobOffer);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetJobOfferByIdAsync), new {id = jobOffer.Id}, null);
        }

        // GET: JobOffers/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _context.JobOffer == null)
        //    {
        //        return NotFound();
        //    }

        //    var jobOffer = await _context.JobOffer.Include(j => j.Company).FirstOrDefaultAsync(x => x.Id == id);
        //    if (jobOffer == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(jobOffer);
        //}

        // POST: JobOffers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(int id, [FromBody] JobOffer jobOffer)
        {
            if (_context.JobOffer == null)
            { 
                return NotFound();
            }
            if (id != jobOffer.Id)
            {
                ModelState.AddModelError("Id", "Does not match Id in URL");
                return BadRequest(ModelState);
            }
            ModelState.Remove("Company");
            jobOffer.Company = await _context.Company.FindAsync(jobOffer.CompanyId);
            _context.Update(jobOffer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: JobOffers/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.JobOffer == null)
        //    {
        //        return NotFound();
        //    }

        //    var jobOffer = await _context.JobOffer
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (jobOffer == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(jobOffer);
        //}

        // POST: JobOffers/Delete/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            if (_context.JobOffer == null)
            {
                return Problem("Entity set 'ApplicationDbContext.JobOffer'  is null.");
            }
            var jobOffer = await _context.JobOffer.FindAsync(id);
            if (jobOffer == null)
            { 
                return NotFound();
            }
            _context.JobOffer.Remove(jobOffer);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool JobOfferExists(int id)
        {
            return (_context.JobOffer?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
