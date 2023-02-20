using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VAC_T.Data;
using VAC_T.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;

namespace VAC_T.Controllers
{
    public class FileUploadController : Controller
    {
        private IWebHostEnvironment _hostingEnv;
        private UserManager<VAC_TUser> _userManager;

        public FileUploadController(IWebHostEnvironment hostingEnv, UserManager<VAC_TUser> userManager)
        {
            _hostingEnv = hostingEnv;
            _userManager = userManager;
        }

        public async Task<IActionResult> EditProfilePicture (string id)
        {
            var user = await _userManager.GetUserAsync(User);
            string? idUser = _userManager.GetUserId(User);
            if (user == null)
            {
                return base.NotFound($"Unable to load user with ID '{idUser}'.");
            }
            if (idUser != id)
            {
                return base.Unauthorized("Kan profielfoto niet updaten");
            }
            return View( new ProfilePictureModel() { Id = id, ProfilePicture = user.ProfilePicture });

        }


        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(string id, IFormFile FormFile)
        {
            var user = await _userManager.GetUserAsync(User);
            string? idUser = _userManager.GetUserId(User);
            if (user == null)
            {
                return base.NotFound($"Unable to load user with ID '{idUser}'.");
            }
            if (idUser != id) 
            {
                return base.Unauthorized("Kan profielfoto niet updaten");
            }

            if (FormFile == null) 
            {
                return View("EditProfilePicture", new ProfilePictureModel() { Id = id, ProfilePicture = user.ProfilePicture });
            }
            var filename = ContentDispositionHeaderValue.Parse(FormFile.ContentDisposition).FileName.Value;
            filename = id + Path.GetExtension(filename);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "img", "user", filename);
            using (System.IO.Stream stream = new FileStream(path, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }

            user.ProfilePicture = Path.Combine("assets", "img", "user", filename);
            await _userManager.UpdateAsync(user);

            return Redirect("/Identity/Account/Manage");

        }

        public async Task<IActionResult> EditCV(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            string? idUser = _userManager.GetUserId(User);
            if (user == null)
            {
                return base.NotFound($"Unable to load user with ID '{idUser}'.");
            }
            if (idUser != id)
            {
                return base.Unauthorized("Kan CV niet updaten");
            }
            return View(new CVModel() { Id = id, CV = user.CV });

        }

        [HttpPost]
        public async Task<IActionResult> UploadCV(string id, IFormFile FormFile)
        {
            var user = await _userManager.GetUserAsync(User);
            string? idUser = _userManager.GetUserId(User);
            if (user == null)
            {
                return base.NotFound($"Unable to load user with ID '{idUser}'.");
            }
            if (idUser != id)
            {
                return base.Unauthorized("Kan CV niet updaten");
            }

            if (FormFile == null)
            {
                return View("EditCV", new CVModel() { Id = id, CV = user.CV });
            }
            var filename = ContentDispositionHeaderValue.Parse(FormFile.ContentDisposition).FileName.Value;
            filename = id + Path.GetExtension(filename);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "cv", filename);
            using (System.IO.Stream stream = new FileStream(path, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }

            user.CV = Path.Combine("assets", "cv", filename);
            await _userManager.UpdateAsync(user);

            return Redirect("/Identity/Account/Manage");
        }

        public async Task<IActionResult> CreateJobOfferLogoURL()
        {
            return View(new JobOfferLogoURLModel());
        }

        public async Task<IActionResult> UploadJobOfferLogoURL(string LanguageName ,IFormFile FormFile)
        {
            if (!(User.IsInRole("ROLE_EMPLOYER") || User.IsInRole("ROLE_ADMIN")))
            {
                return base.Unauthorized("Kan geen Software Logo uploaden");
            }

            if (FormFile == null)
            {
                return View("CreateJobOfferLogoURL", new JobOfferLogoURLModel());
            }
            var filename = ContentDispositionHeaderValue.Parse(FormFile.ContentDisposition).FileName.Value;
            filename = LanguageName + Path.GetExtension(filename);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "img", "job_offer", filename);
            using (System.IO.Stream stream = new FileStream(path, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }

            return Redirect("/JobOffers/Create");
        }



            public string TrimQuotes(string text)
        {
            return text.Replace("\"", "").Trim();
        }
    }
}
