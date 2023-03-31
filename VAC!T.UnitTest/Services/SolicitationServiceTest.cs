using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VAC_T.Business;
using VAC_T.UnitTest.TestObjects;

namespace VAC_T.UnitTest.Services
{
    internal class SolicitationServiceTest
    {
        private SqliteConnection _inMemoryDb;
        private TestDbContext _context;
        private SolicitationService _service;
        private int? testJobOffer1Id;
        private int? testJobOffer2Id;
        private int? testCompanyId;
        private int? testUserSolicitationId1;
        private int? testUserSolicitationId2;

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
                testJobOffer2Id = context.TestCompanyJobOffer2Id;
                testCompanyId = context.TestCompanyId;
                testUserSolicitationId1 = context.TestUserSolicitationTestCompanyWithAppointmentForJobOffer1Id;
                testUserSolicitationId2 = context.TestUserSolicitationTestCompanyForJobOffer2Id;

            }
            _context = new TestDbContext(_inMemoryDb);
            _service = new SolicitationService(_context, _context.UserManager);
        }

        [Test]
        public async Task TestGetSolicitationsForCandidate()
        {
            // prepare
            var user = _context.Users.FirstOrDefault(u => u.Name == "testUser")!;
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
            var solicitations = await _service.GetSolicitationsAsync(claimsPrincipal, null, null, null, null, null);

            // validate
            Assert.That(solicitations, Is.Not.Null);

            var solicitationsEntry = _context.Entry(solicitations.First()!);
            var jobOfferEntry = _context.Entry(solicitations.First()!.JobOffer);

            // Test eager and lazy loaded fields
            Assert.That(solicitationsEntry.Reference(s => s.Appointment!).IsLoaded, Is.True);
            Assert.That(solicitationsEntry.Reference(s => s.JobOffer!).IsLoaded, Is.True);
            Assert.That(jobOfferEntry.Reference(j => j.Company!).IsLoaded, Is.True);

            Assert.That(solicitations.Where(s => s.JobOffer.Name == "Test Job Offer").First()!.Appointment, Is.Not.Null);
            Assert.That(solicitations.First()!.JobOffer, Is.Not.Null);
            Assert.That(solicitations.First()!.JobOffer.Company, Is.Not.Null);
            Assert.That(solicitations.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task TestGetSolicitationsForCandidateNoSolicitations()
        {
            // prepare
            var user = _context.Users.FirstOrDefault(u => u.Name == "TestJustJoinedUser")!;
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
            var solicitations = await _service.GetSolicitationsAsync(claimsPrincipal, null, null, null, null, null);

            // validate
            Assert.That(solicitations, Is.Empty);
        }


            [Test]
        public async Task TestGetSolicitationsForEmployer()
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
            var solicitations = await _service.GetSolicitationsAsync(claimsPrincipal, null, null, null, null, null);


            // validate
            Assert.That(solicitations, Is.Not.Null);
            var solicitationsEntry = _context.Entry(solicitations.First()!);
            var jobOfferEntry = _context.Entry(solicitations.First()!.JobOffer);

            // Test eager and lazy loaded fields
            Assert.That(solicitationsEntry.Reference(s => s.Appointment!).IsLoaded, Is.True);
            Assert.That(solicitationsEntry.Reference(s => s.JobOffer!).IsLoaded, Is.True);
            Assert.That(jobOfferEntry.Reference(j => j.Company!).IsLoaded, Is.True);
            Assert.That(solicitationsEntry.Reference(s => s.User!).IsLoaded, Is.True);

            Assert.That(solicitations.Where(s => s.JobOffer.Name == "Test Job Offer").First()!.Appointment, Is.Not.Null);
            Assert.That(solicitations.First()!.JobOffer, Is.Not.Null);
            Assert.That(solicitations.First()!.JobOffer.Company, Is.Not.Null);
            Assert.That(solicitations.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task TestGetSolicitationsForDifferentEmployer()
        {
            // prepare
            var user = _context.Users.FirstOrDefault(u => u.Name == "someOtherCompanyUser")!;
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
            var solicitations = await _service.GetSolicitationsAsync(claimsPrincipal, null, null, null, null, null);


            // validate
            Assert.That(solicitations, Is.Empty);
        }

        [Test]
        public async Task TestGetSolicitationsForAdmin()
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
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // run
            var solicitations = await _service.GetSolicitationsAsync(claimsPrincipal, null, null, null, null, null);


            // validate
            Assert.That(solicitations, Is.Not.Null);
            var solicitationsEntry = _context.Entry(solicitations.First()!);
            var jobOfferEntry = _context.Entry(solicitations.First()!.JobOffer);

            // Test eager and lazy loaded fields
            Assert.That(solicitationsEntry.Reference(s => s.Appointment!).IsLoaded, Is.True);
            Assert.That(solicitationsEntry.Reference(s => s.JobOffer!).IsLoaded, Is.True);
            Assert.That(jobOfferEntry.Reference(j => j.Company!).IsLoaded, Is.True);
            Assert.That(solicitationsEntry.Reference(s => s.User!).IsLoaded, Is.True);

            Assert.That(solicitations.Where(s => s.JobOffer.Name == "Test Job Offer").First()!.Appointment, Is.Not.Null);
            Assert.That(solicitations.First()!.JobOffer, Is.Not.Null);
            Assert.That(solicitations.First()!.JobOffer.Company, Is.Not.Null);
            Assert.That(solicitations.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task TestGetSolicitationsForSearchOptions()
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
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // run
            var solicitations = await _service.GetSolicitationsAsync(claimsPrincipal, null, null, null, null, null);
            var solicitationsWithSearchJobOffer = await _service.GetSolicitationsAsync(claimsPrincipal, "Test Job Offer", null, null, null, null);
            var solicitationsWithSearchWrongJobOffer = await _service.GetSolicitationsAsync(claimsPrincipal, "Wrong Test Job Offer", null, null, null, null);
            var solicitationsWithSearchCompany = await _service.GetSolicitationsAsync(claimsPrincipal, null, "TestCompany", null, null, null);
            var solicitationsWithSearchWrongCompany = await _service.GetSolicitationsAsync(claimsPrincipal, null, "Some Other Company", null, null, null);
            var solicitationsWithSearchUser = await _service.GetSolicitationsAsync(claimsPrincipal, null, null, "testUser", null, null);
            var solicitationsWithSearchWrongUser = await _service.GetSolicitationsAsync(claimsPrincipal, null, null, "notARealUser", null, null);
            var solicitationsWithSearchSelected = await _service.GetSolicitationsAsync(claimsPrincipal, null, null, null, true, null);
            var solicitationsWithSearchSelectedFalse = await _service.GetSolicitationsAsync(claimsPrincipal, null, null, null, null, true);


            // validate
            Assert.That(solicitations, Is.Not.Null);
            Assert.That(solicitationsWithSearchJobOffer, Is.Not.Null);
            Assert.That(solicitationsWithSearchWrongJobOffer, Is.Empty);
            Assert.That(solicitationsWithSearchCompany, Is.Not.Null);
            Assert.That(solicitationsWithSearchWrongCompany, Is.Empty);
            Assert.That(solicitationsWithSearchUser, Is.Not.Null);
            Assert.That(solicitationsWithSearchWrongUser, Is.Empty);
            Assert.That(solicitationsWithSearchSelected, Is.Not.Null);
            Assert.That(solicitationsWithSearchSelectedFalse, Is.Not.Null);
            var solicitationsEntry = _context.Entry(solicitations.First()!);
            var jobOfferEntry = _context.Entry(solicitations.First()!.JobOffer);

            // Test eager and lazy loaded fields
            Assert.That(solicitationsEntry.Reference(s => s.Appointment!).IsLoaded, Is.True);
            Assert.That(solicitationsEntry.Reference(s => s.JobOffer!).IsLoaded, Is.True);
            Assert.That(jobOfferEntry.Reference(j => j.Company!).IsLoaded, Is.True);
            Assert.That(solicitationsEntry.Reference(s => s.User!).IsLoaded, Is.True);

            Assert.That(solicitations.Where(s => s.JobOffer.Name == "Test Job Offer").First()!.Appointment, Is.Not.Null);
            Assert.That(solicitations.First()!.JobOffer, Is.Not.Null);
            Assert.That(solicitations.First()!.JobOffer.Company, Is.Not.Null);
            Assert.That(solicitations.Count, Is.EqualTo(2));
            Assert.That(solicitationsWithSearchJobOffer.Count, Is.EqualTo(1));
            Assert.That(solicitationsWithSearchCompany.Count, Is.EqualTo(2));
            Assert.That(solicitationsWithSearchUser.Count, Is.EqualTo(2));
            Assert.That(solicitationsWithSearchSelected.Count, Is.EqualTo(1));
            Assert.That(solicitationsWithSearchSelectedFalse.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task TestCreateSolicitation()
        {
            // prepare
            var id = testJobOffer1Id!.Value;
            var user = _context.Users.FirstOrDefault(u => u.Name == "TestJustJoinedUser")!;
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
            var solicitation = await _service.CreateSolicitationAsync(id, claimsPrincipal);
            
            // valididate
            Assert.That(solicitation, Is.Not.Null);
            var solicitationEntry = _context.Entry(solicitation!);

            Assert.That(solicitation.User.Name, Is.EqualTo(user.Name));
            Assert.That(solicitation.JobOffer.Name, Is.EqualTo("Test Job Offer"));
        }

        [Test]
        public async Task TestCreateSolicitationAlreadySolicitated()
        {
            // prepare
            var id = testJobOffer1Id!.Value;
            var user = _context.Users.FirstOrDefault(u => u.Name == "testUser")!;
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
            var solicitation = await _service.CreateSolicitationAsync(id, claimsPrincipal);

            // valididate
            Assert.That(solicitation, Is.Null);
        }

        [Test]
        public async Task TestDeleteSolicitation()
        {
            //prepare
            var user = _context.Users.FirstOrDefault(u => u.Name == "testUser")!;
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
            var id = testJobOffer1Id!.Value;

            // run
            await _service.DeleteSolicitationAsync(id, claimsPrincipal);

            var solicitation = await _context.Solicitation.ToListAsync();
            // valididate
            Assert.That(solicitation, Is.Not.Null);
            Assert.That(solicitation.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task TestDoesJobOfferExist()
        {
            // prepare
            var id = testJobOffer1Id!.Value;
            var wrongId = 1234567890;

            // run
            var result1 = await _service.DoesJobOfferExistsAsync(id);
            var result2 = await _service.DoesJobOfferExistsAsync(wrongId);

            // validate
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.False);
        }

        [Test]
        public async Task TestDoesSolicitaionExistWithJobOfferId()
        {
            // prepare
            var id = testJobOffer1Id!.Value;
            var user = _context.Users.FirstOrDefault(u => u.Name == "testUser")!; // user that gives true as result
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

            var user2 = _context.Users.FirstOrDefault(u => u.Name == "TestJustJoinedUser")!; // user that gives false as result
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
            var result1 = await _service.DoesSolicitationExistWithJobOfferIdAsync(id, claimsPrincipal);
            var result2 = await _service.DoesSolicitationExistWithJobOfferIdAsync(id, claimsPrincipal2);

            // validate
            Assert.That(result1, Is.True);
            Assert.That(result2 , Is.False);
        }

        [Test]
        public async Task TestSelectSolicitation()
        {
            //prepare
            var id1 = testUserSolicitationId1!.Value;
            var id2 = testUserSolicitationId2!.Value;

            // run
            await _service.SelectSolicitationAsync(id1);
            await _service.SelectSolicitationAsync(id2);

            // valididate
            var solicitation1 = await _context.Solicitation.FindAsync(id1);
            var solicitation2 = await _context.Solicitation.FindAsync(id2);

            Assert.That(solicitation1, Is.Not.Null);
            Assert.That(solicitation2, Is.Not.Null);

            Assert.That(solicitation1.Selected, Is.False);
            Assert.That(solicitation2.Selected, Is.True);
        }

        [Test]
        public async Task TestDoesSolicitationExist()
        {
            // prepare
            var id1 = testUserSolicitationId1!.Value;
            var id2 = 1234567890;

            // run
            var result1 = await _service.DoesSolicitationExistsAsync(id1);
            var result2 = await _service.DoesSolicitationExistsAsync(id2);

            // validate
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.False);
        }

    }
}
