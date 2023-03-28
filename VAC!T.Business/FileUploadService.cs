using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using VAC_T.DAL.Exceptions;
using VAC_T.Data;
using VAC_T.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace VAC_T.Business
{
    public class FileUploadService
    {
        private readonly IVact_TDbContext _context;
        private readonly UserManager<VAC_TUser> _userManager;

        public FileUploadService(IVact_TDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task UploadProfilePictureAsync(VAC_TUser user, IFormFile FormFile)
        {
            if (_context.Users == null)
            {
                throw new InternalServerException("Database not found");
            }
            var filename = ContentDispositionHeaderValue.Parse(FormFile.ContentDisposition).FileName.Value;
            filename = user.Id + Path.GetExtension(filename);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "img", "user", filename);
            using (System.IO.Stream stream = new FileStream(path, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }

            user.ProfilePicture = Path.Combine("assets", "img", "user", filename);
            await _userManager.UpdateAsync(user);
        }

        public async Task<bool> UploadCVAsync(VAC_TUser user, IFormFile FormFile)
        {
            if (_context.Users == null)
            {
                throw new InternalServerException("Database not found");
            }
            var filename = ContentDispositionHeaderValue.Parse(FormFile.ContentDisposition).FileName.Value;

            if (Path.GetExtension(filename) != ".pdf")
            {
                return false;
            }

            filename = user.Id + Path.GetExtension(filename);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "cv", filename);
            using (System.IO.Stream stream = new FileStream(path, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }

            user.CV = Path.Combine("assets", "cv", filename);
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task UploadJobOfferLogoURLAsync(string LanguageName, IFormFile FormFile)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var filename = ContentDispositionHeaderValue.Parse(FormFile.ContentDisposition).FileName.Value;
            filename = LanguageName + Path.GetExtension(filename);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "img", "job_offer", filename);
            using (System.IO.Stream stream = new FileStream(path, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }
        }

        public async Task<Company> GetCompanyByIdAsync(int id)
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }
            var company = await _context.Company.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            return company;
        }

        public async Task UploadCompanyLogoAsync(Company company, IFormFile FormFile)
        {
            var filename = ContentDispositionHeaderValue.Parse(FormFile.ContentDisposition).FileName.Value;
            filename = company.Id + Path.GetExtension(filename);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "img", "company", filename);
            using (System.IO.Stream stream = new FileStream(path, FileMode.Create))
            {
                await FormFile.CopyToAsync(stream);
            }
            company.LogoURL = Path.Combine("assets", "img", "company", filename);
            await _context.SaveChangesAsync();
        }
    }
}
