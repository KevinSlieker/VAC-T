using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VAC_T.Business;
using VAC_T.Models;
using VAC_T.UnitTest.TestObjects;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VAC_T.UnitTest.Services
{
    internal class AppointmentServiceTest
    {
        private SqliteConnection _inMemoryDb;
        private TestDbContext _context;
        private AppointmentService _service;
        private int? testJobOffer1Id;
        private int? testCompanyId;
        private int? testUserSolicitationId1;
        private int? testUserSolicitationId2;
        private int? testAppointmentJobOffer1Id;
        private int? testAppointmentAnyJobOfferId;
        private int? testCompanyRepeatAppointment1Id;
        private int? testCompanyRepeatAppointment2Id;
        private int? someOtherCompanyId;

        [SetUp]
        public async Task Setup()
        {
            _inMemoryDb = new SqliteConnection("Filename=:memory:");
            _inMemoryDb.Open();

            // Setup the database in a different context
            using (TestDbContext context = new TestDbContext(_inMemoryDb))
            {
                await context.SetupDatabase();
                await context.AddTestUsersAsync();
                await context.AddTestCompaniesAsync();
                await context.AddTestSolictations();
                // save the id's for later use
                testJobOffer1Id = context.TestCompanyJobOffer1Id;
                testCompanyId = context.TestCompanyId;
                testUserSolicitationId1 = context.TestUserSolicitationTestCompanyWithAppointmentForJobOffer1Id;
                testUserSolicitationId2 = context.TestUserSolicitationTestCompanyForJobOffer2Id;
                testAppointmentJobOffer1Id = context.TestCompanyAppointmentForJobOffer1Id;
                testAppointmentAnyJobOfferId = context.TestCompanyAppointmentForAnyJobOfferId;
                testCompanyRepeatAppointment1Id = context.TestCompanyRepeatAppointment1Id;
                testCompanyRepeatAppointment2Id = context.TestCompanyRepeatAppointment2Id;
                someOtherCompanyId = context.SomeOtherCompanyId;

            }
            _context = new TestDbContext(_inMemoryDb);
            _service = new AppointmentService(_context, _context.UserManager);
        }

        [Test]
        public async Task TestGetAppointments()
        {
            // prepare
            var id = testJobOffer1Id!.Value;
            var user = _context.Users.FirstOrDefault(u => u.Name == "testAdmin")!;
            var userRoles = await _context.UserManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            ClaimsIdentity identity = new ClaimsIdentity(authClaims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var user2 = _context.Users.FirstOrDefault(u => u.Name == "testCompanyUser")!;
            var userRoles2 = await _context.UserManager.GetRolesAsync(user2);
            var authClaims2 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user2.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user2.Id.ToString())
                };
            foreach (var userRole in userRoles2)
            {
                authClaims2.Add(new Claim(ClaimTypes.Role, userRole));
            }
            ClaimsIdentity identity2 = new ClaimsIdentity(authClaims2);
            var claimsPrincipal2 = new ClaimsPrincipal(identity2);

            // run
            var appointments1 = await _service.GetAppointmentsAsync(claimsPrincipal);
            var appointments2 = await _service.GetAppointmentsAsync(claimsPrincipal2);

            // validate
            Assert.That(appointments1, Is.Not.Null);
            Assert.That(appointments2 , Is.Not.Null);

            var appointment1Entry = _context.Entry(appointments1.First()!);
            var company1Entry = _context.Entry(appointments1.First()!.Company);
            var solicitation1Entry = _context.Entry(appointments1.First(a => a.JobOfferId == id).Solicitation!);

            Assert.That(appointment1Entry.Reference(a => a.Company!).IsLoaded, Is.True);
            Assert.That(appointment1Entry.Reference(a => a.Solicitation).IsLoaded, Is.True);
            Assert.That(appointment1Entry.Reference(a => a.JobOffer!).IsLoaded, Is.True);
            Assert.That(company1Entry.Reference(c => c.User!).IsLoaded, Is.True);
            Assert.That(solicitation1Entry.Reference(s => s.User!).IsLoaded, Is.True);

            Assert.That(appointments1.Count, Is.EqualTo(3));
            Assert.That(appointments2.Count, Is.EqualTo(2));

            Assert.That(appointments1.First().Company.Name, Is.EqualTo("TestCompany"));
            Assert.That(appointments1.First(a => a.JobOfferId == id).JobOffer!.Name, Is.EqualTo("Test Job Offer"));
            Assert.That(appointments1.First(a => a.JobOfferId == id).Solicitation!.User.Name, Is.EqualTo("testUser"));
        }

        [Test]
        public async Task TestGetAppointment()
        {
            // prepare
            var id = testAppointmentJobOffer1Id!.Value;
            var idWrong = 1234567890;
            var user = _context.Users.FirstOrDefault(u => u.Name == "testAdmin")!;
            var userRoles = await _context.UserManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            ClaimsIdentity identity = new ClaimsIdentity(authClaims);
            var claimsPrincipalAdmin = new ClaimsPrincipal(identity);

            var user2 = _context.Users.FirstOrDefault(u => u.Name == "testCompanyUser")!;
            var userRoles2 = await _context.UserManager.GetRolesAsync(user2);
            var authClaims2 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user2.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user2.Id.ToString())
                };
            foreach (var userRole2 in userRoles2)
            {
                authClaims2.Add(new Claim(ClaimTypes.Role, userRole2));
            }
            ClaimsIdentity identity2 = new ClaimsIdentity(authClaims2);
            var claimsPrincipalCompanyUser = new ClaimsPrincipal(identity2);

            var user3 = _context.Users.FirstOrDefault(u => u.Name == "someOtherCompanyUser")!;
            var userRoles3 = await _context.UserManager.GetRolesAsync(user3);
            var authClaims3 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user3.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user3.Id.ToString())
                };
            foreach (var userRole3 in userRoles3)
            {
                authClaims3.Add(new Claim(ClaimTypes.Role, userRole3));
            }
            ClaimsIdentity identity3 = new ClaimsIdentity(authClaims3);
            var claimsPrincipalOtherCompanyUser = new ClaimsPrincipal(identity3);

            var user4 = _context.Users.FirstOrDefault(u => u.Name == "testUser")!;
            var userRoles4 = await _context.UserManager.GetRolesAsync(user4);
            var authClaims4 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user4.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user4.Id.ToString())
                };
            foreach (var userRole4 in userRoles4)
            {
                authClaims4.Add(new Claim(ClaimTypes.Role, userRole4));
            }
            ClaimsIdentity identity4 = new ClaimsIdentity(authClaims4);
            var claimsPrincipalCandidate = new ClaimsPrincipal(identity4);

            // run
            var appointment1 = await _service.GetAppointmentAsync(id, claimsPrincipalAdmin);
            var appointment2 = await _service.GetAppointmentAsync(idWrong, claimsPrincipalAdmin);
            var appointment3 = await _service.GetAppointmentAsync(id, claimsPrincipalCompanyUser);
            var appointment4 = await _service.GetAppointmentAsync(id, claimsPrincipalOtherCompanyUser);
            var appointment5 = await _service.GetAppointmentAsync(id, claimsPrincipalCandidate);

            // validate
            Assert.That(appointment1, Is.Not.Null);
            Assert.That(appointment2, Is.Null);
            Assert.That(appointment3, Is.Not.Null);
            Assert.That(appointment4, Is.Null);
            Assert.That(appointment5, Is.Not.Null);

            var appointmentEntry = _context.Entry(appointment1!);
            var solicitationEntry = _context.Entry(appointment1!.Solicitation!);

            Assert.That(appointmentEntry.Reference(a => a.Company!).IsLoaded, Is.True);
            Assert.That(appointmentEntry.Reference(a => a.Solicitation).IsLoaded, Is.True);
            Assert.That(appointmentEntry.Reference(a => a.JobOffer!).IsLoaded, Is.True);
            Assert.That(solicitationEntry.Reference(s => s.User!).IsLoaded, Is.True);

            Assert.That(appointment1.Solicitation!.User.Name, Is.EqualTo("testUser"));
            Assert.That(appointment1.Company.Name, Is.EqualTo("TestCompany"));
            Assert.That(appointment1.JobOffer!.Name, Is.EqualTo("Test Job Offer"));
            Assert.That(appointment1.Duration, Is.EqualTo(TimeSpan.FromMinutes(45)));
            Assert.That(appointment3, Is.EqualTo(appointment1));
            Assert.That(appointment5, Is.EqualTo(appointment1));
        }

        [Test]
        public async Task TestCreateAppointment()
        {
            // prepare
            var user = _context.Users.FirstOrDefault(u => u.Name == "testCompanyUser")!;
            var userRoles = await _context.UserManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            ClaimsIdentity identity = new ClaimsIdentity(authClaims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var testCompany = _context.Company.FirstOrDefault(c => c.Name == "TestCompany")!;

            var appointment = new Appointment()
            {
                Company = testCompany,
                Date = DateTime.Today.AddDays(10),
                Time = DateTime.UnixEpoch.AddHours(4),
                Duration = TimeSpan.FromMinutes(80),
                IsOnline = true,
                JobOffer = null
            };

            // run
            var newAppointment = await _service.CreateAppointmentAsync(appointment, claimsPrincipal);

            // validate
            Assert.That(newAppointment, Is.Not.Null);
            Assert.That(newAppointment.Duration, Is.EqualTo(appointment.Duration));
            Assert.That(newAppointment.Company.Name, Is.EqualTo("TestCompany"));

            var appointments = await _context.Appointment.ToListAsync();
            Assert.That(appointments.Count, Is.EqualTo(4));
        }

        [Test]
        public async Task TestGetCompany()
        {
            // prepare
            var user = _context.Users.FirstOrDefault(u => u.Name == "testCompanyUser")!;
            var userRoles = await _context.UserManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            ClaimsIdentity identity = new ClaimsIdentity(authClaims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // run
            var company = await _service.GetCompanyAsync(claimsPrincipal);

            // validate
            Assert.That(company, Is.Not.Null);
            Assert.That(company.Name, Is.EqualTo("TestCompany"));
        }

        [Test]
        public async Task TestGetJobOffersForSelectList()
        {
            // prepare
            var id = testCompanyId!.Value;
            var company = await _context.Company.FindAsync(id);

            // run
            var jobOffers = _service.GetJobOffersForSelectListAsync(company!);

            // validate
            Assert.That(jobOffers, Is.Not.Null);
            Assert.That(jobOffers.Count, Is.EqualTo(2));
            Assert.That(jobOffers.First()!.Name, Is.EqualTo("Test Job Offer"));
        }

        [Test]
        public async Task TestGetJobOffersForSelectListWithUser()
        {
            // prepare
            var user = _context.Users.FirstOrDefault(u => u.Name == "testCompanyUser")!;
            var userRoles = await _context.UserManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            ClaimsIdentity identity = new ClaimsIdentity(authClaims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // run
            var jobOffers = await _service.GetJobOffersForSelectListWithUserAsync(claimsPrincipal);

            // validate
            Assert.That(jobOffers, Is.Not.Null);
            Assert.That(jobOffers.Count, Is.EqualTo(2));
            Assert.That(jobOffers.First()!.Name, Is.EqualTo("Test Job Offer"));
        }

        [Test]
        public async Task TestUpdateAppointment()
        {
            // prepare
            var id = testAppointmentJobOffer1Id!.Value;
            var appointment = await _context.Appointment.FindAsync(id);
            appointment!.IsOnline = true;
            appointment!.Duration = TimeSpan.FromMinutes(120);

            // run
            await _service.UpdateAppointmentAsync(appointment);

            // validate
            var updatedAppointment = await _context.Appointment.FindAsync(id);
            
            Assert.That(updatedAppointment, Is.Not.Null);
            Assert.That(updatedAppointment.IsOnline, Is.True);
            Assert.That(updatedAppointment.Duration, Is.EqualTo(TimeSpan.FromMinutes(120)));
        }

        [Test]
        public async Task TestDoesAppointmentExist()
        {
            // prepare
            var id = testAppointmentJobOffer1Id!.Value;
            var idWrong = 1234567890;
            var user = _context.Users.FirstOrDefault(u => u.Name == "testAdmin")!;
            var userRoles = await _context.UserManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            ClaimsIdentity identity = new ClaimsIdentity(authClaims);
            var claimsPrincipalAdmin = new ClaimsPrincipal(identity);

            var user2 = _context.Users.FirstOrDefault(u => u.Name == "testCompanyUser")!;
            var userRoles2 = await _context.UserManager.GetRolesAsync(user2);
            var authClaims2 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user2.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user2.Id.ToString())
                };
            foreach (var userRole2 in userRoles2)
            {
                authClaims2.Add(new Claim(ClaimTypes.Role, userRole2));
            }
            ClaimsIdentity identity2 = new ClaimsIdentity(authClaims2);
            var claimsPrincipalCompanyUser = new ClaimsPrincipal(identity2);

            var user3 = _context.Users.FirstOrDefault(u => u.Name == "someOtherCompanyUser")!;
            var userRoles3 = await _context.UserManager.GetRolesAsync(user3);
            var authClaims3 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user3.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user3.Id.ToString())
                };
            foreach (var userRole3 in userRoles3)
            {
                authClaims3.Add(new Claim(ClaimTypes.Role, userRole3));
            }
            ClaimsIdentity identity3 = new ClaimsIdentity(authClaims3);
            var claimsPrincipalOtherCompanyUser = new ClaimsPrincipal(identity3);

            // run
            var result1 = await _service.DoesAppointmentExistAsync(id, claimsPrincipalAdmin);
            var result2 = await _service.DoesAppointmentExistAsync(idWrong, claimsPrincipalAdmin);
            var result3 = await _service.DoesAppointmentExistAsync(id, claimsPrincipalCompanyUser);
            var result4 = await _service.DoesAppointmentExistAsync(id, claimsPrincipalOtherCompanyUser);

            // validate
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.False);
            Assert.That(result3, Is.True);
            Assert.That(result4, Is.False);
        }

        [Test]
        public async Task TestDeleteAppointment()
        {
            // prepare
            var id = testAppointmentJobOffer1Id!.Value;

            // run
            await _service.DeleteAppointmentAsync(id);

            // validate
            var appointment = await _context.Appointment.FindAsync(id);

            Assert.That(appointment, Is.Null);
        }

        [Test]
        public async Task TestGetAvailableAppointments()
        {
            //  prepare
            var id = testUserSolicitationId2!.Value;

            // run
            var appointments = await _service.GetAvailableAppointmentsAsync(id);

            // validate
            Assert.That(appointments, Is.Not.Null);
            Assert.That(appointments.Count, Is.EqualTo(22));
        }

        [Test]
        public async Task TestDoesSolicitationExist()
        {
            // prepare
            var id = testUserSolicitationId1!.Value;
            var idWrong = 1234567890;
            var user = _context.Users.FirstOrDefault(u => u.Name == "testAdmin")!;
            var userRoles = await _context.UserManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            ClaimsIdentity identity = new ClaimsIdentity(authClaims);
            var claimsPrincipalAdmin = new ClaimsPrincipal(identity);

            var user2 = _context.Users.FirstOrDefault(u => u.Name == "testUser")!;
            var userRoles2 = await _context.UserManager.GetRolesAsync(user2);
            var authClaims2 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user2.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user2.Id.ToString())
                };
            foreach (var userRole2 in userRoles2)
            {
                authClaims2.Add(new Claim(ClaimTypes.Role, userRole2));
            }
            ClaimsIdentity identity2 = new ClaimsIdentity(authClaims2);
            var claimsPrincipalTestUser = new ClaimsPrincipal(identity2);

            var user3 = _context.Users.FirstOrDefault(u => u.Name == "TestJustJoinedUser")!;
            var userRoles3 = await _context.UserManager.GetRolesAsync(user3);
            var authClaims3 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user3.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user3.Id.ToString())
                };
            foreach (var userRole3 in userRoles3)
            {
                authClaims3.Add(new Claim(ClaimTypes.Role, userRole3));
            }
            ClaimsIdentity identity3 = new ClaimsIdentity(authClaims3);
            var claimsPrincipalJustJoinedUser = new ClaimsPrincipal(identity3);

            // run
            var result1 = await _service.DoesSolicitationExistAsync(id, claimsPrincipalAdmin);
            var result2 = await _service.DoesSolicitationExistAsync(idWrong, claimsPrincipalAdmin);
            var result3 = await _service.DoesSolicitationExistAsync(id, claimsPrincipalTestUser);
            var result4 = await _service.DoesSolicitationExistAsync(id, claimsPrincipalJustJoinedUser);

            // validate
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.False);
            Assert.That(result3, Is.True);
            Assert.That(result4, Is.False);
        }

        [Test]
        public async Task TestSelectAppointment()
        {
            // prepare
            var appointmentId = testAppointmentAnyJobOfferId!.Value;
            var solicitationId = testUserSolicitationId2!.Value;

            // run
            await _service.SelectAppointmentAsync(appointmentId, solicitationId);

            // validate
            var solicitation = await _context.Solicitation.FindAsync(solicitationId);
            var appointment = await _context.Appointment.FindAsync(appointmentId);

            Assert.That(solicitation, Is.Not.Null);
            Assert.That(appointment, Is.Not.Null);

            Assert.That(solicitation.AppointmentId, Is.EqualTo(appointmentId));
            Assert.That(appointment.Solicitation, Is.EqualTo(solicitation));
        }

        [Test]
        public async Task TestDeleteOldAppointments()
        {
            // prepare
            var user = _context.Users.FirstOrDefault(u => u.Name == "testCompanyUser")!;
            var userRoles = await _context.UserManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            ClaimsIdentity identity = new ClaimsIdentity(authClaims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // run
            await _service.DeleteOldOpenAppointmentsAsync(claimsPrincipal);

            // validate
            var appointments = await _context.Appointment.ToListAsync();

            Assert.That(appointments, Is.Not.Null);
            Assert.That(appointments.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task TestGetRepeatAppointmentAsync()
        {
            // prepare
            int id = testCompanyRepeatAppointment1Id!.Value;
            int idWrong = 1234567890;
            var user = _context.Users.FirstOrDefault(u => u.Name == "testAdmin")!;
            var userRoles = await _context.UserManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            ClaimsIdentity identity = new ClaimsIdentity(authClaims);
            var claimsPrincipalAdmin = new ClaimsPrincipal(identity);

            var user2 = _context.Users.FirstOrDefault(u => u.Name == "testCompanyUser")!;
            var userRoles2 = await _context.UserManager.GetRolesAsync(user2);
            var authClaims2 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user2.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user2.Id.ToString())
                };
            foreach (var userRole2 in userRoles2)
            {
                authClaims2.Add(new Claim(ClaimTypes.Role, userRole2));
            }
            ClaimsIdentity identity2 = new ClaimsIdentity(authClaims2);
            var claimsPrincipalCompanyUser = new ClaimsPrincipal(identity2);

            var user3 = _context.Users.FirstOrDefault(u => u.Name == "someOtherCompanyUser")!;
            var userRoles3 = await _context.UserManager.GetRolesAsync(user3);
            var authClaims3 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user3.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user3.Id.ToString())
                };
            foreach (var userRole3 in userRoles3)
            {
                authClaims3.Add(new Claim(ClaimTypes.Role, userRole3));
            }
            ClaimsIdentity identity3 = new ClaimsIdentity(authClaims3);
            var claimsPrincipalOtherCompanyUser = new ClaimsPrincipal(identity3);

            // run
            var repeatAppointment1 = await _service.GetRepeatAppointmentAsync(id, claimsPrincipalAdmin);
            var repeatAppointment2 = await _service.GetRepeatAppointmentAsync(idWrong, claimsPrincipalAdmin);
            var repeatAppointment3 = await _service.GetRepeatAppointmentAsync(id, claimsPrincipalCompanyUser);
            var repeatAppointment4 = await _service.GetRepeatAppointmentAsync(id, claimsPrincipalOtherCompanyUser);

            // validate
            Assert.That(repeatAppointment1, Is.Not.Null);
            Assert.That(repeatAppointment2, Is.Null);
            Assert.That(repeatAppointment3, Is.Not.Null);
            Assert.That(repeatAppointment4, Is.Null);

            Assert.That(repeatAppointment1.Repeats, Is.EqualTo(RepeatAppointment.RepeatsType.Daily));
            Assert.That(repeatAppointment3, Is.EqualTo(repeatAppointment1));
        }

        [Test]
        public async Task TestGetRepeatAppointmentsAsync()
        {
            // prepare
            var user = _context.Users.FirstOrDefault(u => u.Name == "testAdmin")!;
            var userRoles = await _context.UserManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            ClaimsIdentity identity = new ClaimsIdentity(authClaims);
            var claimsPrincipalAdmin = new ClaimsPrincipal(identity);

            var user2 = _context.Users.FirstOrDefault(u => u.Name == "testCompanyUser")!;
            var userRoles2 = await _context.UserManager.GetRolesAsync(user2);
            var authClaims2 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user2.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user2.Id.ToString())
                };
            foreach (var userRole2 in userRoles2)
            {
                authClaims2.Add(new Claim(ClaimTypes.Role, userRole2));
            }
            ClaimsIdentity identity2 = new ClaimsIdentity(authClaims2);
            var claimsPrincipalCompanyUser = new ClaimsPrincipal(identity2);

            // run
            var repeatAppointments1 = await _service.GetRepeatAppointmentsAsync(claimsPrincipalAdmin);
            var repeatAppointments2 = await _service.GetRepeatAppointmentsAsync(claimsPrincipalCompanyUser);

            // validate
            Assert.That(repeatAppointments1, Is.Not.Null);
            Assert.That(repeatAppointments2, Is.Not.Null);

            Assert.That(repeatAppointments1.Count(), Is.EqualTo(2));
            Assert.That(repeatAppointments2.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task TestCreateRepeatAppointmentAsync()
        {
            // prepare
            var companyId = testCompanyId!.Value;
            var company = await _context.Company.FindAsync(companyId);
            var user2 = _context.Users.FirstOrDefault(u => u.Name == "testCompanyUser")!;
            var userRoles2 = await _context.UserManager.GetRolesAsync(user2);
            var authClaims2 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user2.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user2.Id.ToString())
                };
            foreach (var userRole2 in userRoles2)
            {
                authClaims2.Add(new Claim(ClaimTypes.Role, userRole2));
            }
            ClaimsIdentity identity2 = new ClaimsIdentity(authClaims2);
            var claimsPrincipalCompanyUser = new ClaimsPrincipal(identity2);

            var repeatAppointment = new RepeatAppointment()
            {
                CompanyId = companyId,
                Company = company,
                Repeats = RepeatAppointment.RepeatsType.MonthlyRelative,
                RepeatsDay = 10,
                Time = DateTime.UnixEpoch.AddHours(16),
                Duration = TimeSpan.FromMinutes(75),
                IsOnline = true,
            };

            // run
            var createdRepeatAppointment = await _service.CreateRepeatAppointmentAsync(repeatAppointment, claimsPrincipalCompanyUser);

            // validate
            var repeatAppointmentsCheck = await _context.RepeatAppointment.Where(ra => ra.CompanyId == companyId).ToListAsync();

            Assert.That(createdRepeatAppointment, Is.Not.Null);
            Assert.That(repeatAppointmentsCheck, Is.Not.Null);
            Assert.That(repeatAppointmentsCheck.Count(), Is.EqualTo(3));
            Assert.That(createdRepeatAppointment.Repeats, Is.EqualTo(RepeatAppointment.RepeatsType.MonthlyRelative));
            Assert.That(createdRepeatAppointment.Id, Is.Not.EqualTo(null));
        }

        [Test]
        public async Task TestUpdateRepeatAppointmentAsync()
        {
            // prepare
            var id = testCompanyRepeatAppointment1Id!.Value;
            var repeatAppointment = await _context.RepeatAppointment.FindAsync(id);

            repeatAppointment.Repeats = RepeatAppointment.RepeatsType.MonthlyRelative;
            repeatAppointment.RepeatsDay = 20;

            var id2 = testCompanyRepeatAppointment2Id!.Value;
            var repeatAppointment2 = await _context.RepeatAppointment.FindAsync(id2);

            repeatAppointment2.RepeatsWeekdays = RepeatAppointment.Repeats_Weekdays.Monday;

            // run
            await _service.UpdateRepeatAppointmentAsync(repeatAppointment);
            await _service.UpdateRepeatAppointmentAsync(repeatAppointment2);

            // validate
            var repeatAppointmentCheck = await _context.RepeatAppointment.FindAsync(id);
            var repeatAppointmentCheck2 = await _context.RepeatAppointment.FindAsync(id2);

            Assert.That(repeatAppointmentCheck, Is.Not.Null);
            Assert.That(repeatAppointmentCheck2, Is.Not.Null);

            Assert.That(repeatAppointmentCheck.Repeats, Is.EqualTo(RepeatAppointment.RepeatsType.MonthlyRelative));
            Assert.That(repeatAppointmentCheck.RepeatsDay, Is.Null);

            Assert.That(repeatAppointmentCheck2.Repeats, Is.EqualTo(RepeatAppointment.RepeatsType.Weekly));
            Assert.That(repeatAppointmentCheck2.RepeatsWeekdays, Is.EqualTo(RepeatAppointment.Repeats_Weekdays.Monday));
        }

        [Test]
        public async Task TestDoesRepeatAppointExistAsync()
        {
            // prepare
            var id = testCompanyRepeatAppointment1Id!.Value;
            var idWrong = 1234567890;
            var user = _context.Users.FirstOrDefault(u => u.Name == "testAdmin")!;
            var userRoles = await _context.UserManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            ClaimsIdentity identity = new ClaimsIdentity(authClaims);
            var claimsPrincipalAdmin = new ClaimsPrincipal(identity);

            var user2 = _context.Users.FirstOrDefault(u => u.Name == "testCompanyUser")!;
            var userRoles2 = await _context.UserManager.GetRolesAsync(user2);
            var authClaims2 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user2.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user2.Id.ToString())
                };
            foreach (var userRole2 in userRoles2)
            {
                authClaims2.Add(new Claim(ClaimTypes.Role, userRole2));
            }
            ClaimsIdentity identity2 = new ClaimsIdentity(authClaims2);
            var claimsPrincipalCompanyUser = new ClaimsPrincipal(identity2);

            var user3 = _context.Users.FirstOrDefault(u => u.Name == "someOtherCompanyUser")!;
            var userRoles3 = await _context.UserManager.GetRolesAsync(user3);
            var authClaims3 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user3.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user3.Id.ToString())
                };
            foreach (var userRole3 in userRoles3)
            {
                authClaims3.Add(new Claim(ClaimTypes.Role, userRole3));
            }
            ClaimsIdentity identity3 = new ClaimsIdentity(authClaims3);
            var claimsPrincipalOtherCompanyUser = new ClaimsPrincipal(identity3);

            // run
            var result1 = await _service.DoesRepeatAppointmentExistAsync(id, claimsPrincipalAdmin);
            var result2 = await _service.DoesRepeatAppointmentExistAsync(idWrong, claimsPrincipalAdmin);
            var result3 = await _service.DoesRepeatAppointmentExistAsync(id, claimsPrincipalCompanyUser);
            var result4 = await _service.DoesRepeatAppointmentExistAsync(id, claimsPrincipalOtherCompanyUser);

            // validate
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.False);
            Assert.That(result3, Is.True);
            Assert.That(result4, Is.False);
        }

        [Test]
        public async Task TestDeleteRepeatAppointmentAsync()
        {
            // prepare
            var id = testCompanyRepeatAppointment1Id!.Value;

            // run
            await _service.DeleteRepeatAppointmentAsync(id);

            // validate
            var repeatAppointments = await _context.RepeatAppointment.CountAsync();
            Assert.That(repeatAppointments, Is.EqualTo(1));
        }

        [Test]
        public async Task TestGetAvailableRepeatAppointmentsAsync()
        {
            // prepare
            var id = testCompanyId!.Value;
            var id2 = someOtherCompanyId!.Value;
            var list = new List<Appointment>();
            var list2 = new List<Appointment>();

            // run
            var availableAppointments1 = await _service.GetAvailableRepeatAppointmentsAsync(list, id);
            var availableAppointments2 = await _service.GetAvailableRepeatAppointmentsAsync(list2, id2);

            // validate
            Assert.That(availableAppointments1, Is.Not.Empty);
            Assert.That(availableAppointments2, Is.Empty);
        }

        [Test]
        public async Task TestSelectRepeatAppointmentAsync()
        {
            // prepare
            var id = testCompanyId!.Value;
            var solicitationId = testUserSolicitationId2!.Value;
            var list = new List<Appointment>();
            var list2 = new List<Appointment>();
            var availableAppointments1 = await _service.GetAvailableRepeatAppointmentsAsync(list, id);
            var count1 = availableAppointments1.Count();

            var appointmentFromSelect = availableAppointments1.First();

            // run
            await _service.SelectRepeatAppointmentAsync((int)appointmentFromSelect.RepeatAppointmentId!, appointmentFromSelect.Time, solicitationId);

            var availableAppointments2 = await _service.GetAvailableRepeatAppointmentsAsync(list2, id);
            // validate
            Assert.That(availableAppointments2, Is.Not.Empty);
            Assert.That(availableAppointments2.Count(), Is.EqualTo(count1 - 1));
        }

    }
}
