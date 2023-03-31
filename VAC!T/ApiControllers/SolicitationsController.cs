using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Data.DTO;

namespace VAC_T.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SolicitationsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly SolicitationService _service;

        public SolicitationsController(IMapper mapper, SolicitationService service)
        {
            _mapper = mapper;
            _service = service;
        }

        // GET: api/Solicitations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SolicitationDTOComplete>>> GetAllSolicitationsAsync([FromQuery] string? searchJobOffer,
            [FromQuery] string? searchCompany, [FromQuery] string? searchCandidate, [FromQuery] bool? searchSelectedYes, [FromQuery] bool? searchSelectedNo)
        {
            try
            {
                var solicitations = await _service.GetSolicitationsAsync(User, searchJobOffer, searchCompany, searchCandidate, searchSelectedYes, searchSelectedNo);
                var result = _mapper.Map<List<SolicitationDTOComplete>>(solicitations);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }

        }

        // Post: api/Solicitations/4
        [HttpPost("{jobOfferId}")]
        public async Task<ActionResult> PostSolicitateAsync(int jobOfferId)
        {
            if (!User.IsInRole("ROLE_CANDIDATE"))
            {
                return Unauthorized("You need to be be a candidate to solicitate");
            }
            try
            {
                if (!await _service.DoesJobOfferExistsAsync(jobOfferId))
                {
                    return NotFound($"JobOffer with Id: {jobOfferId} does not exist.");
                }
                var solicitation = await _service.CreateSolicitationAsync(jobOfferId, User);
                if (solicitation == null)
                {
                    return BadRequest("You have already solicitated for this jobOffer.");
                }
                var newSolicitation = _mapper.Map<SolicitationDTOComplete>(solicitation);
                return Ok(newSolicitation);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // Delete: api/Solicitations/5
        [HttpDelete("{jobOfferId}")]
        public async Task<ActionResult> DeleteSolicitateAsync(int jobOfferId)
        {
            if (!User.IsInRole("ROLE_CANDIDATE"))
            {
                return Unauthorized("You need to be be a candidate to cancel your solicitation");
            }
            try
            {
                if (!await _service.DoesJobOfferExistsAsync(jobOfferId))
                {
                    return NotFound($"JobOffer with Id: {jobOfferId} does not exist.");
                }
                if (!await _service.DoesSolicitationExistWithJobOfferIdAsync(jobOfferId, User))
                {
                    return NotFound("Solicitation does not exist.");
                }
                await _service.DeleteSolicitationAsync(jobOfferId, User);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }


        // Put: api/Solicitations/4
        [HttpPut("{id}")]
        public async Task<ActionResult> PutSolicitationSelectAsync(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                if (! await _service.DoesSolicitationExistsAsync(id))
                {
                    return NotFound($"No solicitation with Id: {id} in the database");
                }
                await _service.SelectSolicitationAsync(id);

                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
            //var updatedSolicitation = _mapper.Map<SolicitationDTOSelect>(solicitationEntity);
            //return Ok(updatedSolicitation);
        }
    }
}
