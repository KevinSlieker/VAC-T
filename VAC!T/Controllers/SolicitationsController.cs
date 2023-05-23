using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Models;

namespace VAC_T.Controllers
{
    public class SolicitationsController : Controller
    {

        private readonly SolicitationService _service;
        private readonly SignInManager<VAC_TUser> _signInManager;

        public SolicitationsController(SolicitationService service, SignInManager<VAC_TUser> signInManager)
        {
            _service = service;
            _signInManager = signInManager;
        }

        // GET: Solicitations
        public async Task<IActionResult> Index(string? searchJobOffer, string? searchCompany, string? searchCandidate, bool? searchSelectedYes, bool? searchSelectedNo)
        {
            if (_signInManager.IsSignedIn(User) == false)
            {
                return Unauthorized("Need to be logged in");
            }
            ViewData["searchJobOffer"] = searchJobOffer;
            ViewData["searchCompany"] = searchCompany;
            ViewData["searchCandidate"] = searchCandidate;
            ViewData["searchSelectedYes"] = searchSelectedYes;
            ViewData["searchSelectedNo"] = searchSelectedNo;

            try
            {
                var solicitations = await _service.GetSolicitationsAsync(User, searchJobOffer, searchCompany, searchCandidate, searchSelectedYes, searchSelectedNo);
                return View(solicitations);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Solicitation' is null.");
            }
        }

        public async Task<IActionResult> Create(int jobOfferId)
        {
            if (!User.IsInRole("ROLE_CANDIDATE"))
            {
                return Unauthorized("You need to be be a candidate to solicitate");
            }
            try
            {
                if (! await _service.DoesJobOfferExistsAsync(jobOfferId))
                {
                    return NotFound($"JobOffer with Id: {jobOfferId} does not exist.");
                }
                if (! await _service.AreQuestionsAnsweredAsync(jobOfferId, User))
                {
                    return Problem("You have not answered the interviewQuestions for this jobOffer.");
                }
                var solicitation = await _service.CreateSolicitationAsync(jobOfferId, User);
                if (solicitation == null)
                {
                    return BadRequest("You have already solicitated for this jobOffer.");
                }
                return Redirect("/JobOffers/Details/" + jobOfferId);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Solicitation' is null.");
            }
        }

        public async Task<IActionResult> Delete(int jobOfferId)
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
                if (!await _service.DoesSolicitationExistWithJobOfferIdAsync(jobOfferId, User))
                {
                    return NotFound("Solicitation does not exist.");
                }

                await _service.DeleteSolicitationAsync(jobOfferId, User);
                return Redirect("/JobOffers/Details/" + jobOfferId);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Solicitation' is null.");
            }
        }

        public async Task<IActionResult> Select(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                if (! await _service.DoesSolicitationExistsAsync(id))
                {
                    return NotFound();
                }
                await _service.SelectSolicitationAsync(id);

                return RedirectToAction("Index");
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Solicitation' is null.");
            }
        }
    }
}
