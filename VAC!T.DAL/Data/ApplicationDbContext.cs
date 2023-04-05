using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VAC_T.Models;

namespace VAC_T.Data
{
    public class ApplicationDbContext : IdentityDbContext<VAC_TUser>, IVact_TDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Company> Company { get; set; } = default!;
        public DbSet<JobOffer> JobOffer { get; set; } = default!;
        public DbSet<Solicitation> Solicitation { get; set; } = default!;
        public DbSet<Appointment> Appointment { get; set; } = default!;
        public DbSet<RepeatAppointment> RepeatAppointment { get; set; } = default!;
        public DbSet<UserDetailsModel> UserDetailsModel { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Company>()
                .HasOne(c => c.User)
                .WithOne(u => u.Company)
                .HasForeignKey("Company", "UserId")
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Solicitation>()
                .HasOne(s => s.Appointment)
                .WithOne(a => a.Solicitation)
                .HasForeignKey("Solicitation", "AppointmentId")
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Solicitation>()
                .HasOne(s => s.User)
                .WithMany(u => u.Solicitations)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Company>()
                .HasMany(c => c.Appointments)
                .WithOne(a => a.Company)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Company>()
                .HasMany(c => c.RepeatAppointments)
                .WithOne(a => a.Company)
                .OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<Appointment>()
            //    .HasOne(a => a.Solicitation)
            //    .WithOne(s => s.Appointment)
            //    .HasForeignKey("Appointment", "SolicitationId")
            //    .OnDelete(DeleteBehavior.SetNull);
        }

    }
}