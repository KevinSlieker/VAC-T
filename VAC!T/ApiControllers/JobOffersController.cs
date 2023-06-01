using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Data.DTO;
using VAC_T.Models;

namespace VAC_T.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class JobOffersController : Controller
    {
        private readonly IMapper _mapper;
        private readonly JobOfferService _service;

        public JobOffersController(IMapper mapper, JobOfferService service)
        {
            _mapper = mapper;
            _service = service;
        }

        // GET: api/JobOffers
        /// <summary>
        /// Gets all jobOffers the logged in user is allowed to view.
        /// </summary>
        /// <returns>All jobOffers available</returns>
        /// <remarks>
        /// Employers can only see jobOffers that are connected to their company.
        /// </remarks>
        [HttpGet]
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
        }

        // GET: api/JobOffers/5
        /// <summary>
        /// Get a jobOffer by id.
        /// </summary>
        /// <param name="id">The id of the jobOffer to find</param>
        /// <returns>The jobOffer</returns>
        /// <remarks>
        /// Candidates and admins will also get the solicitation and answers returned if they exist.
        /// </remarks>
        [HttpGet("{id}")]
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
        /// <summary>
        /// Create a jobOffer
        /// </summary>
        /// <param name="jobOffer">The information of the jobOffer to be created</param>
        /// <returns>The created jobOffer</returns>
        /// <remarks>
        /// The jobOffer will belong to the company the logged in user belongs to. Only Employers can create jobOffers.
        /// 
        /// 
        /// Sample request:
        /// 
        ///     POST /api/JobOffer
        ///     {
        ///         "name": "Test Api",
        ///         "description": "TESTEN1233456458",
        ///         "logoURL": "assets/img/job_offer/csharp.png",
        ///         "level": "Test",
        ///         "residence": "Sittard"
        ///     }
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] JobOfferDTOForUpdateAndCreate jobOffer)
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
        /// <summary>
        /// Update a jobOffer
        /// </summary>
        /// <param name="id">Id of the jobOffer to be updated</param>
        /// <param name="jobOffer">The information of the jobOffer to be updated</param>
        /// <returns>No content</returns>
        /// <remarks>
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/JobOffer/5
        ///     {
        ///         "id": 5,
        ///         "name": "Test Api",
        ///         "description": "TESTEN1233456458",
        ///         "logoURL": "assets/img/job_offer/csharp.png",
        ///         "level": "Test",
        ///         "residence": "Sittard"
        ///     }
        /// </remarks>
        [HttpPut("{id}")]
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
        /// <summary>
        /// Deletes a jobOffer by id
        /// </summary>
        /// <param name="id">The id of the jobOffer to be deleted</param>
        /// <returns>Ok</returns>
        [HttpDelete("{id}")]
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

        // PUT: api/JobOffers/status/
        /// <summary>
        /// Changes the status of a jobOffer
        /// </summary>
        /// <param name="id">The id of the jobOffer</param>
        /// <returns>Ok</returns>
        /// <remarks>
        /// The value for closed will be become null if there is already a date there or it becomes the current date if closed was null.
        /// </remarks>
        [HttpPut("Status/{id}")]
        public async Task<ActionResult> PutStatusAsync(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                await _service.ChangeJobOfferStatusAsync(id);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // GET: api/JobOffers/Questions
        /// <summary>
        /// Get all JobOffers that have questions
        /// </summary>
        /// <returns>All JobOffers that have questions</returns>
        /// <remarks>
        /// The information over the jobOffers is small. The questions are not included. This is a list of jobOffers that have set their questions.
        /// </remarks>
        [HttpGet("Questions")]
        public async Task<ActionResult> GetAllJobOfferWQuestionsAsync()
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                var jobOffer = await _service.GetAllJobOffersWQuestionsAsync(User);
                var result = _mapper.Map<List<JobOfferDTOSmall>>(jobOffer);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // GET: api/JobOffers/Questions/5
        /// <summary>
        /// Get a JobOffer with the questions
        /// </summary>
        /// <param name="id">The id of the jobOffer</param>
        /// <returns>Ok</returns>
        /// <remarks>
        /// The options for the questions are not included.
        /// </remarks>
        [HttpGet("Questions/{id}")]
        public async Task<ActionResult> GetJobOfferWQuestionsAsync(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                var jobOffer = await _service.GetJobOfferWQuestionsAsync(id);
                var result = _mapper.Map<JobOfferDTOWQuestions>(jobOffer);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }


        // PUT: api/JobOffers/Questions/18
        /// <summary>
        /// Selects what questions are linked to the jobOffer.
        /// </summary>
        /// <param name="id">The id of the jobOffer</param>
        /// <param name="selectedQuestions">The ids of the selected questions</param>
        /// <returns>The jobOffer with questions</returns>
        /// <remarks>
        /// The options for the questions are not included.
        /// 
        /// 
        /// QuestionIds: this is an array of the ids of the to be selected questions.
        /// 
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/Questions/18
        ///     {
        ///         "id": 18,
        ///         "questionIds": [6,1]
        ///     }
        /// </remarks>
        [HttpPut("Questions/{id}")]
        public async Task<IActionResult> PutQuestionsForJobOfferAsync(int id, [FromBody] JobOfferDTOSelectQuestions selectedQuestions)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            if (id != selectedQuestions.Id)
            {
                ModelState.AddModelError("Id", "Does not match Id in URL");
                return BadRequest(ModelState);
            }
            try
            {
                if (!await _service.DoesJobOfferExistsAsync(id))
                {
                    return NotFound($"No jobOffer with id: {id} in the database");
                }
                await _service.SelectJobOfferQuestionsAsync(id, selectedQuestions.QuestionIds);

                var jobOfferEntity = await _service.GetJobOfferWQuestionsAsync(id);
                var result = _mapper.Map<JobOfferDTOWQuestions>(jobOfferEntity);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

    }
}
