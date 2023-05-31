using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    public class QuestionsController : Controller
    {
        private readonly QuestionService _service;
        private readonly IMapper _mapper;
        public QuestionsController(QuestionService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        // GET: api/Questions
        /// <summary>
        /// Gets all questions.
        /// </summary>
        /// <returns>All questions available</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionDTOMedium>>> GetAllQuestionsAsync()
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var questions = await _service.GetQuestionsAsync(User);
                var result = _mapper.Map<List<QuestionDTOMedium>>(questions);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // GET: api/Questions/7
        /// <summary>
        /// Gets a question by Id.
        /// </summary>
        /// <param name="id">The id of the question</param>
        /// <returns>All questions available</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionDTOComplete>> GetQuestionByIdAsync(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var question = await _service.GetQuestionAsync(id, User);
                if (question == null)
                {
                    return NotFound("The question does not exist or the question does not belong to your company.");
                }
                var result = _mapper.Map<QuestionDTOComplete>(question);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // POST: api/Questions
        /// <summary>
        /// Creates a question.
        /// </summary>
        /// <param name="question"></param>
        /// <returns>The created question</returns>
        /// <remarks>
        /// type: The type of question. Opties are: "Open", "Meerkeuze", "Standpunt" or "Ja/Nee".
        /// 
        /// explanationType: The options for the explanation: "Nooit zichtbaar", "Altijd zichtbaar" or "Laatste optie".
        /// 
        /// optionsAmount: When creating/adding options this will be the amount that will be created/added when you would press create/add.
        /// 
        /// multipleOptions, optionsAmount and explanationType are only important for the type: "Meerkeuze".
        /// 
        /// 
        /// Sample request:
        ///
        ///     POST /api/Questions
        ///     {
        ///        "type": "Meerkeuze",
        ///        "questionText": "Wat is 1+1?",
        ///        "companyId": 6,
        ///        "multipleOptions": false,
        ///        "explanationType": "Laatste optie",
        ///        "optionsAmount": 4
        ///     }
        ///
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] QuestionDTOForCreate question)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var questionEntity = _mapper.Map<Question>(question);
                if (User.IsInRole("ROLE_EMPLOYER"))
                {
                    questionEntity.CompanyId = (await _service.GetCompanyAsync(User)).Id;
                }
                questionEntity = await _service.CreateQuestionAsync(questionEntity);
                var newQuestion = _mapper.Map<QuestionDTOComplete>(questionEntity);
                int id = newQuestion.Id;
                return CreatedAtAction(nameof(GetQuestionByIdAsync), new { id }, newQuestion);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        //// GET: api/Questions/YesOrNo
        //[HttpGet("YesOrNo")]
        //public async Task<ActionResult<List<string>?>> GetYesOrNoQuestionsAsync()
        //{
        //    if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
        //    {
        //        return Unauthorized("Unauthorized");
        //    }
        //    try
        //    {
        //        var textOptions = await _service.GetYesOrNoTextOptionsAsync();
        //        if (textOptions.IsNullOrEmpty())
        //        {
        //            return NotFound("No questionText options found.");
        //        }
        //        return Ok(textOptions);
        //    }
        //    catch (InternalServerException)
        //    {
        //        return Problem("Database not connected");
        //    }
        //}

        //// POST: api/Questions/YesOrNo
        //[HttpPost("YesOrNo")]
        //public async Task<ActionResult> PostYesOrNoAsync([FromBody] QuestionDTOForCreateYesOrNo question)
        //{
        //    if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
        //    {
        //        return Unauthorized("Unauthorized");
        //    }
        //    try
        //    {
        //        var questionEntity = _mapper.Map<Question>(question);
        //        if (User.IsInRole("ROLE_EMPLOYER"))
        //        {
        //            questionEntity.CompanyId = (await _service.GetCompanyAsync(User)).Id;
        //        }
        //        questionEntity = await _service.CreateYesOrNoQuestionAsync(questionEntity);
        //        var newQuestion = _mapper.Map<QuestionDTOId>(questionEntity);
        //        int id = newQuestion.Id;
        //        return CreatedAtAction(nameof(GetQuestionByIdAsync), new { id }, newQuestion);
        //    }
        //    catch (InternalServerException)
        //    {
        //        return Problem("Database not connected");
        //    }
        //}

        // POST: api/Questions/5/Options
        /// <summary>
        /// Creates questionOpties for the question of the given id.
        /// </summary>
        /// <param name="questionId">The id of the question the options belong to</param>
        /// <param name="options">The details of the options to be created</param>
        /// <returns>The created question with options</returns>
        /// <remarks>
        /// optionShort: The not required short option that the employer will see if it is not null
        /// 
        /// optionLong: The option that the candidate will see and possibly the employer if optionShort is left null.
        /// 
        /// 
        /// Sample request:
        ///
        ///     POST /api/Questions/5/Options
        ///     [
        ///         {
        ///            "optionShort": null,
        ///            "optionLong": "Yes"
        ///         },
        ///         {
        ///            "optionShort": "No",
        ///            "optionLong": "The answer to the question is no."
        ///         }
        ///     ]
        ///
        /// </remarks>
        [HttpPost("{questionId}/Options")]
        public async Task<ActionResult> PostOptionsAsync(int questionId, [FromBody] List<QuestionOptionDTOSmall> options)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var question = await _service.GetQuestionAsync(questionId, User);
                if (question == null)
                {
                    return NotFound("The question does not exist or the question does not belong to your company.");
                }
                var questionOptionsEntity = _mapper.Map<List<QuestionOption>>(options);
                foreach (var option in questionOptionsEntity) 
                {
                    option.QuestionId = questionId;
                    option.Question = question;
                }
                questionOptionsEntity = await _service.CreateQuestionOptionsAsync(questionOptionsEntity);
                question = await _service.GetQuestionAsync(questionId, User);
                var questionWOptions = _mapper.Map<QuestionDTOComplete>(question);
                int id = questionId;
                return CreatedAtAction(nameof(GetQuestionByIdAsync), new { id }, questionWOptions);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // PUT: api/Questions/26
        /// <summary>
        /// Updates a question.
        /// </summary>
        /// <param name="id">The id of the question</param>
        /// <param name="question">The values of the question</param>
        /// <returns>No content</returns>
        /// <remarks>
        /// type: The type of question. Opties are: "Open", "Meerkeuze", "Standpunt" or "Ja/Nee".
        /// Changing the type deletes the question options if they exist.
        /// 
        /// explanationType: The options for the explanation: "Nooit zichtbaar", "Altijd zichtbaar" or "Laatste optie".
        /// 
        /// optionsAmount: When creating/adding options this will be the amount that will be created/added when you would press create/add.
        /// 
        /// multipleOptions, optionsAmount and explanationType are only important for the type: "Meerkeuze".
        /// 
        /// 
        /// Sample request:
        ///
        ///     PUT /api/Questions/26
        ///     {
        ///        "id": 26,
        ///        "type": "Meerkeuze",
        ///        "questionText": "Wat is 1+1+21?",
        ///        "companyId": 6,
        ///        "multipleOptions": false,
        ///        "explanationType": "Nooit zichtbaar",
        ///        "optionsAmount": 3
        ///     }
        ///
        /// </remarks>
        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(int id, [FromBody] QuestionDTOForCreate question)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            if (id != question.Id)
            {
                ModelState.AddModelError("Id", "Does not match Id in URL");
                return BadRequest(ModelState);
            }
            try
            {
                var questionEntity = await _service.GetQuestionAsync(question.Id, User);
                if (questionEntity == null)
                {
                    return NotFound("The question does not exist or the question does not belong to your company.");
                }
                _mapper.Map(question, questionEntity);
                if (User.IsInRole("ROLE_EMPLOYER"))
                {
                    questionEntity.CompanyId = (await _service.GetCompanyAsync(User)).Id;
                }
                await _service.UpdateQuestionAsync(questionEntity);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
            return NoContent();
        }

        // PUT: api/Questions/Options/57
        /// <summary>
        /// Updates the questionOption of the given id.
        /// </summary>
        /// <param name="id">The id of the questionOption</param>
        /// <param name="questionOption">The details of the questionOption to be updated</param>
        /// <returns>No Content</returns>
        /// <remarks>
        /// optionShort: The not required short option that the employer will see if it is not null
        /// 
        /// optionLong: The option that the candidate will see and possibly the employer if optionShort is left null.
        /// 
        /// 
        /// Sample request:
        ///
        ///     PUT /api/Questions/Options/57
        ///     {
        ///        "id": 57,
        ///        "optionShort": "No",
        ///        "optionLong": "The answer to the question is: NO."
        ///     }
        ///
        /// </remarks>
        [HttpPut("Options/{id}")]
        public async Task<ActionResult> PutOptionAsync(int id, [FromBody] QuestionOptionDTOSmall questionOption)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            if (id != questionOption.Id)
            {
                ModelState.AddModelError("Id", "Does not match Id in URL");
                return BadRequest(ModelState);
            }
            try
            {
                var questionOptionEntity = await _service.GetQuestionOptionAsync(id, User);
                if (questionOptionEntity == null)
                {
                    return NotFound("The question does not exist or the question does not belong to your company.");
                }
                _mapper.Map(questionOption, questionOptionEntity);
                await _service.UpdateQuestionOptionAsync(questionOptionEntity);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
            return NoContent();
        }



        // DELETE: api/Questions/27
        /// <summary>
        /// Deletes the question of the given id.
        /// </summary>
        /// <param name="id">The id of the question</param>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                if (!await _service.DoesQuestionExistAsync(id, User))
                {
                    return NotFound($"No question with Id: {id} in the database or the question does not belong to your company.");
                }
                await _service.DeleteQuestionAsync(id);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // DELETE: api/Questions/Options/55
        /// <summary>
        /// Deletes the questionOption of the given id.
        /// </summary>
        /// <param name="id">The id of the questionOption</param>
        /// <remarks>
        /// Only questionOptions where the question is of type: "Meerkeuze" can be deleted. Other Options will be deleted if the question itself is deleted
        /// or if the questionType is changed.
        /// </remarks>
        [HttpDelete("Options/{id}")]
        public async Task<ActionResult> DeleteOptionAsync(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                if (!await _service.DoesQuestionOptionExistAsync(id, User))
                {
                    return NotFound($"No question with Id: {id} in the database or the question does not belong to your company.");
                }
                await _service.DeleteQuestionOptionAsync(id);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }
    }
}
