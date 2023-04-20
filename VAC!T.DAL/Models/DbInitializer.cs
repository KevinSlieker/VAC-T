using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VAC_T.DAL.Service;
using VAC_T.Data;

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

                    Random rnd = new Random();
                    static DateTime RandomDay(Random gen, DateTime start, DateTime end)
                    {
                        int range = (end - start).Days;
                        var day = start.AddDays(gen.Next(range));
                        if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday)
                        {
                            day = RandomDay(gen, start, end);
                        }
                        return day;
                    };
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
                                Company = context.Company.Where(n => n.Name == "DSM").First(),
                                Level = "Intro",
                                Residence = "Sittard",
                                LogoURL = "assets/img/job_offer/windows.png"
                            },
                            new JobOffer
                            {
                                Name = "Applicatie Beheerder voor DSM Sittard",
                                Description = "Description 2",
                                Company = context.Company.Where(n => n.Name == "DSM").First(),
                                Level = "Junior",
                                Residence = "Sittard",
                                LogoURL = "assets/img/job_offer/windows.png"
                            },
                            new JobOffer
                            {
                                Name = "C# developer voor Educom Arnhem",
                                Description = "Description 3",
                                Company = context.Company.Where(n => n.Name == "Educom").First(),
                                Level = "Midior",
                                Residence = "Arnhem",
                                LogoURL = "assets/img/job_offer/csharp.png"
                            }
                            );
                        context.SaveChanges();
                    }

                    if (!await context.Users.AnyAsync(u => u.Name.Contains("PopulateUsersDataBase")))
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            var PopulateUsersDataBaseCandidate = new VAC_TUser
                            {
                                UserName = "PopulateUsersDataBaseCandidate" + i + "@mail.nl",
                                Email = "PopulateUsersDataBaseCandidate" + i + "@mail.nl",
                                EmailConfirmed = true,
                                PhoneNumber = "987654321",
                                Name = "PopulateUsersDataBaseCandidate" + i,
                                BirthDate = DateTime.Now,
                                Address = "37 Test Plaza",
                                Postcode = "3597ZA",
                                Residence = "ErgensHuizen",
                                ProfilePicture = "assets/img/user/profile.png",
                                Motivation = "Test modivation. This is a test motivation.",
                                CV = "assets/cv/CV2.pdf"
                            };
                            var result = await manager.CreateAsync(PopulateUsersDataBaseCandidate, "PopulateUsersDataBase123!");
                            await manager.AddToRoleAsync(PopulateUsersDataBaseCandidate, "ROLE_CANDIDATE");

                        }
                        await context.SaveChangesAsync();

                    }

                    if (!await context.Users.AnyAsync(c => c.Name.Contains("Employer PopulateCompany")))
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            var userEmployer = new VAC_TUser
                            {
                                UserName = "EmployerPopulateCompany" + i + "@mail.nl",
                                Email = "EmployerPopulateCompany" + i + "@mail.nl",
                                EmailConfirmed = true,
                                PhoneNumber = "123456798",
                                Name = "Employer PopulateCompany" + i,
                                BirthDate = DateTime.Now,
                                Address = (i + 12) + " Address Plaza",
                                Postcode = "6666AA",
                                Residence = "ErgensHuizen",
                                ProfilePicture = "assets/img/user/profile.png"
                            };
                            var result = await manager.CreateAsync(userEmployer, "EmployerPopulateCompany123!");
                            await manager.AddToRoleAsync(userEmployer, "ROLE_EMPLOYER");
                        }
                        await context.SaveChangesAsync();
                    }
                    if (!await context.Company.AnyAsync(c => c.Name.Contains("PopulateCompany")))
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            await context.Company.AddAsync(new Company
                            {
                                Name = "PopulateCompany" + i,
                                Description = "PopulateCompany" + i + " made for testing",
                                LogoURL = "assets/img/company/dsm.svg",
                                WebsiteURL = "https://www.dsm.com/nederland/nl_NL/home.html",
                                Address = "Poststraat " + (1 + i),
                                Postcode = "6135KR",
                                Residence = "Sittard",
                                User = context.Users.Where(n => n.Name == ("Employer PopulateCompany" + i)).FirstOrDefault()
                            });
                        }
                        await context.SaveChangesAsync();
                    }
                    if (!await context.JobOffer.AnyAsync(c => c.Name.Contains("PopulateCompany")))
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            await context.JobOffer.AddRangeAsync(
                                new JobOffer
                                {
                                    Name = "PopulateCompany" + i + "JobOffer1",
                                    Description = "Description 1",
                                    Company = context.Company.Where(n => n.Name == ("PopulateCompany" + i)).First(),
                                    Level = "Intro",
                                    Residence = "Sittard",
                                    Created = RandomDay(rnd, DateTime.Today.AddMonths(-4), DateTime.Today.AddMonths(-3)),
                                    LogoURL = "assets/img/job_offer/windows.png"
                                },
                                new JobOffer
                                {
                                    Name = "PopulateCompany" + i + "JobOffer2",
                                    Description = "Description 2",
                                    Company = context.Company.Where(n => n.Name == ("PopulateCompany" + i)).First(),
                                    Level = "Junior",
                                    Residence = "Sittard",
                                    Created = RandomDay(rnd, DateTime.Today.AddMonths(-4), DateTime.Today.AddMonths(-3)),
                                    LogoURL = "assets/img/job_offer/windows.png"
                                },
                                new JobOffer
                                {
                                    Name = "PopulateCompany" + i + "JobOffer3",
                                    Description = "Description 3",
                                    Company = context.Company.Where(n => n.Name == ("PopulateCompany" + i)).First(),
                                    Level = "Senior",
                                    Residence = "Sittard",
                                    Created = RandomDay(rnd, DateTime.Today.AddMonths(-4), DateTime.Today.AddMonths(-3)),
                                    LogoURL = "assets/img/job_offer/windows.png"
                                },
                                new JobOffer
                                {
                                    Name = "PopulateCompany" + i + "JobOffer4",
                                    Description = "Description 4",
                                    Company = context.Company.Where(n => n.Name == ("PopulateCompany" + i)).First(),
                                    Level = "Intro",
                                    Residence = "Sittard",
                                    Created = RandomDay(rnd, DateTime.Today.AddMonths(-4), DateTime.Today.AddMonths(-3)),
                                    LogoURL = "assets/img/job_offer/windows.png"
                                },
                                new JobOffer
                                {
                                    Name = "PopulateCompany" + i + "JobOffer5",
                                    Description = "Description 5",
                                    Company = context.Company.Where(n => n.Name == ("PopulateCompany" + i)).First(),
                                    Level = "Midior",
                                    Residence = "Sittard",
                                    Created = RandomDay(rnd, DateTime.Today.AddMonths(-4), DateTime.Today.AddMonths(-3)),
                                    LogoURL = "assets/img/job_offer/windows.png"
                                });
                        }
                        await context.SaveChangesAsync();
                        //context.ChangeTracker.Clear();
                    }

                    if (!await context.Solicitation.AnyAsync(c => c.JobOffer.Name.Contains("PopulateCompany")))
                    {
                        var jobOffers = await context.JobOffer.Where(j => j.Name.Contains("PopulateCompany")).ToListAsync();
                        var users = await context.Users.Where(u => u.Name.Contains("PopulateUsersDataBaseCandidate")).ToListAsync();
                        foreach (var user in users)
                        {
                            var rndJobOffers = jobOffers.OrderBy(x => rnd.Next()).Take(3).ToList();
                            await context.Solicitation.AddRangeAsync(
                                new Solicitation
                                {
                                    User = user,
                                    UserId = user.Id,
                                    JobOffer = rndJobOffers[0],
                                    Date = RandomDay(rnd, rndJobOffers[0].Created.AddDays(1), rndJobOffers[0].Created.AddMonths(1))
                                },
                                new Solicitation
                                {
                                    User = user,
                                    UserId = user.Id,
                                    JobOffer = rndJobOffers[1],
                                    Date = RandomDay(rnd, rndJobOffers[1].Created.AddDays(1), rndJobOffers[1].Created.AddMonths(1))
                                },
                                new Solicitation
                                {
                                    User = user,
                                    UserId = user.Id,
                                    JobOffer = rndJobOffers[2],
                                    Date = RandomDay(rnd, rndJobOffers[2].Created.AddDays(1), rndJobOffers[2].Created.AddMonths(1))
                                }
                                );
                        }
                        await context.SaveChangesAsync();
                    }

                    if (!await context.Solicitation.Where(s => s.JobOffer.Name.Contains("PopulateCompany")).Where(s => s.Selected == true).AnyAsync())
                    {
                        var solicitations = await context.Solicitation.Where(s => s.JobOffer.Name.Contains("PopulateCompany")).ToListAsync();
                        var rndSolicitations = solicitations.OrderBy(x => rnd.Next()).Take(100).ToList();
                        foreach (var solicitation in rndSolicitations)
                        {
                            solicitation.Selected = true;
                            solicitation.DateSelectedIsTrue = RandomDay(rnd, solicitation.Date.AddDays(1), solicitation.Date.AddDays(14));
                        }
                        await context.SaveChangesAsync();
                    }

                    if (!await context.Appointment.Where(a => a.Company.Name.Contains("PopulateCompany")).AnyAsync())
                    {
                        var duration = new TimeSpan(0, 0, 0);
                        var timeToAdd = new TimeSpan(0, 15, 0);
                        var maxDuration = new TimeSpan(2, 0, 0);
                        var durationList = new List<TimeSpan>();
                        while (duration < maxDuration)
                        {
                            duration += timeToAdd;
                            durationList.Add(duration);
                        }

                        var time = DateTime.Today.AddHours(9);
                        var maxTime = DateTime.Today.AddHours(17);
                        var timeList = new List<DateTime>() { time };
                        while (time < maxTime)
                        {
                            time = time.AddHours(1);
                            timeList.Add(time);
                        }

                        var isOnlineList = new List<bool>() { true, false };

                        var companies = await context.Company.Where(c => c.Name.Contains("PopulateCompany")).ToListAsync();
                        foreach (var company in companies)
                        {
                            var jobOffers = await context.JobOffer.Where(j => j.Company == company).ToListAsync();
                            var jobOffersList = new List<JobOffer?>();
                            foreach (var jobOffer in jobOffers)
                            {
                                jobOffersList.Add(jobOffer);
                                jobOffersList.Add(null);
                                jobOffersList.Add(null);
                            }

                            for (int i = 0; i < 10; i++)
                            {
                                var date = RandomDay(rnd, DateTime.Today.AddMonths(-2).AddDays(7), DateTime.Today.AddDays(-20));
                                var timeSelected = timeList.OrderBy(x => rnd.Next()).FirstOrDefault();
                                var durationSelected = durationList.OrderBy(x => rnd.Next()).FirstOrDefault();
                                var isOnlineSelected = isOnlineList.OrderBy(x => rnd.Next()).FirstOrDefault();
                                var jobOfferSelected = jobOffersList.OrderBy(x => rnd.Next()).FirstOrDefault();
                                await context.Appointment.AddAsync(
                                    new Appointment
                                    {
                                        Date = date,
                                        Time = date.Add(timeSelected.TimeOfDay),
                                        Duration = durationSelected,
                                        IsOnline = isOnlineSelected,
                                        Company = company,
                                        CompanyId = company.Id,
                                        JobOffer = jobOfferSelected,
                                    }
                                    );
                            }
                            await context.SaveChangesAsync();
                        }
                    }

                    if (!await context.RepeatAppointment.Where(a => a.Company.Name.Contains("PopulateCompany")).AnyAsync())
                    {
                        var duration = new TimeSpan(0, 0, 0);
                        var timeToAdd = new TimeSpan(0, 15, 0);
                        var maxDuration = new TimeSpan(2, 0, 0);
                        var durationList = new List<TimeSpan>();
                        while (duration < maxDuration)
                        {
                            duration += timeToAdd;
                            durationList.Add(duration);
                        }

                        var time = DateTime.Today.AddHours(9);
                        var maxTime = DateTime.Today.AddHours(17);
                        var timeList = new List<DateTime>() { time };
                        while (time < maxTime)
                        {
                            time = time.AddHours(1);
                            timeList.Add(time);
                        }

                        var isOnlineList = new List<bool>() { true, false };

                        var weekDaysHighest = Enum.GetValues(typeof(RepeatAppointment.Repeats_Weekdays)).Cast<int>().Max();
                        var weekDaysList = new List<RepeatAppointment.Repeats_Weekdays>();
                        for (int i = 1; i < (weekDaysHighest * 2); i++)
                        {
                            weekDaysList.Add((RepeatAppointment.Repeats_Weekdays)i);
                        }
                        var relativeWeekHighest = Enum.GetValues(typeof(RepeatAppointment.Repeats_Relative_Week)).Cast<int>().Max();
                        var relativeWeekList = new List<RepeatAppointment.Repeats_Relative_Week>();
                        for (int i = 1; i < (relativeWeekHighest * 2); i++)
                        {
                            relativeWeekList.Add((RepeatAppointment.Repeats_Relative_Week)i);
                        }

                        var companies = await context.Company.Where(c => c.Name.Contains("PopulateCompany")).ToListAsync();
                        foreach (var company in companies)
                        {
                            await context.RepeatAppointment.AddRangeAsync(
                                new RepeatAppointment
                                {
                                    Company = company,
                                    CompanyId = company.Id,
                                    Repeats = RepeatAppointment.RepeatsType.Daily,
                                    Time = timeList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                    Duration = durationList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                    IsOnline = isOnlineList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                },
                                new RepeatAppointment
                                {
                                    Company = company,
                                    CompanyId = company.Id,
                                    Repeats = RepeatAppointment.RepeatsType.Weekly,
                                    Time = timeList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                    Duration = durationList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                    IsOnline = isOnlineList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                    RepeatsWeekdays = weekDaysList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                },
                                new RepeatAppointment
                                {
                                    Company = company,
                                    CompanyId = company.Id,
                                    Repeats = RepeatAppointment.RepeatsType.Monthly,
                                    Time = timeList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                    Duration = durationList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                    IsOnline = isOnlineList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                    RepeatsDay = rnd.Next(1, 32),
                                },
                                new RepeatAppointment
                                {
                                    Company = company,
                                    CompanyId = company.Id,
                                    Repeats = RepeatAppointment.RepeatsType.MonthlyRelative,
                                    Time = timeList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                    Duration = durationList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                    IsOnline = isOnlineList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                    RepeatsWeekdays = weekDaysList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                    RepeatsRelativeWeek = relativeWeekList.OrderBy(x => rnd.Next()).FirstOrDefault(),
                                }
                                );
                            await context.SaveChangesAsync();
                        }
                    }

                    if (!await context.Solicitation.Where(s => s.JobOffer.Name.Contains("PopulateCompany")).Where(s => s.Appointment != null).Where(s => s.Selected == true).AnyAsync())
                    {
                        var service = new DbIntitializerService(context);
                        var solicitations = await context.Solicitation.Where(s => s.JobOffer.Name.Contains("PopulateCompany")).Where(s => s.Selected == true).ToListAsync();
                        var solicitationsList = solicitations.OrderBy(x => rnd.Next()).Take(80).ToList();

                        foreach (var solicitation in solicitationsList)
                        {
                            var randomDay = RandomDay(rnd, solicitation.DateSelectedIsTrue!.Value.AddDays(1), solicitation.DateSelectedIsTrue.Value.AddDays(14));
                            var appointments = await service.GetAvailableAppointmentsAsync(solicitation.Id, randomDay);
                            if (appointments == null)
                            {
                                continue;
                            }
                            var appointment = appointments.OrderBy(x => rnd.Next()).First();
                            if (appointment.Id == 0)
                            {
                                appointment.Solicitation = solicitation;
                                solicitation.Appointment = appointment;
                                solicitation.DateAppointmentSelected = randomDay;
                                await context.Appointment.AddAsync(appointment);
                                context.Solicitation.Update(solicitation);
                                await context.SaveChangesAsync();
                            }
                            else
                            {
                                appointment.Solicitation = solicitation;
                                solicitation.Appointment = appointment;
                                solicitation.DateAppointmentSelected = randomDay;
                                context.Appointment.Update(appointment);
                                context.Solicitation.Update(solicitation);
                                await context.SaveChangesAsync();
                            }
                        }
                    }

                    if (!await context.JobOffer.Where(j => j.Name.Contains("PopulateCompany")).Where(j => j.Closed != null).AnyAsync())
                    {
                        var jobOffers = await context.JobOffer.Where(j => j.Name.Contains("PopulateCompany")).ToListAsync();
                        foreach (var jobOffer in jobOffers)
                        {
                            jobOffer.Closed = RandomDay(rnd, DateTime.Today.AddMonths(1), DateTime.Today.AddMonths(3));
                        }
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
