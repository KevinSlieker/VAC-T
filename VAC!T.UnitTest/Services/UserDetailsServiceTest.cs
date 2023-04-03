using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VAC_T.Business;
using VAC_T.UnitTest.TestObjects;

namespace VAC_T.UnitTest.Services
{
    internal class UserDetailsServiceTest
    {
        private SqliteConnection _inMemoryDb;
        private TestDbContext _context;
        private UserDetailsService _service;
        private string? testUserId;

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
                testUserId = context.TestUserId;

            }
            _context = new TestDbContext(_inMemoryDb);
            _service = new UserDetailsService(_context, _context.UserManager);
        }

        [Test]
        public async Task TestGetUsers()
        {
            // prepare

            // run
            var users = await _service.GetUsersAsync(null, null);
            var usersSearchName = await _service.GetUsersAsync("JustJoined", null);
            var usersSearchEmail = await _service.GetUsersAsync(null, "testCompany");
            var usersNoResult = await _service.GetUsersAsync(null, "ABCDEF");

            // validate
            Assert.That(users, Is.Not.Null);
            Assert.That(usersSearchName, Is.Not.Null);
            Assert.That(usersSearchEmail, Is.Not.Null);
            Assert.That(usersNoResult, Is.Empty);

            Assert.That(users.Count, Is.EqualTo(5));
            Assert.That(usersSearchName.Count, Is.EqualTo(1));
            Assert.That(usersSearchEmail.Count, Is.EqualTo(1));

            Assert.That(usersSearchEmail.First().Name, Is.EqualTo("testCompanyUser"));
        }

        [Test]
        public async Task TestGetUserDetails()
        {
            // prepare
            string id = testUserId!;
            string idWrong = "Wrong";

            // run
            var userDetails = await _service.GetUserDetailsAsync(id);
            var userDetailsWrong = await _service.GetUserDetailsAsync(idWrong);

            // validate
            Assert.That(userDetails, Is.Not.Null);
            Assert.That(userDetailsWrong, Is.Null);

            Assert.That(userDetails.Name, Is.EqualTo("testUser"));
        }

        [Test]
        public async Task TestDeleteUser()
        {
            // prepare
            string id = testUserId!;
            string idWrong = "Wrong";

            // run
            await _service.DeleteUserAsync(id);
            await _service.DeleteUserAsync(idWrong);

            // validate
            var users = await _context.Users.ToListAsync();

            Assert.That(users, Is.Not.Null);
            Assert.That(users.Count, Is.EqualTo(4));
        }

        [Test]
        public async Task TestGetUserRoles()
        {
            // prepare
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == "testUser");

            // run
            var roles = await _service.GetUserRolesAsync(user!);

            // validate
            Assert.That(roles, Is.Not.Null);
            Assert.That(roles, Is.EqualTo("ROLE_CANDIDATE"));
        }
    }
}
