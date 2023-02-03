using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VAC_T.Models;

namespace VAC_T.Data
{
    public class ApplicationDbContext : IdentityDbContext<VAC_TUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<VAC_T.Models.Company> Company { get; set; } = default!;
        public DbSet<VAC_T.Models.JobOffer> JobOffer { get; set; } = default!;
        public DbSet<VAC_T.Models.Solicitation> Solicitation { get; set; } = default!;

        //public DbSet<VAC_T.Models.VAC_TUser> VAC_TUser { get; set;} = default!;

    }
}