using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VAC_T.DAL.Exceptions;
using VAC_T.Data;
using VAC_T.Models;

namespace VAC_T.Business
{
    public class DashBoardService
    {
        private readonly IVact_TDbContext _context;
        private readonly UserManager<VAC_TUser> _userManager;

        public DashBoardService(IVact_TDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Solicitation>> GetSolicitationsAsync()
        {
            if (_context.Solicitation == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.Solicitation.ToListAsync();
        }
    }
}
