using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VAC_T.Business;
using VAC_T.Models;
using VAC_T.UnitTest.TestObjects;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace VAC_T.UnitTest.Services
{
    internal class JobOfferServiceTest
    {
        private SqliteConnection _inMemoryDb;
        private TestDbContext _context;
        private JobOfferService _service;
        private int? testJobOffer1Id;
        private int? testJobOffer2Id;
        private int? testCompanyId;
        private int? someOtherCompanyId;
        private int? testQuestion1Id;
        private int? testQuestion2Id;

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
                await context.AddTestQuestionsAsync();
                // save the id's for later use
                testJobOffer1Id = context.TestCompanyJobOffer1Id;
                testJobOffer2Id = context.TestCompanyJobOffer2Id;
                testCompanyId = context.TestCompanyId;
                someOtherCompanyId = context.SomeOtherCompanyId;
                testQuestion1Id = context.TestCompanyQuestion1Id;
                testQuestion2Id = context.TestCompanyQuestion2Id;
            }
            _context = new TestDbContext(_inMemoryDb);
            _service = new JobOfferService(_context, _context.UserManager);
        }

        [Test]
        public async Task TestGetJobOffers()
        {
            // prepare
            var user = _context.Users.FirstOrDefault(u => u.Name== "testAdmin")!;
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
            var jobOffers = await _service.GetJobOffersAsync(claimsPrincipal);

            // validate
            Assert.That(jobOffers, Is.Not.Null);

            var jobOffersEntry = _context.Entry(jobOffers.First()!);
            // Test eager and lazy loaded fields
            Assert.That(jobOffersEntry.Reference(c => c.Company).IsLoaded, Is.True);
            Assert.That(jobOffersEntry.Collection(c => c.Solicitations!).IsLoaded, Is.False);

            Assert.That(jobOffers.First().Company, Is.Not.Null);
            Assert.That(jobOffers.First().Solicitations, Is.Null.Or.Empty);
            Assert.That(jobOffers.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task TestGetJobOffersEmployer()
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
            var jobOffers = await _service.GetJobOffersAsync(claimsPrincipal);

            // validate
            Assert.That(jobOffers, Is.Not.Null);

            var jobOffersEntry = _context.Entry(jobOffers.First()!);
            // Test eager and lazy loaded fields
            Assert.That(jobOffersEntry.Reference(c => c.Company).IsLoaded, Is.True);
            Assert.That(jobOffersEntry.Collection(c => c.Solicitations!).IsLoaded, Is.False);

            Assert.That(jobOffers.First().Company, Is.Not.Null);
            Assert.That(jobOffers.First().Solicitations, Is.Null.Or.Empty);
            Assert.That(jobOffers.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task TestGetJobOffersEmployerNoJobOffers()
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
            var jobOffers = await _service.GetJobOffersAsync(claimsPrincipal);

            // validate
            Assert.That(jobOffers, Is.Empty);
        }

        [Test]
        public async Task TestGetJobOffersNotLoggedIn()
        {
            // prepare
            ClaimsIdentity identity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // run
            var jobOffers = await _service.GetJobOffersAsync(claimsPrincipal);

            // validate
            Assert.That(jobOffers, Is.Not.Null);

            var jobOffersEntry = _context.Entry(jobOffers.First()!);
            // Test eager and lazy loaded fields
            Assert.That(jobOffersEntry.Reference(c => c.Company).IsLoaded, Is.True);
            Assert.That(jobOffersEntry.Collection(c => c.Solicitations!).IsLoaded, Is.False);

            Assert.That(jobOffers.First().Company, Is.Not.Null);
            Assert.That(jobOffers.First().Solicitations, Is.Null.Or.Empty);
            Assert.That(jobOffers.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task TestGetJobOffer()
        {
            // prepare
            int id = testJobOffer1Id!.Value;
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
            var jobOffer = await _service.GetJobOfferAsync(id, claimsPrincipal);
            var jobOffer2 = await _service.GetJobOfferAsync(id, claimsPrincipalCompanyUser);
            var jobOffer3 = await _service.GetJobOfferAsync(id, claimsPrincipalOtherCompanyUser);

            // validate
            Assert.That(jobOffer, Is.Not.Null);
            Assert.That(jobOffer2, Is.Not.Null);
            Assert.That(jobOffer3, Is.Null);
            var jobOfferEntry = _context.Entry(jobOffer!);
            Assert.That(jobOffer.Company, Is.Not.Null);
            var companyEntry = _context.Entry(jobOffer!.Company);

            Assert.That(jobOffer.Name, Is.EqualTo("Test Job Offer"));
            // Test eager and lazy loaded fields
            Assert.That(jobOfferEntry.Reference(j => j.Company!).IsLoaded, Is.True);
            Assert.That(jobOfferEntry.Collection(j => j.Solicitations!).IsLoaded, Is.True);
            Assert.That(companyEntry.Collection(c => c.JobOffers!).IsLoaded, Is.True);

            Assert.That(jobOffer.Company.Name, Is.EqualTo("TestCompany"));

            Assert.That(jobOffer.Company.JobOffers, Is.Not.Null);
            Assert.That(jobOffer.Company.JobOffers.Count, Is.EqualTo(2));

            Assert.That(jobOffer.Solicitations, Is.Not.Null);
            Assert.That(jobOffer.Solicitations.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task TestGetJobOfferNotLoggedIn()
        {
            // prepare
            int id = testJobOffer1Id!.Value;
            ClaimsIdentity identity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // run
            var jobOffer = await _service.GetJobOfferAsync(id, claimsPrincipal);

            // validate
            Assert.That(jobOffer, Is.Not.Null);
            var jobOfferEntry = _context.Entry(jobOffer!);
            Assert.That(jobOffer.Company, Is.Not.Null);
            var companyEntry = _context.Entry(jobOffer!.Company);

            Assert.That(jobOffer.Name, Is.EqualTo("Test Job Offer"));
            // Test eager and lazy loaded fields
            Assert.That(jobOfferEntry.Reference(j => j.Company!).IsLoaded, Is.True);
            Assert.That(jobOfferEntry.Collection(j => j.Solicitations!).IsLoaded, Is.True);
            Assert.That(companyEntry.Collection(c => c.JobOffers!).IsLoaded, Is.True);

            Assert.That(jobOffer.Company.Name, Is.EqualTo("TestCompany"));

            Assert.That(jobOffer.Company.JobOffers, Is.Not.Null);
            Assert.That(jobOffer.Company.JobOffers.Count, Is.EqualTo(2));

            Assert.That(jobOffer.Solicitations, Is.Empty);
        }

        [Test]
        public async Task TestGetJobOfferWrongId()
        {
            // prepare
            int id = 123465789;
            ClaimsIdentity identity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // run
            var jobOffer = await _service.GetJobOfferAsync(id, claimsPrincipal);

            // validate
            Assert.That(jobOffer, Is.Null);
        }



        [Test]
        public async Task TestCreateJobOffer()
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

            //var company = await _context.Company.FindAsync(testCompanyId!.Value);

            var jobOffer = new JobOffer()
            {
                Name = "Test Create JobOffer",
                Description = "Test for creating a jobOffer",
                CompanyId = testCompanyId!.Value,
                //Company = company,
                Created = DateTime.Now.AddDays(-10),
                Level = "Beginner",
                Residence = "TestCity",
            };

            // run
            var newJobOffer = await _service.CreateJobOfferAsync(jobOffer, claimsPrincipal);

            // validate
            Assert.That(newJobOffer, Is.Not.Null);
            var jobOfferEntry = _context.Entry(newJobOffer);

            Assert.That(newJobOffer.Name, Is.EqualTo(jobOffer.Name));
            // Test eager and lazy loaded fields
            Assert.That(jobOfferEntry.Reference(j => j.Company).IsLoaded, Is.True);
            Assert.That(jobOfferEntry.Collection(j => j.Solicitations).IsLoaded, Is.False);

            Assert.That(newJobOffer.Company, Is.Not.Null);
            Assert.That(newJobOffer.Company.Name, Is.EqualTo("TestCompany"));
        }

        [Test]
        public async Task TestGetCompanyForJobOffer()
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
            var company = await _service.GetCompanyForJobOfferAsync(claimsPrincipal);

            // validate
            Assert.That(company, Is.Not.Null);
            var companyEntry = _context.Entry(company);

            Assert.That(company.Name, Is.EqualTo("TestCompany"));
            // Test eager and lazy loaded fields
            Assert.That(companyEntry.Reference(c => c.User).IsLoaded, Is.True);
            Assert.That(companyEntry.Collection(c => c.JobOffers!).IsLoaded, Is.False);
            Assert.That(companyEntry.Collection(c => c.Appointments!).IsLoaded, Is.False);

            Assert.That(company.User, Is.Not.Null);
            Assert.That(company.User.Name, Is.EqualTo("testCompanyUser"));

            Assert.That(company.JobOffers, Is.Null);
            Assert.That(company.Appointments, Is.Null);
        }

        [Test]
        public async Task TestUpdateJobOffer()
        {
            // prepare
            int id = testJobOffer1Id!.Value;
            var jobOffer = (await _context.JobOffer.FindAsync(id))!;

            jobOffer.Name = "Test Update";
            jobOffer.Description = "Test updating the description.";

            // run
            await _service.UpdateJobOfferAsync(jobOffer);

            // validate
            var updatedJobOffer = await _context.JobOffer.FindAsync(id);

            Assert.That(updatedJobOffer, Is.Not.Null);
            Assert.That(updatedJobOffer.Name, Is.EqualTo("Test Update"));
            Assert.That(updatedJobOffer.Description, Is.EqualTo(jobOffer.Description));
        }

        [Test]
        public async Task TestDoesJobOfferExist()
        {
            // prepare
            int id = testJobOffer1Id!.Value;

            // run
            var result = await _service.DoesJobOfferExistsAsync(id);

            // validate
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task TestDoesJobOfferExistWrongId()
        {
            int id = 123456789;

            // run
            var result = await _service.DoesJobOfferExistsAsync(id);

            // validate
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TestDeleteJobOffer()
        {
            // prepare
            int id = testJobOffer1Id!.Value;

            // run
            await _service.DeleteJobOfferAsync(id);

            var jobOffer = await _context.JobOffer.FindAsync(id);
            var appointments = await _context.Appointment.ToListAsync();
            // validate
            Assert.That(jobOffer, Is.Null);
            Assert.That(appointments, Is.Not.Null);
            Assert.That(appointments.Any(a => a.JobOfferId == id), Is.False);
        }

        [Test]
        public async Task TestChangeJobOfferStatusAsync()
        {
            // prepare
            var id = testJobOffer1Id!.Value;
            var jobOffer = await _context.JobOffer.FindAsync(id);
            var statusFirst = jobOffer.Closed;
            var id2 = testJobOffer2Id!.Value;
            var jobOffer2 = await _context.JobOffer.FindAsync(id2);
            jobOffer2.Closed = DateTime.Now;
            _context.JobOffer.Update(jobOffer2);
            await _context.SaveChangesAsync();


            // run
            await _service.ChangeJobOfferStatusAsync(id);
            await _service.ChangeJobOfferStatusAsync(id2);

            var jobOfferAfter = await _context.JobOffer.FindAsync(jobOffer.Id);
            var jobOffer2After = await _context.JobOffer.FindAsync(jobOffer2.Id);
            // validate
            Assert.That(jobOfferAfter.Closed, Is.Not.EqualTo(statusFirst));
            Assert.That(jobOfferAfter.Closed, Is.Not.EqualTo(null));
            Assert.That(jobOffer2After.Closed, Is.EqualTo(null));
        }

        [Test]
        public async Task TestGetAllJobOffersWQuestionsAsync()
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
            var result1 = await _service.GetAllJobOffersWQuestionsAsync(claimsPrincipalAdmin);
            var result2 = await _service.GetAllJobOffersWQuestionsAsync(claimsPrincipalCompanyUser);
            var result3 = await _service.GetAllJobOffersWQuestionsAsync(claimsPrincipalOtherCompanyUser);

            // validate
            Assert.That(result1, Is.Not.Null);
            Assert.That(result2, Is.Not.Null);
            Assert.That(result3, Is.Not.Null);

            var jobOfferEntry = _context.Entry(result1.First());
            // Test eager and lazy loaded fields
            Assert.That(jobOfferEntry.Reference(j => j.Company).IsLoaded, Is.True);

            Assert.That(result1.Count, Is.EqualTo(2));
            Assert.That(result2.Count, Is.EqualTo(2));
            Assert.That(result3, Is.Empty);
        }

        [Test]
        public async Task TestGetJobOfferWQuestionsAsync()
        {
            // prepare
            var id = testJobOffer1Id!.Value;
            var id2 = testJobOffer2Id!.Value;

            // run
            var jobOffer1 = await _service.GetJobOfferWQuestionsAsync(id);
            var jobOffer2 = await _service.GetJobOfferWQuestionsAsync(id2);

            // validate
            Assert.That(jobOffer1, Is.Not.Null);
            Assert.That(jobOffer2, Is.Not.Null);

            var jobOfferEntry = _context.Entry(jobOffer1!);
            Assert.That(jobOffer1.Name, Is.EqualTo("Test Job Offer"));
            // Test eager and lazy loaded fields
            Assert.That(jobOfferEntry.Collection(j => j.Questions!).IsLoaded, Is.True);

            Assert.That(jobOffer1.Questions, Is.Not.Null);
            Assert.That(jobOffer1.Questions.Count, Is.EqualTo(2));
            Assert.That(jobOffer2.Questions, Is.Not.Null);
            Assert.That(jobOffer2.Questions.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task TestGetQuestionsForCompanyAsync()
        {
            // prepare
            var id = testCompanyId!.Value;
            var id2 = someOtherCompanyId!.Value;

            // run
            var questions1 = await _service.GetQuestionsForCompanyAsync(id);
            var questions2 = await _service.GetQuestionsForCompanyAsync(id2);

            // validate
            Assert.That(questions1, Is.Not.Null);
            Assert.That(questions2, Is.Not.Null);

            var question1Entry = _context.Entry(questions1.First(q => q.Type == "Meerkeuze"));
            // Test eager and lazy loaded fields
            Assert.That(question1Entry.Collection(q => q.Options!).IsLoaded, Is.True);
            Assert.That(question1Entry.Reference(q => q.Company).IsLoaded, Is.True);

            Assert.That(questions1.Count, Is.EqualTo(2));
            Assert.That(questions2, Is.Empty);
        }

        [Test]
        public async Task TestSelectJobOfferQuestionsAsync()
        {
            // prepare
            var id = testJobOffer1Id!.Value;
            var id2 = testJobOffer2Id!.Value;
            var questionId1 = testQuestion1Id!.Value;
            var questionId2 = testQuestion2Id!.Value;
            var array1 = new int[] {questionId1, questionId2};
            var array2 = new int[] { };
            // run
            await _service.SelectJobOfferQuestionsAsync(id, array2);
            await _service.SelectJobOfferQuestionsAsync(id2, array1);

            // validate
            var jobOffer1 = await _context.JobOffer.Include(j => j.Questions).FirstOrDefaultAsync(q => q.Id == id);
            var jobOffer2 = await _context.JobOffer.Include(j => j.Questions).FirstOrDefaultAsync(q => q.Id == id2);

            Assert.That(jobOffer1, Is.Not.Null);
            Assert.That(jobOffer2, Is.Not.Null);
            Assert.That(jobOffer1.Questions, Is.Empty);
            Assert.That(jobOffer2.Questions!.Count, Is.EqualTo(2));
            Assert.That(jobOffer2.Questions.Any(q => q.Id == questionId1), Is.True);
            Assert.That(jobOffer2.Questions.Any(q => q.Id == questionId2), Is.True);
        }
    }
}
