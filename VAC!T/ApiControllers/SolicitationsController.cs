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
        /// <summary>
        /// Gets all solicitations
        /// </summary>
        /// <param name="searchJobOffer">(optional)filter for jobOffer name</param>
        /// <param name="searchCompany">(optional)filter for company name</param>
        /// <param name="searchCandidate">(optional)filter for candidate name</param>
        /// <param name="searchSelectedYes">(optional)filter for if the candidate is selected</param>
        /// <param name="searchSelectedNo">(optional)filter for if the candidate is NOT selected</param>
        /// <returns>A list of solicitations</returns>
        /// <remarks>
        /// Depending on the roles of the logged in user. A different selection of solicitations are returned.
        /// </remarks>
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
        /// <summary>
        /// Create a solicitation / solicitate
        /// </summary>
        /// <param name="jobOfferId">The id of the jobOffer the solicitation is about</param>
        /// <returns>The created solicitation</returns>
        /// <remarks>
        /// Only candidates are allowed to solicitate.
        /// 
        /// To solicitate the user needs to have answered the questions.
        /// </remarks>
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
                if (!await _service.AreQuestionsAnsweredAsync(jobOfferId, User))
                {
                    return BadRequest("You have not answered all/any interviewQuestions for this jobOffer.");
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
        /// <summary>
        /// Deletes a solicitation
        /// </summary>
        /// <param name="jobOfferId">The id of the jobOffer the solicitation belongs to</param>
        /// <returns>A list of solicitations</returns>
        /// <remarks>
        /// Only candidates themself can delete their solicitation. Employers and admins can only decide to not select them.
        /// </remarks>
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
        /// <summary>
        /// Updates the value for selected for a solicitation
        /// </summary>
        /// <param name="id">The id of the solicitation to be updated for select</param>
        /// <returns>Ok</returns>
        /// <remarks>
        /// Changes the value of selected from false to true or true to false.
        /// </remarks>
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
