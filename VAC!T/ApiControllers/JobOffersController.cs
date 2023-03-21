using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Data;
using VAC_T.Data.DTO;
using VAC_T.Models;

namespace VAC_T.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobOffersController : Controller
    {
        private readonly UserManager<VAC_TUser> _userManager;
        private readonly IMapper _mapper;
        private readonly JobOfferService _service;

        public JobOffersController(UserManager<VAC_TUser> userManager, IMapper mapper, JobOfferService service)
        {
            _userManager = userManager;
            _mapper = mapper;
            _service = service;
        }

        // GET: api/JobOffers
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<JobOfferDTO>>> GetAllJobOffersAsync()
        {
            try
            {
                var jobOffers = await _service.GetJobOffersAsync(User);
                var result = _mapper.Map<List<JobOfferDTO>>(jobOffers);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }

            //if (User.IsInRole("ROLE_EMPLOYER") && _signInManager.IsSignedIn(User))
            //{
            //    var user = await _userManager.GetUserAsync(User);
            //    jobOffers = jobOffers.Where(C => C.Company.User == user);
            //}
            //var result = await _mapper.ProjectTo<JobOfferDTO>(jobOffers).ToListAsync();
            //return Ok(result);
        }

        // GET: api/JobOffers/5
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [AllowAnonymous]
        public async Task<ActionResult<JobOfferDTO>> GetJobOfferByIdAsync(int id)
        {
            try
            {
                var jobOffer = await _service.GetJobOfferAsync(id, User);
                if (jobOffer == null)
                {
                    return NotFound();
                }

                var result = _mapper.Map<JobOfferDTO>(jobOffer);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // POST: api/JobOffers

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> PostAsync([FromBody] JobOfferDTOForCreateTemp jobOffer) // JobOfferDTOForUpdateAndCreate
        {
            if (!User.IsInRole("ROLE_EMPLOYER"))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                var jobOfferEntity = _mapper.Map<JobOffer>(jobOffer);
                jobOfferEntity = await _service.CreateJobOfferAsync(jobOfferEntity, User);
                var newJobOffer = _mapper.Map<JobOfferDTO>(jobOfferEntity);
                return CreatedAtAction(nameof(GetJobOfferByIdAsync), new { id = newJobOffer.Id }, newJobOffer);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }


        // Put: api/JobOffers/5
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> PutAsync(int id, [FromBody] JobOfferDTOForUpdateAndCreate jobOffer)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            if (id != jobOffer.Id)
            {
                ModelState.AddModelError("Id", "Does not match Id in URL");
                return BadRequest(ModelState);
            }
            try
            {
                var jobOfferEntity = await _service.GetJobOfferAsync(id, User);
                if (jobOfferEntity == null)
                {
                    return NotFound($"No jobOffer with id: {id} in the database");
                }

                _mapper.Map(jobOffer, jobOfferEntity);

                await _service.UpdateJobOfferAsync(jobOfferEntity);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
            return NoContent();
        }

        // Delete: api/JobOffers/5
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> DeleteJobOfferAsync(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                if (!await _service.DoesJobOfferExistsAsync(id))
                {
                    return NotFound();
                }

                await _service.DeleteJobOfferAsync(id);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }
    }
}
