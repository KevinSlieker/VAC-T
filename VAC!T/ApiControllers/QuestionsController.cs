using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
                var newQuestion = _mapper.Map<QuestionDTOId>(questionEntity);
                int id = newQuestion.Id;
                return CreatedAtAction(nameof(GetQuestionByIdAsync), new { id }, newQuestion);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // GET: api/Questions/YesOrNo
        [HttpGet("YesOrNo")]
        public async Task<ActionResult<List<string>?>> GetYesOrNoQuestionsAsync()
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var textOptions = await _service.GetYesOrNoTextOptionsAsync();
                if (textOptions.IsNullOrEmpty())
                {
                    return NotFound("No questionText options found.");
                }
                return Ok(textOptions);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // POST: api/Questions/YesOrNo
        [HttpPost("YesOrNo")]
        public async Task<ActionResult> PostYesOrNoAsync([FromBody] QuestionDTOForCreateYesOrNo question)
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
                questionEntity = await _service.CreateYesOrNoQuestionAsync(questionEntity);
                var newQuestion = _mapper.Map<QuestionDTOId>(questionEntity);
                int id = newQuestion.Id;
                return CreatedAtAction(nameof(GetQuestionByIdAsync), new { id }, newQuestion);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // POST: api/Questions/5/Options
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
                int id = questionId;
                return CreatedAtAction(nameof(GetQuestionByIdAsync), new { id });
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }
    }
}
