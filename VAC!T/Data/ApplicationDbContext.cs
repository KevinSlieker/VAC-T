﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
        public DbSet<VAC_T.Models.UserDetailsModel> UserDetailsModel { get; set; } = default!;

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

            //modelBuilder.Entity<Appointment>()
            //    .HasOne(a => a.Employer)
            //    .WithMany(u => u.Appointments)
            //    .HasForeignKey(a => a.EmployerId);

            //modelBuilder.Entity<Appointment>()
            //   .HasOne(a => a.Candidate)
            //   .WithMany(u => u.AppointmentsCandidate)
            //   .HasForeignKey(a => a.CandidateId);
        }

        public DbSet<VAC_T.Models.Appointment> Appointment { get; set; } = default!;
    }
}