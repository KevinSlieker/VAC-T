using System.Security.Cryptography;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VAC_T.Data;
using VAC_T.Models;
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
                using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var manager = scope.ServiceProvider.GetRequiredService<UserManager<VAC_TUser>>();
                    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

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
                                ConcurrencyStamp = "2"
                            }
                            );
                        context.SaveChanges();
                    }

                    if (!context.Roles.Any(i => i.Name == "ROLE_ADMIN"))
                    {
                        context.Roles.Add(
                            new IdentityRole
                            {
                                Name = "ROLE_ADMIN",
                                NormalizedName = "ROLE_ADMIN",
                                ConcurrencyStamp = "3"
                            });
                        context.SaveChanges();

                        var userAdmin = new VAC_TUser
                        {
                            UserName = "Admin@mail.nl",
                            Email = "Admin@mail.nl",
                            EmailConfirmed = true,
                            PhoneNumber = "123456798",
                            Name = "Admin",
                            BirthDate = DateTime.Now,
                            Address = "Admin",
                            Postcode = "Admin",
                            Residence = "Admin",
                            ProfilePicture = "assets/img/user/profile.png"
                        };
                        var result = await manager.CreateAsync(userAdmin, "Admin123!");
                        await manager.AddToRoleAsync(userAdmin, "ROLE_ADMIN");
                        context.SaveChanges();
                    }

                    if (!context.Users.Any())
                    {
                        var userEmployer = new VAC_TUser
                        {
                            UserName = "EmployerDSM@mail.nl",
                            Email = "EmployerDSM@mail.nl",
                            EmailConfirmed = true,
                            PhoneNumber = "123456798",
                            Name = "Test Employer DSM",
                            BirthDate = DateTime.Now,
                            Address = "12 Address Plaza",
                            Postcode = "6666AA",
                            Residence = "ErgensHuizen",
                            ProfilePicture = "assets/img/user/profile.png"
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
                            Postcode = "6666AA",
                            Residence = "ErgensHuizen",
                            ProfilePicture = "assets/img/user/profile.png"
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
                            Postcode = "3597ZA",
                            Residence = "ErgensHuizen",
                            ProfilePicture = "assets/img/user/profile.png",
                            Motivation = "Test modivation. This is a test motivation.",
                            CV = "assets/cv/CV2.pdf"
                            //Role = context.Roles.Local.SingleOrDefault(x => x.Name == "ROLE_CANDIDATE")
                        };
                        var result3 = await manager.CreateAsync(userCandidate, "Candidate123!");
                        await manager.AddToRoleAsync(userCandidate, "ROLE_CANDIDATE");

                        context.SaveChanges();

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
                                Postcode = "6135KR",
                                Residence = "Sittard",
                                User = context.Users.Where(n => n.Name == "Test Employer DSM").FirstOrDefault()
                            },
                            new Company
                            {
                                Name = "Educom",
                                Description = "Educom is een professionele ICT-opleider die mensen bij- of omschoolt  tot softwareontwikkelaar. Wij begeleiden werkzoekende ICT-ers," +
                                " of mensen die van de ICT hun beroep willen maken, door middel van een gedegen en vooral praktijkgericht traineeship naar een baan. ",
                                LogoURL = "assets/img/company/educom.png",
                                WebsiteURL = "https://edu-deta.com/",
                                Address = "D.U. Stikkerstraat 10",
                                Postcode = "6842CW",
                                Residence = "Arnhem",
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
                                Residence = "Sittard",
                                LogoURL = "assets/img/job_offer/windows.png"
                            },
                            new JobOffer
                            {
                                Name = "Applicatie Beheerder voor DSM Sittard",
                                Description = "Description 2",
                                Company = context.Company.Where(n => n.Name == "DSM").FirstOrDefault(),
                                Level = "Junior",
                                Residence = "Sittard",
                                LogoURL = "assets/img/job_offer/windows.png"
                            },
                            new JobOffer
                            {
                                Name = "C# developer voor Educom Arnhem",
                                Description = "Description 3",
                                Company = context.Company.Where(n => n.Name == "Educom").FirstOrDefault(),
                                Level = "Midior",
                                Residence = "Arnhem",
                                LogoURL = "assets/img/job_offer/csharp.png"
                            }
                            );
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}
