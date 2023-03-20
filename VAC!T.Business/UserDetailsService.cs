using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VAC_T.DAL.Exceptions;
using VAC_T.Data;
using VAC_T.Models;

namespace VAC_T.Business
{
    public class UserDetailsService
    {
        private readonly IVact_TDbContext _context;
        private readonly UserManager<VAC_TUser> _userManager;

        public UserDetailsService(IVact_TDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Get a list of Users (optional) that match a search string
        /// </summary>
        /// <param name="searchName">(optional) string to match the name of the User to </param>
        /// <param name="searchEmail">(optional) string to match the email of the User to </param>
        /// <returns>a list of entries or null </returns>
        public async Task<IEnumerable<VAC_TUser>> GetUsersAsync(string? searchName, string? searchEmail)
        {

            if (_context.Users == null)
            {
                throw new InternalServerException("Database not found");
            }

            var users = from s in _context.Users select s;
            if (!string.IsNullOrEmpty(searchEmail))
            {
                users = users.Where(u => u.Email!.Contains(searchEmail));
            }

            if (!string.IsNullOrEmpty(searchName))
            {
                users = users.Where(u => u.Name!.Contains(searchName));
            }
            return await users.ToListAsync();
        }

        public async Task<VAC_TUser?> GetUserDetailsAsync(string id)
        {
            if (_context.Users == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            return await user;
        }

        public async Task<bool> DoesUserExistsAsync(string id)
        {
            if (_context.Users == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.Users.AnyAsync(c => c.Id == id);
        }

        public async Task DeleteUserAsync(string id)
        {
            if (_context.Users == null)
            {
                throw new InternalServerException("Database not found");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return;
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
