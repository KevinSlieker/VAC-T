using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VAC_T.Data;
using static System.Net.Mime.MediaTypeNames;

namespace VAC_T.Models
{
    public static class DbInitializer
    {
        public static async void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<ApplicationDbContext>>()))
            {
                if (!context.Roles.Any())
                {
                    context.Roles.AddRange(
                        new IdentityRole
                        {
                            Name = "ROLE_EMPLOYER",
                            NormalizedName = "ROLE_EMPLOYER",
                            ConcurrencyStamp = "1"
                        },
                        new IdentityRole
                        {
                            Name = "ROLE_CANDIDATE",
                            NormalizedName = "ROLE_CANDIDATE",
                            ConcurrencyStamp = "1"
                        }
                        );
                    context.SaveChanges();
                }
                if (!context.Users.Any())
                {
                    using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {
                        var manager = scope.ServiceProvider.GetRequiredService<UserManager<VAC_TUser>>();
                        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                        var userEmployer = new VAC_TUser
                        {
                            UserName = "EmployerDSM@mail.nl",
                            Email = "EmployerDSM@mail.nl",
                            EmailConfirmed= true,
                            PhoneNumber = "123456798",
                            Name = "Test Employer DSM",
                            BirthDate= DateTime.Now,
                            Address = "12 Address Plaza",
                            ProfilePicture = "assets/img/user/TestEmployerDSM.svg"
                            //Role = context.Roles.Local.SingleOrDefault(x => x.Name == "ROLE_EMPLOYER")
                        };
                        var result = await manager.CreateAsync(userEmployer, "EmployerDSM1!");
                        await manager.AddToRoleAsync(userEmployer, "ROLE_EMPLOYER");
                        context.SaveChanges();

                        var userEmployer2 = new VAC_TUser
                        {
                            UserName = "EmployerEducom@mail.nl",
                            Email = "EmployerEducom@mail.nl",
                            EmailConfirmed = true,
                            PhoneNumber = "123456798",
                            Name = "Test Employer Educom",
                            BirthDate = DateTime.Now,
                            Address = "3 Address Plaza",
                            ProfilePicture = "assets/img/user/TestEmployerEducom.svg"
                            //Role = context.Roles.Local.SingleOrDefault(x => x.Name == "ROLE_EMPLOYER")
                        };
                        var result2 = await manager.CreateAsync(userEmployer2, "EmployerEducom1!");
                        await manager.AddToRoleAsync(userEmployer2, "ROLE_EMPLOYER");

                        var userCandidate = new VAC_TUser
                        {
                            UserName = "Candidate@mail.nl",
                            Email = "Candidate@mail.nl",
                            EmailConfirmed = true,
                            PhoneNumber = "987654321",
                            Name = "Test Candidate",
                            BirthDate = DateTime.Now,
                            Address = "37 Test Plaza",
                            ProfilePicture = "assets/img/user/TestCandidate.svg",
                            Motivation = "Test modivation. This is a test motivation.",
                            CV = "http://testCV.com/"
                            //Role = context.Roles.Local.SingleOrDefault(x => x.Name == "ROLE_CANDIDATE")
                        };
                        var result3 = await manager.CreateAsync(userCandidate, "Candidate123!");
                        await manager.AddToRoleAsync(userCandidate, "ROLE_CANDIDATE");

                        context.SaveChanges();
                    }
                }
                if (!context.Company.Any())
                {
                    context.Company.AddRange(
                        new Company
                        {
                            Name = "DSM",
                            Description = "Koninklijke DSM N.V. is een wereldwijd, ‘purpose-led’ bedrijf in Gezondheid," +
                            " Voeding en Bioscience dat vanuit wetenschappelijke basis de gezondheid van mens, dier en planeet verbetert. " +
                            "Duurzaamheid is voor ons een verantwoordelijkheid, een kernwaarde en staat centraal in alles wat we doen. " +
                            "Met onze producten en innovatieve oplossingen willen we het leven van mensen verbeteren. " +
                            "We richten ons hierbij op een goede gezondheid en gezonde, goed smakende en duurzaam geproduceerde voeding voor iedereen. " +
                            "Denk hierbij aan vitamines, mineralen, eiwitten, gezonde vetzuren, enzymen en andere gezonde ingrediënten die je terugvindt in voedselproducten" +
                            " voor mens en dier.",
                            LogoURL = "assets/img/company/dsm.svg",
                            WebsiteURL = "https://www.dsm.com/nederland/nl_NL/home.html",
                            Address = "Poststraat 1",
                            User = context.Users.Where(n => n.Name == "Test Employer DSM").FirstOrDefault()
                        },
                        new Company
                        {
                            Name = "Educom",
                            Description = "Educom is een professionele ICT-opleider die mensen bij- of omschoolt  tot softwareontwikkelaar. Wij begeleiden werkzoekende ICT-ers," +
                            " of mensen die van de ICT hun beroep willen maken, door middel van een gedegen en vooral praktijkgericht traineeship naar een baan. ",
                            LogoURL = "assets/img/company/dsm.svg",
                            WebsiteURL = "https://www.dsm.com/nederland/nl_NL/home.html",
                            Address = "D.U. Stikkerstraat 10",
                            User = context.Users.Where(n => n.Name == "Test Employer Educom").FirstOrDefault()
                        });
                    context.SaveChanges();
                }
                if (!context.JobOffer.Any())
                {
                    context.JobOffer.AddRange(
                        new JobOffer
                        {
                            Name = "Applicatie Beheerder voor DSM Sittard",
                            Description = "Description 1",
                            Company = context.Company.Where(n => n.Name == "DSM").FirstOrDefault(),
                            Level = "Intro",
                            LogoURL = "assets/img/job_offer/windows.png"
                        },
                        new JobOffer
                        {
                            Name = "Applicatie Beheerder voor DSM Sittard",
                            Description = "Description 2",
                            Company = context.Company.Where(n => n.Name == "DSM").FirstOrDefault(),
                            Level = "Junior",
                            LogoURL = "assets/img/job_offer/windows.png"
                        },
                        new JobOffer
                        {
                            Name = "C# developer",
                            Description = "Description 3",
                            Company = context.Company.Where(n => n.Name == "Educom").FirstOrDefault(),
                            Level = "Midior",
                            LogoURL = "assets/img/job_offer/csharp.png"
                        }
                        );
                    context.SaveChanges();
                }
            }
        }
    }
}
