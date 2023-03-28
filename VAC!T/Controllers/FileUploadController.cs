using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Models;

namespace VAC_T.Controllers
{
    public class FileUploadController : Controller
    {
        private UserManager<VAC_TUser> _userManager;
        private readonly FileUploadService _service;

        public FileUploadController(UserManager<VAC_TUser> userManager, FileUploadService service)
        {
            _userManager = userManager;
            _service = service;
        }

        public async Task<IActionResult> EditProfilePicture(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return base.NotFound($"Unable to load user with ID '{id}'.");
            }
            if (user.Id != id)
            {
                return base.Unauthorized("Kan profielfoto niet updaten");
            }
            return View(new ProfilePictureModel() { Id = id, ProfilePicture = user.ProfilePicture });
        }


        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(string id, IFormFile FormFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return base.NotFound($"Unable to load user with ID '{id}'.");
            }
            if (user.Id != id)
            {
                return base.Unauthorized("Kan profielfoto niet updaten");
            }

            if (FormFile == null)
            {
                return View("EditProfilePicture", new ProfilePictureModel() { Id = id, ProfilePicture = user.ProfilePicture });
            }
            try
            {
                await _service.UploadProfilePictureAsync(user, FormFile);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
            return Redirect("/Identity/Account/Manage");
        }

        public async Task<IActionResult> EditCV(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return base.NotFound($"Unable to load user with ID '{id}'.");
            }
            if (user.Id != id)
            {
                return base.Unauthorized("Kan CV niet updaten");
            }
            return View(new CVModel() { Id = id, CV = user.CV });
        }

        [HttpPost]
        public async Task<IActionResult> UploadCV(string id, IFormFile FormFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return base.NotFound($"Unable to load user with ID '{id}'.");
            }
            if (user.Id != id)
            {
                return base.Unauthorized("Kan CV niet updaten");
            }

            if (FormFile == null)
            {
                return View("EditCV", new CVModel() { Id = id, CV = user.CV });
            }
            try
            {
                var result = await _service.UploadCVAsync(user, FormFile);
                if (result == false)
                {
                    ViewData["Message"] = "Je kan alleen een pdf uploaden";
                    return View("EditCV", new CVModel() { Id = id, CV = user.CV });
                }
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
            return Redirect("/Identity/Account/Manage");
        }

        [HttpGet]
        public IActionResult GetCV(string cv)
        {
            string filePath = "~/" + cv;
            string fileName = Path.GetFileName(filePath);
            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(filePath, "application/pdf");
        }

        public IActionResult CreateJobOfferLogoURL()
        {
            return View(new JobOfferLogoURLModel());
        }

        public async Task<IActionResult> UploadJobOfferLogoURL(string LanguageName, IFormFile FormFile)
        {
            if (!(User.IsInRole("ROLE_EMPLOYER") || User.IsInRole("ROLE_ADMIN")))
            {
                return base.Unauthorized("Kan geen Software Logo uploaden");
            }

            if (FormFile == null)
            {
                return View("CreateJobOfferLogoURL", new JobOfferLogoURLModel());
            }
            try
            {
                await _service.UploadJobOfferLogoURLAsync(LanguageName, FormFile);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
            if (User.IsInRole("ROLE_ADMIN"))
            {
                return Redirect("/JobOffers/Index");
            }
            return Redirect("/JobOffers/Create");
        }

        public async Task<IActionResult> EditCompanyLogo(int id)
        {
            try
            {
                var company = await _service.GetCompanyByIdAsync(id);
                var user = await _userManager.GetUserAsync(User);
                if (company == null)
                {
                    return base.NotFound($"Unable to load company with ID '{id}'.");
                }
                if (company.User != user && !User.IsInRole("ROLE_ADMIN") || !User.IsInRole("ROLE_ADMIN"))
                {
                    return base.Unauthorized("Kan Bedrijf Logo niet updaten");
                }
                return View(new CompanyLogoModel() { Id = company.Id, LogoURL = company.LogoURL });
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        public async Task<IActionResult> UploadCompanyLogo(int id, IFormFile FormFile)
        {
            try
            {
                var company = await _service.GetCompanyByIdAsync(id);
                var user = await _userManager.GetUserAsync(User);
                if (company == null)
                {
                    return base.NotFound($"Unable to load company with ID '{id}'.");
                }
                if (company.User != user && !User.IsInRole("ROLE_ADMIN") || !User.IsInRole("ROLE_ADMIN"))
                {
                    return base.Unauthorized("Kan Bedrijf Logo niet uploaden");
                }

                if (FormFile == null)
                {
                    return View("EditCompanyLogo", new CompanyLogoModel() { Id = company.Id, LogoURL = company.LogoURL });
                }
                await _service.UploadCompanyLogoAsync(company, FormFile);

                if (User.IsInRole("ROLE_ADMIN"))
                {
                    return Redirect("/Companies/Index");
                }
                return Redirect("/Companies/Details/" + company.Id);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        public string TrimQuotes(string text)
        {
            return text.Replace("\"", "").Trim();
        }
    }
}
