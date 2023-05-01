using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using VAC_T.Business;
using VAC_T.Models;
using VAC_T.UnitTest.TestObjects;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace VAC_T.UnitTest.Services
{
    internal class DashBoardServiceTest
    {
        private SqliteConnection _inMemoryDb;
        private TestDbContext _context;
        private DashBoardService _service;
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
            _service = new DashBoardService(_context, _context.UserManager);
        }

        [Test]
        public async Task TestGetSolicitationsAsync()
        {
            //prepare
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

            var user3 = _context.Users.FirstOrDefault(u => u.Name == "testUser")!;
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
            var claimsPrincipalUser = new ClaimsPrincipal(identity3);

            // run
            var solicitations1 = await _service.GetSolicitationsAsync(claimsPrincipalAdmin);
            var solicitations2 = await _service.GetSolicitationsAsync(claimsPrincipalCompanyUser);
            var solicitations3 = await _service.GetSolicitationsAsync(claimsPrincipalUser);

            var company = _context.Company.FirstOrDefault(c => c.User == user2);
            // validate
            Assert.That(solicitations1, Is.Not.Null);
            Assert.That(solicitations2, Is.Not.Null);
            Assert.That(solicitations3 , Is.Not.Null);

            var solicitationsEntry = _context.Entry(solicitations1.First()!);

            // Test eager and lazy loaded fields
            Assert.That(solicitationsEntry.Reference(s => s.JobOffer).IsLoaded, Is.True);


            Assert.That(solicitations2.First().JobOffer.CompanyId, Is.EqualTo(company.Id));
            Assert.That(solicitations3.First().UserId, Is.EqualTo(user3.Id));

            Assert.That(solicitations1.Count(), Is.EqualTo(2));
            Assert.That(solicitations2.Count(), Is.EqualTo(2));
            Assert.That(solicitations3.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task TestGetCompanyAsync()
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

            var companyEntry = _context.Entry(company);

            // Test eager and lazy loaded fields
            Assert.That(companyEntry.Collection(c => c.JobOffers).IsLoaded, Is.True);
            Assert.That(companyEntry.Collection(c => c.Appointments).IsLoaded, Is.True);
            Assert.That(companyEntry.Collection(c => c.RepeatAppointments).IsLoaded, Is.True);

            Assert.That(company.User, Is.EqualTo(user));
        }

        [Test]
        public async Task TestGetCompanyAppointmentsAsync()
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
            var appointments = await _service.GetCompanyAppointmentsAsync(claimsPrincipal);

            var company = _context.Company.FirstOrDefault(c => c.User == user);
            // validate
            Assert.That(appointments, Is.Not.Null);

            var appointmentsEntry = _context.Entry(appointments!.First());

            // Test eager and lazy loaded fields
            Assert.That(appointmentsEntry.Reference(a => a.RepeatAppointment).IsLoaded, Is.True);

            Assert.That(appointments.First().CompanyId, Is.EqualTo(company.Id));
            Assert.That(appointments.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task TestGetAmountRepeatAppointmentsLast6MonthsAsync()
        {
            // prepare
            var id = testCompanyId!.Value;

            // run
            var repeatAmounts = await _service.GetAmountRepeatAppointmentsLast6MonthsAsync(id);

            // validate
            Assert.That(repeatAmounts, Is.Not.Null);
            Assert.That(repeatAmounts[RepeatAppointment.RepeatsType.Daily], Is.EqualTo(130));
            Assert.That(repeatAmounts[RepeatAppointment.RepeatsType.Weekly], Is.EqualTo(52));
            Assert.That(repeatAmounts[RepeatAppointment.RepeatsType.Monthly], Is.EqualTo(0));
            Assert.That(repeatAmounts[RepeatAppointment.RepeatsType.MonthlyRelative], Is.EqualTo(0));
        }

        [Test]
        public async Task TestGetJobOffersAsync()
        {
            // run
            var jobOffers = await _service.GetJobOffersAsync();

            // validate
            Assert.That(jobOffers, Is.Not.Null);
            Assert.That(jobOffers.Count(), Is.EqualTo(2));
        }

    }
}
