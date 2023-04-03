using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VAC_T.Business;
using VAC_T.Models;
using VAC_T.UnitTest.TestObjects;

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

            // run
            var appointment1 = await _service.GetAppointmentAsync(id);
            var appointment2 = await _service.GetAppointmentAsync(idWrong);

            // validate
            Assert.That(appointment1, Is.Not.Null);
            Assert.That(appointment2, Is.Null);

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

            // run
            var result1 = await _service.DoesAppointmentExistAsync(id);
            var result2 = await _service.DoesAppointmentExistAsync(idWrong);

            // validate
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.False);
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
            Assert.That(appointments.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task TestDoesSolicitationExist()
        {
            // prepare
            var id = testUserSolicitationId1!.Value;
            var idWrong = 1234567890;

            // run
            var result1 = await _service.DoesSolicitationExistsAsync(id);
            var result2 = await _service.DoesSolicitationExistsAsync(idWrong);

            // validate
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.False);
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

    }
}
