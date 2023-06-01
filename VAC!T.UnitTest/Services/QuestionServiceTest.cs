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

namespace VAC_T.UnitTest.Services
{
    internal class QuestionServiceTest
    {
        private SqliteConnection _inMemoryDb;
        private TestDbContext _context;
        private QuestionService _service;
        private int? testJobOffer1Id;
        private int? testJobOffer2Id;
        private int? testCompanyId;
        private int? someOtherCompanyId;
        private int? testQuestion1Id;
        private int? testQuestion2Id;
        private int? testQuestion2Option1Id;

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
                testQuestion2Option1Id = context.TestCompanyQuestion2QuestionOption1Id;
            }
            _context = new TestDbContext(_inMemoryDb);
            _service = new QuestionService(_context, _context.UserManager);
        }

        [Test]
        public async Task TestGetQuestionsAsync()
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
            var questions1 = await _service.GetQuestionsAsync(claimsPrincipalAdmin);
            var questions2 = await _service.GetQuestionsAsync(claimsPrincipalCompanyUser);
            var questions3 = await _service.GetQuestionsAsync(claimsPrincipalOtherCompanyUser);

            // validate
            Assert.That(questions1, Is.Not.Null);
            Assert.That(questions2, Is.Not.Null);
            Assert.That(questions3, Is.Not.Null);

            var questions1Entry = _context.Entry(questions1.First());
            // Test eager and lazy loaded fields
            Assert.That(questions1Entry.Reference(q => q.Company).IsLoaded, Is.True);
            Assert.That(questions1Entry.Collection(q => q.Options).IsLoaded, Is.True);

            Assert.That(questions1.Count, Is.EqualTo(2));
            Assert.That(questions2.Count, Is.EqualTo(2));
            Assert.That(questions3, Is.Empty);
        }

        [Test]
        public async Task TestGetQuestionAsync()
        {
            // prepare
            var id = testQuestion1Id!.Value;
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
            var question1 = await _service.GetQuestionAsync(id, claimsPrincipalAdmin);
            var question2 = await _service.GetQuestionAsync(id, claimsPrincipalCompanyUser);
            var question3 = await _service.GetQuestionAsync(id, claimsPrincipalOtherCompanyUser);
            var question4 = await _service.GetQuestionAsync(idWrong, claimsPrincipalAdmin);

            // validate
            Assert.That(question1, Is.Not.Null);
            Assert.That(question2, Is.Not.Null);
            Assert.That(question3, Is.Null);
            Assert.That(question4, Is.Null);

            var questionEntry = _context.Entry(question1!);
            // Test eager and lazy loaded fields
            Assert.That(questionEntry.Reference(q => q.Company).IsLoaded, Is.True);
            Assert.That(questionEntry.Collection(q => q.Options!).IsLoaded, Is.True);

            Assert.That(question1.Equals(question2), Is.True);
            Assert.That(question1.Type.Equals("Open"), Is.True);
        }

        [Test]
        public async Task TestGetQuestionOptionAsync()
        {
            // prepare
            var id = testQuestion2Option1Id!.Value;
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
            var option = await _service.GetQuestionOptionAsync(id, claimsPrincipalAdmin);
            var option2 = await _service.GetQuestionOptionAsync(id, claimsPrincipalCompanyUser);

            // valdiate
            Assert.That(option, Is.Not.Null);
            Assert.That(option2, Is.Not.Null);
            Assert.That(option == option2, Is.True);

            Assert.That(option.OptionShort.Equals("yes"), Is.True);

            var optionEntity = _context.Entry(option);
            // Test eager and lazy loaded fields
            Assert.That(optionEntity.Reference(o => o.Question).IsLoaded, Is.True);
        }

        [Test]
        public async Task TestGetCompanyAsync()
        {
            // prepare
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
            var company = await _service.GetCompanyAsync(claimsPrincipalCompanyUser);

            // validate
            Assert.That(company, Is.Not.Null);
            Assert.That(company.Name, Is.EqualTo("TestCompany"));
        }

        [Test]
        public async Task TestGetCompaniesAsync()
        {
            // run
            var companies = await _service.GetCompaniesAsync();

            // validate
            Assert.That(companies, Is.Not.Null);
            Assert.That(companies.Count, Is.EqualTo(2));
            Assert.That(companies.Select(c => c.Name).Contains("TestCompany"), Is.True);
        }

        [Test]
        public async Task TestCreateQuestionAsync()
        {
            // prepare
            var companyId = testCompanyId!.Value;
            var question = new Question()
            {
                CompanyId = companyId,
                Type = "Open",
                QuestionText = "Test",
                MultipleOptions = true,
                Options = null,
                ExplanationType = "abc",
                OptionsAmount = 2
            };
            var question2 = new Question()
            {
                CompanyId = companyId,
                Type = "Ja/Nee",
                QuestionText = "Test Ja/Nee",
                MultipleOptions = false,
                Options = null,
                ExplanationType = "",
                OptionsAmount = 2
            };

            // run
            var createdQuestion1 = await _service.CreateQuestionAsync(question);
            var createdQuestion2 = await _service.CreateQuestionAsync(question2);

            // validate
            Assert.That(createdQuestion1, Is.Not.Null);
            Assert.That(createdQuestion2, Is.Not.Null);
            Assert.That(createdQuestion1.MultipleOptions, Is.False);
            Assert.That(createdQuestion1.ExplanationType, Is.EqualTo(""));
            Assert.That(createdQuestion2.Options!.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task TestCreateQuestionOptionsAsync()
        {
            // prepare
            var id = testQuestion2Id!.Value;
            var options = new List<QuestionOption>()
            {
                new QuestionOption()
                {
                    QuestionId = id,
                    OptionShort = "test",
                    OptionLong = "This is a test"
                },
                new QuestionOption()
                {
                    QuestionId = id,
                    OptionShort = null,
                    OptionLong = "This is also a test"
                }
            };

            // run
            var questionOptions = await _service.CreateQuestionOptionsAsync(options);

            // validate
            Assert.That(questionOptions, Is.Not.Null);
            Assert.That(questionOptions.Count, Is.EqualTo(2));
            Assert.That(questionOptions.First().QuestionId, Is.EqualTo(id));
            Assert.That(questionOptions.Any(o => o.OptionShort == null), Is.True);
            Assert.That(questionOptions.Any(o => o.OptionLong == "This is also a test"), Is.True);
        }

        [Test]
        public async Task TestDoesQuestionExistAsync()
        {
            // prepare
            var id = testQuestion1Id!.Value;
            var idWrong = 123456789;
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
            var result1 = await _service.DoesQuestionExistAsync(id, claimsPrincipalAdmin);
            var result2 = await _service.DoesQuestionExistAsync(id, claimsPrincipalCompanyUser);
            var result3 = await _service.DoesQuestionExistAsync(id, claimsPrincipalOtherCompanyUser);
            var result4 = await _service.DoesQuestionExistAsync(idWrong, claimsPrincipalAdmin);

            // validate
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.True);
            Assert.That(result3, Is.False);
            Assert.That(result4, Is.False);
        }

        [Test]
        public async Task TestDoesQuestionOptionExistAsync()
        {
            // prepare
            var id = testQuestion2Option1Id!.Value;
            var idWrong = 123456789;
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
            var result1 = await _service.DoesQuestionOptionExistAsync(id, claimsPrincipalAdmin);
            var result2 = await _service.DoesQuestionOptionExistAsync(id, claimsPrincipalCompanyUser);
            var result3 = await _service.DoesQuestionOptionExistAsync(id, claimsPrincipalOtherCompanyUser);
            var result4 = await _service.DoesQuestionOptionExistAsync(idWrong, claimsPrincipalAdmin);

            // validate
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.True);
            Assert.That(result3, Is.False);
            Assert.That(result4, Is.False);
        }

        [Test]
        public async Task TestUpdateQuestionAsync()
        {
            // prepare
            var id = testQuestion1Id!.Value;
            var question = await _context.Question.FindAsync(id);
            question.Type = "Ja/Nee";
            question.QuestionText = "Update";

            // run
            await _service.UpdateQuestionAsync(question);

            // validate
            var updatedQuestion = await _context.Question.Include(q => q.Options).FirstOrDefaultAsync(q => q.Id == question.Id);

            Assert.That(updatedQuestion, Is.Not.Null);
            Assert.That(updatedQuestion.Type, Is.EqualTo("Ja/Nee"));
            Assert.That(updatedQuestion.Options!.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task TestUpdateQuestionOptionAsync()
        {
            // prepare
            var id = testQuestion2Option1Id!.Value;
            var option = await _context.QuestionOption.FindAsync(id);
            option.OptionShort = "Test Update";

            // run
            await _service.UpdateQuestionOptionAsync(option);

            // validate
            var updatedQuestion = await _context.QuestionOption.FindAsync(option.Id);

            Assert.That(updatedQuestion, Is.Not.Null);
            Assert.That(updatedQuestion.OptionShort, Is.EqualTo("Test Update"));
        }

        [Test]
        public async Task TestDeleteQuestionAsync()
        {
            // prepare
            var id = testQuestion2Id!.Value;

            // run
            await _service.DeleteQuestionAsync(id);

            // validate
            var count = await _context.Question.CountAsync();
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task TestDeleteQuestionOptionAsync()
        {
            // prepare
            var id = testQuestion2Option1Id!.Value;

            // run
            await _service.DeleteQuestionOptionAsync(id);

            // validate
            var count = await _context.QuestionOption.CountAsync();

            Assert.That(count, Is.EqualTo(1));
        }
    }
}
