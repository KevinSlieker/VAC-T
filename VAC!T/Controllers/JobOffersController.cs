using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Models;

namespace VAC_T.Controllers
{
    public class JobOffersController : Controller
    {
        private readonly JobOfferService _service;

        public JobOffersController(JobOfferService service)
        {
            _service = service;
        }

        // GET: JobOffers
        public async Task<IActionResult> Index()
        {
            try
            {
                var jobOffers = await _service.GetJobOffersAsync(User);
                return View(jobOffers);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.JobOffer' is null.");
            }
        }

        // GET: JobOffers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var jobOffer = await _service.GetJobOfferAsync(id, User);
                if (jobOffer == null)
                {
                    return NotFound();
                }

                return View(jobOffer);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.JobOffer' is null.");
            }
        }

        // GET: JobOffers/Create
        public async Task<IActionResult> Create()
        {
            if (!User.IsInRole("ROLE_EMPLOYER"))
            {
                return Unauthorized("Not the correct roles.");
            }
            var jobOffer = new JobOffer();
            var company = await _service.GetCompanyForJobOfferAsync(User);
            jobOffer.CompanyId = company.Id;
            jobOffer.Residence = company.Residence;
            return View(jobOffer);
        }

        // POST: JobOffers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,LogoURL,Level,CompanyId,Residence")] JobOffer jobOffer)
        {
            if (!User.IsInRole("ROLE_EMPLOYER"))
            {
                return Unauthorized("Not the correct roles.");
            }
            ModelState.Remove("Company");
            if (ModelState.IsValid)
            {
                try
                {
                    jobOffer = await _service.CreateJobOfferAsync(jobOffer, User);
                    return RedirectToAction(nameof(Index));
                }
                catch (InternalServerException)
                {
                    return Problem("Entity set 'ApplicationDbContext.JobOffer' is null.");
                }
            }
            return View(jobOffer);
        }

        // GET: JobOffers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                var jobOffer = await _service.GetJobOfferAsync(id, User);
                if (jobOffer == null)
                {
                    return NotFound();
                }
                return View(jobOffer);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.JobOffer' is null.");
            }
        }

        // POST: JobOffers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,LogoURL,Level,Created,Residence,CompanyId,Company")] JobOffer jobOffer)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            if (id != jobOffer.Id)
            {
                return NotFound();
            }
            ModelState.Remove("Company");
            if (ModelState.IsValid)
            {
                try
                {
                    if (!await _service.DoesJobOfferExistsAsync(id))
                    {
                        return NotFound();
                    }
                    await _service.UpdateJobOfferAsync(jobOffer);
                    return RedirectToAction("Details", new { id });
                }
                catch (InternalServerException)
                {
                    return Problem("Entity set 'ApplicationDbContext.JobOffer' is null.");
                }
            }
            return View(jobOffer);
        }

        // GET: JobOffers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                var jobOffer = await _service.GetJobOfferAsync(id, User);
                if (jobOffer == null)
                {
                    return NotFound();
                }

                return View(jobOffer);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.JobOffer' is null.");
            }
        }

        // POST: JobOffers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                await _service.DeleteJobOfferAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.JobOffer' is null.");
            }
        }

        public async Task<IActionResult> ChangeJobOfferStatus(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                await _service.ChangeJobOfferStatusAsync(id);
                return RedirectToAction("Edit", new { id });
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.JobOffer' is null.");
            }
        }

        public async Task<IActionResult> SelectQuestionsForJobOffer(int id) //jobOffer Id
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var jobOffer = await _service.GetJobOfferWQuestionsAsync(id);
                if (jobOffer == null)
                {
                    return NotFound();
                }
                var questions = await _service.GetQuestionsForCompanyAsync(jobOffer.CompanyId);
                if (questions == null)
                {
                    return NotFound("No questions available to select from");
                }
                var selectList = new SelectList(questions, "Id", "QuestionText", jobOffer.Questions);
                ViewData["Questions"] = selectList;
                var viewModel = new SelectQuestionForJobOfferModel() { Id = id, SelectedQuestionIds = jobOffer.Questions.Select(j => j.Id).ToArray(), CompanyId = jobOffer.CompanyId};
                return View(viewModel);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.JobOffer' is null.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectQuestionsForJobOffer(int id, SelectQuestionForJobOfferModel selectedQuestions) //jobOffer Id
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            if (id != selectedQuestions.Id)
            {
                return NotFound();
            }
            try
            {
                if (ModelState.IsValid)
                {
                    if (!await _service.DoesJobOfferExistsAsync(id))
                    {
                        return NotFound();
                    }
                    var jobOffer = await _service.GetJobOfferWQuestionsAsync(id);
                    await _service.SelectJobOfferQuestionsAsync(id, selectedQuestions.SelectedQuestionIds);
                    return RedirectToAction("Details", new { id });
                }
                var questions = await _service.GetQuestionsForCompanyAsync(selectedQuestions.CompanyId);
                if (questions == null)
                {
                    return NotFound("No questions available to select from");
                }
                ViewData["Questions"] = new SelectList(questions, "Id", "QuestionText");
                return View(selectedQuestions);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.JobOffer' is null.");
            }
        }

    }
}
