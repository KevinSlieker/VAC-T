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
        public DbSet<Company> Company { get; set; } = default!;
        public DbSet<JobOffer> JobOffer { get; set; } = default!;
        public DbSet<Solicitation> Solicitation { get; set; } = default!;
        public DbSet<UserDetailsModel> UserDetailsModel { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Company>()
                .HasOne(c => c.User)
                .WithOne(u => u.Company)
                .HasForeignKey("Company", "UserId")
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}