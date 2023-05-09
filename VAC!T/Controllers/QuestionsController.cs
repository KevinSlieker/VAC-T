using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Models;

namespace VAC_T.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly QuestionService _service;
        private readonly IMapper _mapper;
        public QuestionsController(QuestionService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        // GET: Questions
        public async Task<IActionResult> Index()
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var questions = await _service.GetQuestionsAsync(User);
                return View(questions);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Question' is null.");
            }
        }

        // GET: Questions/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var questions = await _service.GetQuestionAsync(id, User);
                if (questions == null)
                {
                    return NotFound("The question does not exist or the question does not belong to your company.");
                }
                return View(questions);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Question' is null.");
            }
        }

        // GET: Questions/Create
        public async Task<IActionResult> Create()
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var question = new Question();
                ViewData["ExplanationType"] = new SelectList(new List<string>() { "Nooit zichtbaar", "Altijd zichtbaar", "Laatste optie" });
                if (User.IsInRole("ROLE_ADMIN"))
                {
                    ViewData["CompanyId"] = new SelectList(await _service.GetCompaniesAsync(), "Id", "Name");
                    ViewData["Type"] = new SelectList(new List<string>() { "Open", "Meerkeuze", "Standpunt", "Ja/Nee" });
                    return View();
                }
                else
                {
                    question.CompanyId = (await _service.GetCompanyAsync(User)).Id;
                    ViewData["Type"] = new SelectList(new List<string>() { "Open", "Meerkeuze", "Standpunt" });
                    return View(question);
                }
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Company' is null.");
            }
        }

        // POST: Questions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Type,QuestionText,MultipleOptions,ExplanationType,OptionsAmount")] Question question)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            ModelState.Remove("Company");
            try
            {
                if ((question.CompanyId != null && question.Type != "Ja/Nee") || question.Type == "Ja/Nee")
                {
                    ViewData["ErrorMessage"] = null;
                    if (ModelState.IsValid)
                    {
                        question = await _service.CreateQuestionAsync(question);
                        if (question.Type == "Meerkeuze")
                        {
                            //ViewData["QuestionText"] = question.QuestionText;
                            //var options = new List<QuestionOption>();
                            //for (int i = 0; i < question.OptionsAmount; i++)
                            //{
                            //    options.Add(new QuestionOption() { QuestionId = question.Id });
                            //}
                            //return View("CreateOptions", options);
                            return RedirectToAction(nameof(CreateOptions), new { question.Id });
                        }
                        if (question.Type == "Standpunt")
                        {
                            return RedirectToAction(nameof(CreateOptions), new { question.Id });
                        }
                        return RedirectToAction(nameof(Details), new { question.Id });
                    }
                } else
                {
                    ViewData["ErrorMessage"] = "Je moet een bedrijf selecteren.";
                }
                ViewData["Type"] = new SelectList(new List<string>() { "Open", "Meerkeuze", "Standpunt" });
                if (User.IsInRole("ROLE_ADMIN"))
                {
                    ViewData["CompanyId"] = new SelectList(await _service.GetCompaniesAsync(), "Id", "Name");
                    ViewData["Type"] = new SelectList(new List<string>() { "Open", "Meerkeuze", "Standpunt", "Ja/Nee" });
                }
                ViewData["ExplanationType"] = new SelectList(new List<string>() { "Nooit zichtbaar", "Altijd zichtbaar", "Laatste optie" });
                return View(question);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Question' is null.");
            }
        }

        public async Task<IActionResult> CreateYesOrNoQuestion()
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
                ViewData["textOptions"] = new SelectList(textOptions);
                var question = new Question();
                question.Type = "Ja/Nee";
                if (User.IsInRole("ROLE_EMPLOYER"))
                {
                    question.CompanyId = (await _service.GetCompanyAsync(User)).Id;
                }
                else
                {
                    ViewData["CompanyId"] = new SelectList(await _service.GetCompaniesAsync(), "Id", "Name");
                }
                return View(question);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Question' is null.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateYesOrNoQuestion([Bind("Id,CompanyId,Type,QuestionText,MultipleOptions,ExplanationType,OptionsAmount")] Question question)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            ModelState.Remove("Company");
            try
            {
                if (question.CompanyId != null)
                {
                    ViewData["ErrorMessage"] = null;
                    if (ModelState.IsValid)
                    {
                        question = await _service.CreateYesOrNoQuestionAsync(question);
                        return RedirectToAction(nameof(Details), new { question.Id });
                    }
                } else
                {
                        ViewData["ErrorMessage"] = "Je moet een bedrijf selecteren.";
                }
                var textOptions = await _service.GetYesOrNoTextOptionsAsync();
                ViewData["textOptions"] = new SelectList(textOptions);
                if (User.IsInRole("ROLE_ADMIN"))
                {
                    ViewData["CompanyId"] = new SelectList(await _service.GetCompaniesAsync(), "Id", "Name");
                }
                return View(question);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Question' is null.");
            }
        }

        public async Task<IActionResult> CreateOptions(int id) // question id
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
                ViewData["QuestionText"] = question.QuestionText;
                List<QuestionOption> options = new List<QuestionOption>();
                var amount = 2;
                if (question.Type == "Meerkeuze")
                {
                    amount = question.OptionsAmount;
                }
                for (int i = 0; i < amount; i++)
                {
                    options.Add(new QuestionOption() { QuestionId = question.Id });
                }
                return View(options.ToArray());
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Question' is null.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOptions([Bind("Id,QuestionId,OptionShort,OptionLong")] QuestionOption[] questionOptions)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unautzed");
            }
            for (int i = 0; i < questionOptions.Count(); i++)
            {
                ModelState.Remove($"[{i}].Question");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var questionOptionsCreated = await _service.CreateQuestionOptionsAsync(questionOptions.ToList());
                    return RedirectToAction(nameof(Details), new { Id = questionOptionsCreated.First().QuestionId });
                }
                catch (InternalServerException)
                {
                    return Problem("Entity set 'ApplicationDbContext.Question' is null.");
                }
            }
            return View(questionOptions);
        }

        // GET: Questions/Edit/5
        public async Task<IActionResult> Edit(int id)
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
                ViewData["textOptions"] = new SelectList(await _service.GetYesOrNoTextOptionsAsync());
                ViewData["Type"] = new SelectList(new List<string>() { "Open", "Meerkeuze", "Standpunt" });
                if (User.IsInRole("ROLE_ADMIN"))
                {
                    ViewData["CompanyId"] = new SelectList(await _service.GetCompaniesAsync(), "Id", "Name");
                    ViewData["Type"] = new SelectList(new List<string>() { "Open", "Meerkeuze", "Standpunt", "Ja/Nee" });
                }
                ViewData["ExplanationType"] = new SelectList(new List<string>() { "Nooit zichtbaar", "Altijd zichtbaar", "Laatste optie" });
                return View(question);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Question' is null.");
            }
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Type,QuestionText,MultipleOptions,ExplanationType,OptionsAmount")] Question question)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            if (id != question.Id)
            {
                return NotFound();
            }
            ModelState.Remove("Company");
            //ModelState.Remove("Options");
            //ModelState.Remove("JobOffers");
            //ModelState.Remove("Answers");
            try
            {
                if (ModelState.IsValid)
                {
                    if (!await _service.DoesQuestionExistsAsync(id))
                    {
                        return NotFound();
                    }
                    await _service.UpdateQuestionAsync(question);
                    return RedirectToAction(nameof(Details), new { id });
                }
                ViewData["textOptions"] = new SelectList(await _service.GetYesOrNoTextOptionsAsync());
                ViewData["Type"] = new SelectList(new List<string>() { "Open", "Meerkeuze", "Standpunt" });
                if (User.IsInRole("ROLE_ADMIN"))
                {
                    ViewData["CompanyId"] = new SelectList(await _service.GetCompaniesAsync(), "Id", "Name");
                    ViewData["Type"] = new SelectList(new List<string>() { "Open", "Meerkeuze", "Standpunt", "Ja/Nee" });
                }
                ViewData["ExplanationType"] = new SelectList(new List<string>() { "Nooit zichtbaar", "Altijd zichtbaar", "Laatste optie" });
                return View(question);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Question' is null.");
            }
        }

        public async Task<IActionResult> EditOption(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var questionOption = await _service.GetQuestionOptionAsync(id, User);
                if (questionOption == null)
                {
                    return NotFound("The questionOption does not exist or the questionOption does not belong to your company.");
                }
                return View(questionOption);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.QuestionOption' is null.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOption(int id, [Bind("Id,QuestionId,OptionShort,OptionLong")] QuestionOption questionOption)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            if (id != questionOption.Id)
            {
                return NotFound();
            }
            ModelState.Remove("Question");
            try
            {
                if (ModelState.IsValid)
                {
                    if (!await _service.DoesQuestionOptionExistsAsync(id))
                    {
                        return NotFound();
                    }
                    await _service.UpdateQuestionOptionAsync(questionOption);
                    var questionId = questionOption.QuestionId;
                    return RedirectToAction(nameof(Details), new { id = questionId });
                }
                return View(questionOption);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Question' is null.");
            }
        }

        // GET: Questions/Delete/5
        public async Task<IActionResult> Delete(int id)
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
                    return NotFound();
                }
                return View(question);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Question' is null.");
            }
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                await _service.DeleteQuestionAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Question' is null.");
            }
        }

        public async Task<IActionResult> DeleteOption(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var questionOption = await _service.GetQuestionOptionAsync(id, User);
                if (questionOption == null)
                {
                    return NotFound();
                }
                return View(questionOption);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.QuestionOption' is null.");
            }
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("DeleteOption")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOptionConfirmed(int id, int questionId)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                await _service.DeleteQuestionOptionAsync(id);
                return RedirectToAction(nameof(Details), new { id = questionId });
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.QuestionOption' is null.");
            }
        }

        //private bool QuestionExists(int id)
        //{
        //  return (_context.Question?.Any(e => e.Id == id)).GetValueOrDefault();
        //}
    }
}
