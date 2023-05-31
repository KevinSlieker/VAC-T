using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Data.DTO;
using VAC_T.Models;

namespace VAC_T.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AnswersController : Controller
    {
        private readonly AnswerService _service;
        private readonly IMapper _mapper;
        public AnswersController(AnswerService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        // GET: api/Answers
        /// <summary>
        /// Gets 1 random answer per user per jobOffer.
        /// </summary>
        /// <returns>All answers available</returns>
        /// <remarks>
        /// This function is made to return what user answered the questions for what jobOffer. Only one answer is returned per user per jobOffer.
        /// </remarks>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnswerDTOMedium>>> GetAnswersAsync()
        {
            try
            {
                var answers = await _service.GetAnswersAsync(User);
                var result = _mapper.Map<List<AnswerDTOMedium>>(answers);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // GET: api/Answers/27/74d39ecc-0f5b-4e8c-aad1-f5esf94794ee
        /// <summary>
        /// Gets the answers from a user for a jobOffer.
        /// </summary>
        /// <param name="jobOfferId">The id of the jobOffer you want to find</param>
        /// <param name="userId">The id of the user you want to find</param>
        /// <returns>The answers for a user for a jobOffer if you are allowed to view it.</returns>
        [HttpGet("{jobOfferId}/{userId}")]
        public async Task<ActionResult<AnswerDTOExtended>> GetAnswersByJobOfferIdByUserIdAsync(int jobOfferId, string userId)
        {
            try
            {
                var answers = await _service.GetAnswersForJobOfferAsync(jobOfferId, userId, User);
                if (answers.IsNullOrEmpty())
                {
                    return Unauthorized("Unauthorized, you are not allowed to view the details of this jobOffer or user");
                }
                var viewModel = _mapper.Map<List<AnswerViewModel>>(answers);
                foreach (var answer in viewModel)
                {
                    answer.DisplayAnswerText = _service.PrepareAnswerForDisplay(answer.AnswerText, answer.Question);
                }

                var result = _mapper.Map<List<AnswerDTOExtended>>(viewModel);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // POST: api/Answers/7
        /// <summary>
        /// Creates all answers for the question of the joboffer.
        /// </summary>
        /// <param name="jobOfferId">The id of the jobber that the answers belong too</param>
        /// <param name="answers">The input to create the answers</param>
        /// <returns>The created answers</returns>
        /// <remarks>
        /// BEWARE: You need to answer all the questions at once. Otherwise you need delete the answers and create them again, complete this time.
        /// 
        /// 
        /// questiondId: The id of the question the answer belongs too.
        /// 
        /// 
        /// multipleChoiceAnswers: An array of strings that can contain the ids of the options and "Anders". This is only needed if
        /// the question is multipleChoice. If the questionType is not "Meerkeuze"(multiple choice) you can leave this empty: [].
        /// 
        /// 
        /// answerText: For "Ja/Nee"questions you put "Ja"or "Nee" here. For "Standpunt" questions you put a number(as a string) ranging from 1 to 100.
        /// Where 1 is the first option and 100 is the last option. For "Open" questions you just write your answer here. For "Meerkeuze" questions
        /// you can leave this an empty string: "".
        /// 
        /// 
        /// explanation: For "Meerkeuze" questions you put your possible explanation here if it is allowed/selected. It can be left as an empty string.
        /// 
        /// 
        /// 
        /// Sample request:
        ///
        ///     POST /api/Answers/7
        ///     [
        ///         {
        ///                "questionId": 1,
        ///                "multipleChoiceAnswers": ["3", "Anders"],
        ///                "answerText": "",
        ///                "explanation": "Testing of this works"
        ///         },
        ///         {
        ///                "questionId": 6,
        ///                "multipleChoiceAnswers": [],
        ///                "answerText": "Ja",
        ///                "explanation": ""
        ///         }
        ///     ]
        ///
        /// </remarks>
        [HttpPost("{jobOfferId}")]
        public async Task<ActionResult> PostAsync(int jobOfferId, [FromBody] List<AnswerDTOForCreate> answers)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                if (await _service.DoAnswersExistAsync(jobOfferId, User))
                {
                    return BadRequest("You already answered the questions");
                }
                var answersEntity = await _service.PrepareUserAnswersForCreateAsync(jobOfferId, User);
                if (answersEntity == null)
                {
                    return NotFound("No questions are made yet");
                }
                var answersIntermediate = _mapper.Map<List<AnswerViewModel>>(answersEntity);
                var inputAnswers = _mapper.Map<List<AnswerViewModel>>(answers);
                foreach (var answer in inputAnswers)
                {
                    var index = answersIntermediate.FindIndex(a => a.QuestionId == answer.QuestionId);
                    answersIntermediate[index].AnswerText = answer.AnswerText;
                    answersIntermediate[index].MultipleChoiceAnswers = answer.MultipleChoiceAnswers;
                    answersIntermediate[index].Explanation = answer.Explanation;
                }
                _mapper.Map(answersIntermediate, answersEntity);


                var createdAnswers = await _service.CreateAnswersAsync(answersEntity);
                var answersToReturn = _mapper.Map<List<AnswerDTOExtended>>(createdAnswers);
                foreach (var answer in answersToReturn)
                {

                    answer.DisplayAnswerText = _service.PrepareAnswerForDisplay(answer.AnswerText, (await _service.GetQuestionAsync(answer.QuestionId))!);
                }
                return CreatedAtAction(nameof(GetAnswersByJobOfferIdByUserIdAsync), new { jobOfferId, userId = answersToReturn.First().UserId }, answersToReturn);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // PUT: api/Answers/70
        /// <summary>
        /// Updates the answer of the given id.
        /// </summary>
        /// <param name="id">The id of the anser</param>
        /// <param name="answer">The details of the answer to be updated</param>
        /// <returns>No Content</returns>
        /// <remarks>
        /// multipleChoiceAnswers: An array of strings that can contain the ids of the options and "Anders". This is only needed if
        /// the question is multipleChoice. If the questionType is not "Meerkeuze"(multiple choice) you can leave this empty: [].
        /// 
        /// 
        /// answerText: For "Ja/Nee"questions you put "Ja"or "Nee" here. For "Standpunt" questions you put a number(as a string) ranging from 1 to 100.
        /// Where 1 is the first option and 100 is the last option. For "Open" questions you just write your answer here. For "Meerkeuze" questions
        /// you can leave this an empty string: "".
        /// 
        /// 
        /// explanation: For "Meerkeuze" questions you put your possible explanation here if it is allowed/selected. It can be left as an empty string.
        /// 
        /// 
        /// Sample request:
        ///
        ///     PUT /api/Answers/70
        ///     {
        ///        "id": 70,
        ///        "questionId": 1,
        ///        "multipleChoiceAnswers": ["2", "Anders"],
        ///        "answerText": "",
        ///        "explanation": "Testing of this works 2 for update"
        ///     }
        ///
        /// </remarks>
        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(int id, [FromBody] AnswerDTOForCreate answer)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            if (id != answer.Id)
            {
                ModelState.AddModelError("Id", "Does not match Id in URL");
                return BadRequest(ModelState);
            }
            try
            {
                var answerEntity = await _service.GetAnswerAsync(answer.Id, User);
                if (answerEntity == null)
                {
                    return NotFound("The answer does not exist or the answer does not belong to you.");
                }
                _mapper.Map(answer, answerEntity);
                await _service.UpdateAnswerAsync(answerEntity);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
            return NoContent();
        }


        // DELETE: api/Answers/7/74d39ecc-0f5b-4e8c-aad1-f5esf94794ee
        /// <summary>
        /// Deletes the answers of the given user(Id) of the given jobOffer(Id).
        /// </summary>
        /// <param name="jobOfferId">The id of the jobOffer</param>
        /// <param name="userId">The id of the user</param>
        /// <remarks>
        /// Answers can only be deleted if the user has not solicited for the according jobOffer.
        /// </remarks>
        [HttpDelete("{jobOfferId}/{userId}")]
        public async Task<ActionResult> DeleteAsync(int jobOfferId, string userId)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                if (!await _service.DoAnswersExistAsync(jobOfferId, userId, User))
                {
                    return NotFound("The answers do not exist or you are not allowed to delete the answers");
                }
                if (await _service.CheckUserSolicitatedAsync(jobOfferId, userId))
                {
                    return BadRequest("You can't delete your answers while you solicited, you need to cancel your solicitation first");
                }
                await _service.DeleteUserAnswersForJobOfferAsync(jobOfferId, userId);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // DELETE: api/Answers/7/74d39ecc-0f5b-4e8c-aad1-f5esf94794ee
        /// <summary>
        /// Returns the CSV file for the answers and questions of the given user(Id) of the given jobOffer(Id).
        /// </summary>
        /// <param name="jobOfferId">The id of the jobOffer</param>
        /// <param name="userId">The id of the user</param>
        /// <remarks>
        /// Answers can only be deleted if the user has not solicited for the according jobOffer.
        /// </remarks>
        [HttpGet("CSV/{jobOfferId}/{userId}")]
        public async Task<ActionResult> GetCSVasync(int jobOfferId, string userId)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var answers = await _service.GetAnswersForJobOfferAsync(jobOfferId, userId, User);
                if (answers.IsNullOrEmpty())
                {
                    return Unauthorized("Unauthorized, you are not allowed to view the details of this jobOffer or user");
                }
                var fileName = answers.First().JobOfferId.ToString() + "_" + answers.First().User.Name + ".csv";
                var ms = await _service.CreateCSVFileAsync(answers);
                var file = File(ms.ToArray(), "text/csv", fileName);
                ms.Close();
                return file;
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }
    }
}
