using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework.Internal;
using VAC_T.Business;
using VAC_T.Models;
using VAC_T.UnitTest.TestObjects;
using CsvHelper.Configuration.Attributes;

namespace VAC_T.UnitTest.Services
{
    internal class AnswerServiceTest
    {
        private SqliteConnection _inMemoryDb;
        private TestDbContext _context;
        private AnswerService _service;
        private int? testJobOffer1Id;
        private int? testJobOffer2Id;
        private int? testCompanyId;
        private int? someOtherCompanyId;
        private int? testQuestion1Id;
        private int? testQuestion2Id;
        private int? testQuestion2Option1Id;
        private int? testUserAnswer1Id;

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
                testUserAnswer1Id = context.TestUserAnswer1ForJobOffer2Id;
            }
            _context = new TestDbContext(_inMemoryDb);
            _service = new AnswerService(_context, _context.UserManager);
        }

        [Test]
        public async Task TestGetAnswersAsync()
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
            var answers1 = await _service.GetAnswersAsync(claimsPrincipalAdmin);
            var answers2 = await _service.GetAnswersAsync(claimsPrincipalCompanyUser);
            var answers3 = await _service.GetAnswersAsync(claimsPrincipalOtherCompanyUser);
            var answers4 = await _service.GetAnswersAsync(claimsPrincipalCandidate);

            // validate
            Assert.That(answers1, Is.Not.Null);
            Assert.That(answers2, Is.Not.Null);
            Assert.That(answers3, Is.Not.Null);
            Assert.That(answers4, Is.Not.Null);

            var answers1Entity = _context.Entry(answers1.First());
            // test eager and lazy loaded fiels
            Assert.That(answers1Entity.Reference(a => a.JobOffer).IsLoaded, Is.True);
            Assert.That(answers1Entity.Reference(a => a.Question).IsLoaded, Is.True);

            Assert.That(answers1.Count, Is.EqualTo(1));
            Assert.That(answers2, Is.EqualTo(answers1));
            Assert.That(answers3, Is.Empty);
            Assert.That(answers4, Is.EqualTo(answers1));
        }

        [Test]
        public async Task TestGetAnswerAsync()
        {
            // prepare
            var id = testUserAnswer1Id!.Value;
            var idWrong = 123465789;
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
            var answers1 = await _service.GetAnswerAsync(id, claimsPrincipalAdmin);
            var answers2 = await _service.GetAnswerAsync(id, claimsPrincipalTestUser);
            var answers3 = await _service.GetAnswerAsync(id, claimsPrincipalJustJoinedUser);
            var answers4 = await _service.GetAnswerAsync(idWrong, claimsPrincipalAdmin);

            // validate
            Assert.That(answers1, Is.Not.Null);
            Assert.That(answers2, Is.Not.Null);
            Assert.That(answers3, Is.Null);
            Assert.That(answers4, Is.Null);

            var answers1Entity = _context.Entry(answers1!);
            var answers1QuestionEntity = _context.Entry(answers1.Question);
            // test eager and lazy loaded fields
            Assert.That(answers1Entity.Reference(a => a.JobOffer).IsLoaded, Is.True);
            Assert.That(answers1Entity.Reference(a => a.Question).IsLoaded, Is.True);
            Assert.That(answers1QuestionEntity.Collection(a => a.Options!).IsLoaded, Is.True);
            Assert.That(answers1Entity.Reference(a => a.User).IsLoaded, Is.True);

            Assert.That(answers1.AnswerText, Is.EqualTo("The test question is fine"));
            Assert.That(answers1.JobOffer.Name, Is.EqualTo("Software Tester"));
        }

        [Test]
        public async Task TestGetQuestionAsync()
        {
            // prepare
            var id = testQuestion1Id!.Value;
            var idWrong = 132456789;

            // run
            var question1 = await _service.GetQuestionAsync(id);
            var question2 = await _service.GetQuestionAsync(idWrong);

            // validate
            Assert.That(question1, Is.Not.Null);
            Assert.That(question2, Is.Null);

            var question1Entity = _context.Entry(question1!);
            // test eager and lazy loaded fields
            Assert.That(question1Entity.Collection(q => q.Options!).IsLoaded, Is.True);

            Assert.That(question1.QuestionText, Is.EqualTo("How is the test question?"));
        }

        [Test]
        public async Task TestGetJobOffersAsync()
        {
            // run
            var jobOffers = await _service.GetJobOffersAsync();

            // validate
            Assert.That(jobOffers, Is.Not.Null);
            Assert.That(jobOffers.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task TestGetUserAsync()
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

            // run
            var returnedUser = await _service.GetUserAsync(claimsPrincipalAdmin);

            // validate
            Assert.That(returnedUser, Is.Not.Null);
            Assert.That(returnedUser.Name, Is.EqualTo("testAdmin"));
            Assert.That(returnedUser.Id, Is.EqualTo(user.Id));
        }

        [Test]
        public async Task TestGetAnswersForJobOfferAsync()
        {
            // prepare
            var id = testJobOffer2Id!.Value;
            var idWrong = 123465789;
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

            var user5 = _context.Users.FirstOrDefault(u => u.Name == "TestJustJoinedUser")!;
            var userRoles5 = await _context.UserManager.GetRolesAsync(user5);
            var authClaims5 = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user5.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user5.Id.ToString())
                };
            foreach (var userRole5 in userRoles5)
            {
                authClaims5.Add(new Claim(ClaimTypes.Role, userRole5));
            }
            ClaimsIdentity identity5 = new ClaimsIdentity(authClaims5);
            var claimsPrincipalJustJoinedUser = new ClaimsPrincipal(identity5);
            
            var userId = user4.Id;
            var userIdWrong = "test";

            // run
            var answers1 = await _service.GetAnswersForJobOfferAsync(id, userId, claimsPrincipalAdmin);
            var answers2 = await _service.GetAnswersForJobOfferAsync(idWrong, userId, claimsPrincipalAdmin);
            var answers3 = await _service.GetAnswersForJobOfferAsync(id, userIdWrong, claimsPrincipalAdmin);
            var answers4 = await _service.GetAnswersForJobOfferAsync(id, userId, claimsPrincipalCompanyUser);
            var answers5 = await _service.GetAnswersForJobOfferAsync(id, userId, claimsPrincipalOtherCompanyUser);
            var answers6 = await _service.GetAnswersForJobOfferAsync(id, userId, claimsPrincipalCandidate);
            var answers7 = await _service.GetAnswersForJobOfferAsync(id, userId, claimsPrincipalJustJoinedUser);

            // validate
            Assert.That(answers1, Is.Not.Null);
            Assert.That(answers2, Is.Not.Null);
            Assert.That(answers3, Is.Not.Null);
            Assert.That(answers4, Is.Not.Null);
            Assert.That(answers5, Is.Not.Null);
            Assert.That(answers6, Is.Not.Null);
            Assert.That(answers7, Is.Null);

            var answers1Entity = _context.Entry(answers1.First()!);
            var answers1QuestionEntity = _context.Entry(answers1.First().Question);
            // test eager and lazy loaded fields
            Assert.That(answers1Entity.Reference(a => a.JobOffer).IsLoaded, Is.True);
            Assert.That(answers1Entity.Reference(a => a.Question).IsLoaded, Is.True);
            Assert.That(answers1QuestionEntity.Collection(a => a.Options!).IsLoaded, Is.True);
            Assert.That(answers1Entity.Reference(a => a.User).IsLoaded, Is.True);

            Assert.That(answers1.Count, Is.EqualTo(1));
            Assert.That(answers2, Is.Empty);
            Assert.That(answers3, Is.Empty);
            Assert.That(answers4, Is.EqualTo(answers1));
            Assert.That(answers5, Is.Empty);
            Assert.That(answers6, Is.EqualTo(answers1));

            Assert.That(answers1.First().AnswerText, Is.EqualTo("The test question is fine"));
        }

        [Test]
        public async Task TestDoAnswersExistAsync()
        {
            // prepare
            var id = testJobOffer2Id!.Value;
            var idWrong = 123465789;
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
            var result1 = await _service.DoAnswersExistAsync(id, claimsPrincipalAdmin);
            var result2 = await _service.DoAnswersExistAsync(id, claimsPrincipalTestUser);
            var result3 = await _service.DoAnswersExistAsync(id, claimsPrincipalJustJoinedUser);
            var result4 = await _service.DoAnswersExistAsync(idWrong, claimsPrincipalTestUser);

            var result5 = await _service.DoAnswersExistAsync(id, user2.Id, claimsPrincipalAdmin);
            var result6 = await _service.DoAnswersExistAsync(id, user2.Id, claimsPrincipalTestUser);
            var result7 = await _service.DoAnswersExistAsync(id, user2.Id, claimsPrincipalJustJoinedUser);
            var result8 = await _service.DoAnswersExistAsync(idWrong, user2.Id, claimsPrincipalTestUser);
            var result9 = await _service.DoAnswersExistAsync(id, user3.Id, claimsPrincipalAdmin);

            // validate
            Assert.That(result1, Is.False);
            Assert.That(result2, Is.True);
            Assert.That(result3, Is.False);
            Assert.That(result4, Is.False);
            Assert.That(result5, Is.True);
            Assert.That(result6, Is.True);
            Assert.That(result7, Is.False);
            Assert.That(result8, Is.False);
            Assert.That(result9, Is.False);
        }

        [Test]
        public async Task TestPrepareUserAnswersForCreateAsync()
        {
            // prepare
            var id = testJobOffer2Id!.Value;
            var idNoQuestion = testJobOffer1Id!.Value;
            var prepNoQuestion = await _context.JobOffer.Include(j => j.Questions).FirstOrDefaultAsync(j => j.Id == idNoQuestion);
            prepNoQuestion!.Questions = null;
            _context.JobOffer.Update(prepNoQuestion);
            await _context.SaveChangesAsync();

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

            // run
            var answers1 = await _service.PrepareUserAnswersForCreateAsync(id, claimsPrincipalAdmin);
            var answers2 = await _service.PrepareUserAnswersForCreateAsync(idNoQuestion, claimsPrincipalAdmin);
            var answers3 = await _service.PrepareUserAnswersForCreateAsync(123465789, claimsPrincipalAdmin);

            // validate
            Assert.That(answers1, Is.Not.Null);
            Assert.That(answers2, Is.Null);
            Assert.That(answers3, Is.Null);

            Assert.That(answers1.Count, Is.EqualTo(1));
            Assert.That(answers1.First().UserId, Is.EqualTo(user.Id));
            Assert.That(answers1.First().JobOffer.Id, Is.EqualTo(id));
        }

        [Test]
        public async Task TestCreateAnsersAsync()
        {
            // prepare
            var questionId = testQuestion1Id!.Value;
            var jobOfferId = testJobOffer2Id!.Value;
            var user = _context.Users.FirstOrDefault(u => u.Name == "testAdmin")!;
            var answers = new List<Answer>()
            {
                new Answer()
                {
                    QuestionId = questionId,
                    JobOfferId = jobOfferId,
                    UserId = user.Id,
                    AnswerText = "test",
                    Explanation = null
                }
            };

            // run
            var created = await _service.CreateAnswersAsync(answers);

            // validate
            Assert.That(created, Is.Not.Null);
            Assert.That(created.First().AnswerText, Is.EqualTo("test"));
        }

        [Test]
        public async Task TestUpdateAnswerAsync()
        {
            // prepare
            var id = testUserAnswer1Id!.Value;
            var answer = await _context.Answer.FindAsync(id);
            answer.AnswerText = "Update";

            // run
            await _service.UpdateAnswerAsync(answer);

            // validate
            var updatedAnswer = await _context.Answer.FindAsync(id);
            Assert.That(updatedAnswer, Is.Not.Null);
            Assert.That(updatedAnswer.AnswerText, Is.EqualTo("Update"));
        }

        [Test]
        public async Task TestDeleteUserAnswersForJobOfferAsync()
        {
            // prepare
            var id = testJobOffer2Id!.Value;
            var user = _context.Users.FirstOrDefault(u => u.Name == "testUser")!;

            // run
            await _service.DeleteUserAnswersForJobOfferAsync(id, user.Id);

            // validate
            var any = await _context.Answer.AnyAsync();

            Assert.That(any, Is.False);
        }

        [Test]
        public async Task TestCheckUserSolicitatedAsync()
        {
            // prepare
            var id = testJobOffer2Id!.Value;
            var idWrong = 132456789;
            var user = _context.Users.FirstOrDefault(u => u.Name == "testUser")!;
            var user2 = _context.Users.FirstOrDefault(u => u.Name == "TestJustJoinedUser")!;

            // run
            var result1 = await _service.CheckUserSolicitatedAsync(id, user.Id);
            var result2 = await _service.CheckUserSolicitatedAsync(idWrong, user.Id);
            var result3 = await _service.CheckUserSolicitatedAsync(id, user2.Id);
            var result4 = await _service.CheckUserSolicitatedAsync(idWrong, user2.Id);

            // validate
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.False);
            Assert.That(result3, Is.False);
            Assert.That(result4, Is.False);
        }

        [Test]
        public void TestPrepareAnswerForDisplay()
        {
            // prepare
            var options1 = new QuestionOption()
            {
                Id = 1,
                OptionShort = "True",
                OptionLong = "100% True"
            };
            var options2 = new QuestionOption()
            {
                Id = 2,
                OptionShort = "False",
                OptionLong = "100% False"
            };
            var answerString1 = "Test";
            var question1 = new Question()
            {
                Type = "Open"
            };
            var answerString2 = "56";
            var question2 = new Question()
            {
                Type = "Standpunt",
                Options = new List<QuestionOption>() { options1, options2 },
            };
            var answerString3 = "Ja";
            var question3 = new Question()
            {
                Type = "Ja/Nee"
            };
            var answerString4 = "2";
            var question4 = new Question()
            {
                Type = "Meerkeuze",
                MultipleOptions = false,
                Options = new List<QuestionOption>() { options1, options2 },
            };
            var answerString5 = "Anders";
            var answerString6 = "1_2_Anders";
            var question6 = new Question()
            {
                Type = "Meerkeuze",
                MultipleOptions = true,
                Options = new List<QuestionOption>() { options1, options2 },
            };

            // run
            var answer1 = _service.PrepareAnswerForDisplay(answerString1, question1);
            var answer2 = _service.PrepareAnswerForDisplay(answerString2, question2);
            var answer3 = _service.PrepareAnswerForDisplay(answerString3, question3);
            var answer4 = _service.PrepareAnswerForDisplay(answerString4, question4);
            var answer5 = _service.PrepareAnswerForDisplay(answerString5, question4);
            var answer6 = _service.PrepareAnswerForDisplay(answerString6, question6);

            // validate
            Assert.That(answer1, Is.Not.Null);
            Assert.That(answer2, Is.Not.Null);
            Assert.That(answer3, Is.Not.Null);
            Assert.That(answer4, Is.Not.Null);
            Assert.That(answer5, Is.Not.Null);
            Assert.That(answer6, Is.Not.Null);
            Assert.That(answer1, Is.EqualTo(answerString1));
            Assert.That(answer2, Is.EqualTo("0 100% True - 100% False 100: 56"));
            Assert.That(answer3, Is.EqualTo(answerString3));
            Assert.That(answer4, Is.EqualTo("100% False"));
            Assert.That(answer5, Is.EqualTo("Anders"));
            Assert.That(answer6, Is.EqualTo("100% True, 100% False, Anders"));
        }

        [Test]
        public void TestPrepareAnswerForCSV()
        {
            // prepare
            var options1 = new QuestionOption()
            {
                Id = 1,
                OptionShort = "True",
                OptionLong = "100% True"
            };
            var options2 = new QuestionOption()
            {
                Id = 2,
                OptionShort = null,
                OptionLong = "100% False"
            };
            var answerString1 = "Test";
            var question1 = new Question()
            {
                Type = "Open"
            };
            var answerString2 = "56";
            var question2 = new Question()
            {
                Type = "Standpunt",
                Options = new List<QuestionOption>() { options1, options2 },
            };
            var answerString3 = "Ja";
            var question3 = new Question()
            {
                Type = "Ja/Nee"
            };
            var answerString4 = "2";
            var question4 = new Question()
            {
                Type = "Meerkeuze",
                MultipleOptions = false,
                Options = new List<QuestionOption>() { options1, options2 },
            };
            var answerString5 = "Anders";
            var answerString6 = "1_2_Anders";
            var question6 = new Question()
            {
                Type = "Meerkeuze",
                MultipleOptions = true,
                Options = new List<QuestionOption>() { options1, options2 },
            };

            // run
            var answer1 = _service.PrepareAnswerForCSV(answerString1, question1);
            var answer2 = _service.PrepareAnswerForCSV(answerString2, question2);
            var answer3 = _service.PrepareAnswerForCSV(answerString3, question3);
            var answer4 = _service.PrepareAnswerForCSV(answerString4, question4);
            var answer5 = _service.PrepareAnswerForCSV(answerString5, question4);
            var answer6 = _service.PrepareAnswerForCSV(answerString6, question6);

            // validate
            Assert.That(answer1, Is.Not.Null);
            Assert.That(answer2, Is.Not.Null);
            Assert.That(answer3, Is.Not.Null);
            Assert.That(answer4, Is.Not.Null);
            Assert.That(answer5, Is.Not.Null);
            Assert.That(answer6, Is.Not.Null);
            Assert.That(answer1, Is.EqualTo(answerString1));
            Assert.That(answer2, Is.EqualTo("0 True - 100% False 100: 56"));
            Assert.That(answer3, Is.EqualTo(answerString3));
            Assert.That(answer4, Is.EqualTo("100% False"));
            Assert.That(answer5, Is.EqualTo("Anders"));
            Assert.That(answer6, Is.EqualTo("True,100% False,Anders"));
        }

        [Test]
        public async Task TestCreateCSVFileAsync()
        {
            // prepare
            var answers = await _context.Answer.Include(a => a.JobOffer).Include(a => a.Question.Options).Include(a => a.User).ToListAsync();
            var testClass = new CSVLine() { Answer = "a", Question = "b"};
            var type = testClass.GetType();
            var question = (NameAttribute)Attribute.GetCustomAttribute(type.GetProperty("Question")!, typeof(NameAttribute))!;
            var answer = (NameAttribute)Attribute.GetCustomAttribute(type.GetProperty("Answer")!, typeof(NameAttribute))!;
            var explanation = (NameAttribute)Attribute.GetCustomAttribute(type.GetProperty("Explanation")!, typeof(NameAttribute))!;
            var compare = new string[] 
            {
                answers.First().JobOffer.Name + ";" + DateTime.Today.ToShortDateString() + ";" + answers.First().User.Name,
                question.Names[0] + ";" + answer.Names[0] + ";" + explanation.Names[0],
                answers.First().Question.QuestionText + ";" + _service.PrepareAnswerForCSV(answers.First().AnswerText, answers.First().Question) + ";" + answers.First().Explanation,
                ""
            };
            
            // run
            var ms = await _service.CreateCSVFileAsync(answers);

            // validate
            var array = ms.ToArray();
            var str = System.Text.Encoding.Default.GetString(array).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.That(str, Is.Not.Null);
            Assert.That(str, Is.EqualTo(compare));
            ms.Close();
        }
    }
}
