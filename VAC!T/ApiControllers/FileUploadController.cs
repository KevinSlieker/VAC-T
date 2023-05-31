using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Models;

namespace VAC_T.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FileUploadController : Controller
    {
        private readonly IMapper _mapper;
        private readonly FileUploadService _service;
        private readonly UserManager<VAC_TUser> _userManager;

        public FileUploadController(IMapper mapper, FileUploadService service, UserManager<VAC_TUser> userManager)
        {
            _mapper = mapper;
            _service = service;
            _userManager = userManager;
        }

        // PUT: api/FileUpload/ProfilePicture/(user Id)
        /// <summary>
        /// Upload/Update a profilepicture.
        /// </summary>
        /// <param name="id">The id of the user</param>
        /// <param name="FormFile">The profilepicture</param>
        /// <returns>Ok</returns>
        /// <remarks>
        /// You select/upload the profilepicture in the body for the key: FormFile.
        /// </remarks>
        [HttpPut("ProfilePicture/{id}")]
        public async Task<IActionResult> PutProfilePictureAsync(string id, IFormFile FormFile)
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
                return NotFound("No file uploaded");
            }
            try
            {
                await _service.UploadProfilePictureAsync(user, FormFile);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // PUT: api/FileUpload/CV/(user Id)
        /// <summary>
        /// Upload/Update a CV for the user matching the userId
        /// </summary>
        /// <param name="id">The id of the user</param>
        /// <param name="FormFile">The CV</param>
        /// <returns>Ok</returns>
        /// <remarks>
        /// You select/upload the CV in the body for the key: FormFile. The CV needs to be a Pdf file.
        /// </remarks>
        [HttpPut("CV/{id}")]
        public async Task<IActionResult> PutCVAsync(string id, IFormFile FormFile)
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
                    return BadRequest("Je kan alleen een pdf uploaden");
                }
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // POST: api/FileUpload/JobOfferLogoURL
        /// <summary>
        /// Upload a jobOfferLogo for a programming language
        /// </summary>
        /// <param name="LanguageName">The programming language name</param>
        /// <param name="FormFile">The picture</param>
        /// <returns>Ok</returns>
        /// <remarks>
        /// You select/upload the picture in the body for the key: FormFile.
        /// </remarks>
        [HttpPost("JobOfferLogoURL/{LanguageName}")]
        public async Task<IActionResult> PostJobOfferLogoURLAsync(string LanguageName, IFormFile FormFile)
        {
            if (!(User.IsInRole("ROLE_EMPLOYER") || User.IsInRole("ROLE_ADMIN")))
            {
                return base.Unauthorized("Kan geen Software Logo uploaden");
            }

            if (FormFile == null)
            {
                return NotFound("No file uploaded");
            }
            try
            {
                await _service.UploadJobOfferLogoURLAsync(LanguageName, FormFile);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // PUT: api/FileUpload/CompanyLogo/5
        /// <summary>
        /// Upload/Update a company logo.
        /// </summary>
        /// <param name="id">The id of the company</param>
        /// <param name="FormFile">The company logo</param>
        /// <returns>Ok</returns>
        /// <remarks>
        /// You select/upload the company logo in the body for the key: FormFile.
        /// </remarks>
        [HttpPut("CompanyLogo/{id}")]
        public async Task<IActionResult> PutCompanyLogoAsync(int id, IFormFile FormFile)
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
                    return NotFound("No file uploaded");
                }
                await _service.UploadCompanyLogoAsync(company, FormFile);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }
    }
}
