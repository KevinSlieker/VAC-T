﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Models;

namespace VAC_T.Controllers
{
    public class AnswersController : Controller
    {
        private readonly AnswerService _service;
        private readonly IMapper _mapper;
        public AnswersController(AnswerService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        // GET: Answers
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var answers = await _service.GetAnswersAsync();
                return View(answers);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Answer' is null.");
            }
        }

        public async Task<IActionResult> DetailsPerJobOffer(int id) // jobOffer id
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var answers = await _service.GetAnswersForJobOfferAsync(id, User);
                var viewModel = _mapper.Map<List<AnswerViewModel>>(answers);
                foreach (var answer in viewModel)
                {
                    answer.DisplayAnswerText = _service.PrepareAnswerForDisplay(answer.AnswerText, answer.Question);
                }
                return View(viewModel);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Answer' is null.");
            }
        }

        // GET: Answers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var answer = await _service.GetAnswerAsync(id);
                if (answer == null)
                {
                    return NotFound();
                }

                return View(answer);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Answer' is null.");
            }
        }

        public async Task<IActionResult> AnswerQuestions(int id) // jobOffer id
        {
            try
            {
                if (await _service.DoAnswersExistAsync(id, User))
                {
                    return RedirectToAction("DetailsPerJobOffer", new { id });
                    //var answers = await _service.GetUserAnswersForJobOfferAsync(id, User);
                    //return View("Edit", answers);
                }
                else
                {
                    return RedirectToAction("Create", new { id });
                    //var answers = await _service.PrepareUserAnswersForCreateAsync(id, User);
                    //if (answers == null)
                    //{
                    //    return NotFound("No questions are made yet");
                    //}
                    //return View("Create", answers);
                }
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Answer' is null.");
            }
        }

        // GET: Answers/Create
        public async Task<IActionResult> Create(int id) // jobOffer id
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var answers = await _service.PrepareUserAnswersForCreateAsync(id, User);
                if (answers == null)
                {
                    return NotFound("No questions are made yet");
                }
                var viewModel = _mapper.Map<List<AnswerViewModel>>(answers);
                return View(viewModel.ToArray());
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Answer' is null.");
            }
        }

        // POST: Answers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuestionId,JobOfferId,UserId,AnswerText,MultipleChoiceAnswers,Explanation")] AnswerViewModel[] answersViewModel)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                for (int i = 0; i < answersViewModel.Count(); i++)
                {
                    var question = await _service.GetQuestionAsync(answersViewModel[i].QuestionId);
                    if (question == null)
                    {
                        return BadRequest();
                    }
                    answersViewModel[i].Question = question;
                    ModelState.Remove($"[{i}].Question");
                    ModelState.Remove($"[{i}].JobOffer");
                    ModelState.Remove($"[{i}].User");
                    if (!(answersViewModel[i].Question.Type == "Meerkeuze" && answersViewModel[i].Question.MultipleOptions == true))
                    {
                        ModelState.Remove($"[{i}].MultipleChoiceAnswers");
                    }
                    else
                    {
                        ModelState.Remove($"[{i}].AnswerText");
                    }
                }
                if (ModelState.IsValid)
                {
                    for (int i = 0; i < answersViewModel.Count(); i++)
                    {
                        if (answersViewModel[i].Question.Type == "Meerkeuze" && answersViewModel[i].Question.MultipleOptions == true)
                        {
                            for (int s = 0; s < answersViewModel[i].MultipleChoiceAnswers.Count(); s++)
                            {
                                answersViewModel[i].AnswerText += answersViewModel[i].MultipleChoiceAnswers[s] + "_";
                            }
                            answersViewModel[i].AnswerText = answersViewModel[i].AnswerText.Trim('_');
                        }
                    }
                    var answers = _mapper.Map<List<Answer>>(answersViewModel);
                    var greatedAnswers = await _service.CreateAnswersAsync(answers);
                    return RedirectToAction(nameof(DetailsPerJobOffer), new { id = greatedAnswers.First().JobOfferId });
                }
                return View(answersViewModel);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Answer' is null.");
            }
        }

        // GET: Answers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var answer = await _service.GetAnswerAsync(id);
                if (answer == null)
                {
                    return NotFound();
                }
                var viewModel = _mapper.Map<AnswerViewModel>(answer);
                if (viewModel.Question.Type == "Meerkeuze" && viewModel.Question.MultipleOptions == true)
                {
                    viewModel.MultipleChoiceAnswers = viewModel.AnswerText.Split('_');
                }
                return View(viewModel);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Answer' is null.");
            }
        }

        // POST: Answers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuestionId,JobOfferId,UserId,AnswerText,MultipleChoiceAnswers,Explanation")] AnswerViewModel answerViewModel)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            if (id != answerViewModel.Id)
            {
                return NotFound();
            }
            try
            {
                var question = await _service.GetQuestionAsync(answerViewModel.QuestionId);
                if (question == null)
                {
                    return NotFound();
                }
                answerViewModel.Question = question;
                ModelState.Remove("Question");
                ModelState.Remove("JobOffer");
                ModelState.Remove("User");
                if (!(answerViewModel.Question.Type == "Meerkeuze" && answerViewModel.Question.MultipleOptions == true))
                {
                    ModelState.Remove("MultipleChoiceAnswers");
                }
                else
                {
                    ModelState.Remove("AnswerText");
                }
                if (ModelState.IsValid)
                {
                    var answer = await _service.GetAnswerAsync(id);
                    if (answer == null)
                    {
                        return NotFound();
                    }
                    _mapper.Map(answerViewModel, answer);
                    await _service.UpdateAnswerAsync(answer);
                    return RedirectToAction(nameof(DetailsPerJobOffer), new { id = answer.JobOfferId });
                }
                return View(answerViewModel);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Answer' is null.");
            }
            ;
        }

        //// GET: Answers/Delete/5
        //public async Task<IActionResult> Delete(int id)
        //{
        //    if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
        //    {
        //        return Unauthorized("Unauthorized");
        //    }
        //    try
        //    {
        //        var answers = await _service.GetAnswerAsync(id);
        //        if (answers != null)
        //        {
        //            NotFound();
        //        }
        //        return View(answers);
        //    }
        //    catch (InternalServerException)
        //    {
        //        return Problem("Entity set 'ApplicationDbContext.Answer' is null.");
        //    }
        //}

        //// POST: Answers/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
        //    {
        //        return Unauthorized("Unauthorized");
        //    }
        //    try
        //    {
        //        await _service.DeleteAnswerAsync(id);
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (InternalServerException)
        //    {
        //        return Problem("Entity set 'ApplicationDbContext.Answer' is null.");
        //    }
        //}

        // GET: Answers/Delete/5
        public async Task<IActionResult> DeleteUserAnswersForJobOffer(int id, string userId) // jobOffer Id
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var answers = await _service.GetAnswersForJobOfferAsync(id, userId);
                if (answers != null)
                {
                    NotFound();
                }
                return View(answers);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Answer' is null.");
            }
        }

        // POST: Answers/Delete/5
        [HttpPost, ActionName("DeleteUserAnswersForJobOffer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserAnswersForJobOfferConfirmed(int id, string userId) // jobOffer id
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                await _service.DeleteUserAnswersForJobOfferAsync(id, userId);
                return RedirectToAction(nameof(DetailsPerJobOffer), new { id });
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Answer' is null.");
            }
        }
    }
}