using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VAC_T.Data;
using VAC_T.Models;

namespace VAC_T.UnitTest.TestObjects
{
    /// <summary>
    /// Application context that uses an in memory database & functions to generate test users/companies etc
    /// </summary>
    internal class TestDbContext : ApplicationDbContext
    {
        public UserManager<VAC_TUser> UserManager { get; }

        // Test entities
        private VAC_TUser? testAdmin;
        public string? TestAdminId { get { return testAdmin?.Id; } }

        private VAC_TUser? testCompanyUser;
        public string? TestCompanyUserId { get { return testCompanyUser?.Id; } }

        private VAC_TUser? testUser;
        public string? TestUserId { get { return testUser?.Id; } }

        private VAC_TUser? testJustJoinedUser;
        public string? TestJustJoinedUserId { get { return testJustJoinedUser?.Id; } }

        private VAC_TUser? someOtherCompanyUser;
        public string? TestSomeOtherCompanyUserId { get { return someOtherCompanyUser?.Id; } }

        private Company? testCompany;
        public int? TestCompanyId { get { return testCompany?.Id; } }

        private Company? someOtherCompany;
        public int? SomeOtherCompanyId { get { return someOtherCompany?.Id; } }

        private JobOffer? testCompanyJobOffer1;
        public int? TestCompanyJobOffer1Id { get { return testCompanyJobOffer1?.Id; } }

        private JobOffer? testCompanyJobOffer2;
        public int? TestCompanyJobOffer2Id { get { return testCompanyJobOffer2?.Id; } }

        private Appointment? testCompanyAppointmentForAnyJobOffer;
        public int? TestCompanyAppointmentForAnyJobOfferId { get { return testCompanyAppointmentForAnyJobOffer?.Id; } }

        private Appointment? testCompanyAppointmentForJobOffer1;
        public int? TestCompanyAppointmentForJobOffer1Id { get { return testCompanyAppointmentForJobOffer1?.Id; } }

        private Appointment? testCompanyAppointmentInPast;

        private Solicitation? testUserSolicitationTestCompanyWithAppointmentForJobOffer1;

        private Solicitation? testUserSolicitationTestCompanyForJobOffer2;

        public TestDbContext() :
            base(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Vac!t")
                .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole())) // Log to the test console
                //                .EnableSensitiveDataLogging(true)
                .Options)
        {
            UserManager = CreateUserManager();
        }

        public async Task SetupDatabase()
        {
            await Database.EnsureCreatedAsync();
            await Roles.AddRangeAsync(
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
                    },
                    new IdentityRole
                    {
                        Name = "ROLE_ADMIN",
                        NormalizedName = "ROLE_ADMIN",
                        ConcurrencyStamp = "3"
                    }
            );
            await SaveChangesAsync();
        }

        public async Task AddTestUsersAsync()
        {
            testUser = new VAC_TUser()
            {
                Name = "testUser",
                UserName = "testUser",
                BirthDate = new DateTime(1980, 2, 28),
                Address = "TestLane 44",
                Postcode = "9988ZZ",
                Motivation = "I'm very motivated",
                ProfilePicture = "assets/testUser/1.jpg",
                CV = "assets/testUser/1.pdf",
                Email = "testuser@test.nl",
                EmailConfirmed = true,
            };
            await AddUserWithPasswordAndRoleAsync(testUser, "p@ssWord1", "ROLE_CANDIDATE");

            testCompanyUser = new VAC_TUser()
            {
                Name = "testCompanyUser",
                UserName = "testCompanyUser",
                Address = "TestStreet 3312",
                Postcode = "1234ER",
                Email = "info@testCompany.com",
                EmailConfirmed = true,
            };
            await AddUserWithPasswordAndRoleAsync(testCompanyUser, "$3cretCode", "ROLE_EMPLOYER");

            testAdmin = new VAC_TUser()
            {
                Name = "testAdmin",
                UserName = "testAdmin",
                Address = "TestSquare 3",
                Postcode = "9742EE",
                Email = "admin@test.nl",
                EmailConfirmed = true,
            };
            await AddUserWithPasswordAndRoleAsync(testAdmin, "t0p$ecretCod3", "ROLE_ADMIN");

            testJustJoinedUser = new VAC_TUser()
            {
                Name = "TestJustJoinedUser",
                UserName = "TestJustJoinedUser",
                ProfilePicture = "assets/img/testUser/profile.png",
                Email = "testuser@test.nl",
                EmailConfirmed = true,
            };
            await AddUserWithPasswordAndRoleAsync(testJustJoinedUser, "pas$C0de", "ROLE_CANDIDATE");

            someOtherCompanyUser = new VAC_TUser()
            {
                Name = "someOtherCompanyUser",
                UserName = "someOtherCompanyUser",
                Address = "TestAvenue 33",
                Postcode = "1233AA",
                Email = "info@someOthercompany.nl",
                EmailConfirmed = true,
            };
            await AddUserWithPasswordAndRoleAsync(someOtherCompanyUser, "s0meComp@ny", "ROLE_EMPLOYER");

        }

        public async Task AddTestCompaniesAsync()
        {
            testCompany = new Company {
                Name = "TestCompany",
                Description = "A Test Company",
                LogoURL = "assets/img/company/testcompany.jpg",
                WebsiteURL = "http://testCompany.com",
                Address = "Somewherestreet 11",
                Postcode = "1122AB",
                Residence = "TestCity",
                User = testCompanyUser
            };
            await Company.AddAsync(testCompany);

            someOtherCompany = new Company
            {
                Name = "Some Other Company",
                Description = "We make software",
                LogoURL = "assets/img/company/testcompany.jpg",
                WebsiteURL = "http://someOtherCompany.com",
                Address = "Industrial Zone 2",
                Postcode = "2345RR",
                Residence = "TestCity",
                User = someOtherCompanyUser
            };
            await Company.AddAsync(someOtherCompany);

            testCompanyJobOffer1 = new JobOffer
            {
                Name = "Test Job Offer",
                Description = "To do some testing",
                Company = testCompany,
                Created = DateTime.Now.AddDays(-10),
                Level = "Beginner",
                Residence = "TestCity",
            };
            await JobOffer.AddAsync(testCompanyJobOffer1);

            testCompanyJobOffer2 = new JobOffer
            {
                Name = "Software Tester",
                Description = "Make a lot of unit tests",
                Company = testCompany,
                Created = DateTime.Now.AddDays(-1),
                Level = "Medior",
                Residence = "TestCity",
            };
            await JobOffer.AddAsync(testCompanyJobOffer2);


            testCompanyAppointmentForAnyJobOffer = new Appointment
            {
                Company = testCompany,
                Date = DateTime.Today.AddDays(2),
                Time = DateTime.UnixEpoch.AddHours(14),
                Duration = TimeSpan.FromMinutes(45),
                IsOnline = false,
                JobOffer = null
            };
            await Appointment.AddAsync(testCompanyAppointmentForAnyJobOffer);

            testCompanyAppointmentForJobOffer1 = new Appointment
            {
                Company = testCompany,
                Date = DateTime.Today.AddDays(2),
                Time = DateTime.UnixEpoch.AddHours(15).AddMinutes(15),
                Duration = TimeSpan.FromMinutes(45),
                IsOnline = false,
                JobOffer = testCompanyJobOffer1,
            };
            await Appointment.AddAsync(testCompanyAppointmentForJobOffer1);

            testCompanyAppointmentInPast = new Appointment
            {
                Company = testCompany,                
                Date = DateTime.Today.AddDays(-1),
                Time = DateTime.UnixEpoch.AddHours(14),
                Duration = TimeSpan.FromMinutes(45),
                IsOnline = false,
                JobOffer = testCompanyJobOffer2,
            };
            await Appointment.AddAsync(testCompanyAppointmentInPast);

            await SaveChangesAsync();
        }
        public async Task AddTestSolictations()
        {

            testUserSolicitationTestCompanyWithAppointmentForJobOffer1 = new Solicitation
            {
                User = testUser!,
                JobOffer = testCompanyJobOffer1!,
                Date = DateTime.Now.AddDays(-8),
                Selected = true,
                Appointment = testCompanyAppointmentForJobOffer1
            };
            await Solicitation.AddAsync(testUserSolicitationTestCompanyWithAppointmentForJobOffer1);

            testUserSolicitationTestCompanyForJobOffer2 = new Solicitation
            {
                User = testUser!,
                JobOffer = testCompanyJobOffer2!,
                Date = DateTime.Now.AddDays(-1),
                Selected = false,
                Appointment = null
            };
            await Solicitation.AddAsync(testUserSolicitationTestCompanyForJobOffer2);

            await SaveChangesAsync();
        }






        #region Service functions 
        /// <summary>
        /// Create a test User manager
        /// </summary>
        /// <returns>The User Manager</returns>
        private UserManager<VAC_TUser> CreateUserManager()
        {
            var store = new UserStore<VAC_TUser>(this);

            var idOptions = new IdentityOptions();

            //this should be keep in sync with settings in ConfigureIdentity in WebApi -> Startup.cs
            idOptions.Lockout.AllowedForNewUsers = false;
            idOptions.Password.RequireDigit = true;
            idOptions.Password.RequireLowercase = true;
            idOptions.Password.RequireNonAlphanumeric = true;
            idOptions.Password.RequireUppercase = true;
            idOptions.Password.RequiredLength = 8;
            idOptions.Password.RequiredUniqueChars = 1;

            idOptions.SignIn.RequireConfirmedEmail = false;

            // Lockout settings.
            idOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            idOptions.Lockout.MaxFailedAccessAttempts = 5;
            idOptions.Lockout.AllowedForNewUsers = true;

            var options = Options.Create(idOptions);

            var userValidators = new List<IUserValidator<VAC_TUser>>();
            UserValidator<VAC_TUser> validator = new UserValidator<VAC_TUser>();
            userValidators.Add(validator);

            var passValidator = new PasswordValidator<VAC_TUser>();
            var pwdValidators = new List<IPasswordValidator<VAC_TUser>>
            {
                passValidator
            };
            var spf = new DefaultServiceProviderFactory();
            var sp = spf.CreateServiceProvider(new ServiceCollection());

            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<UserManager<VAC_TUser>>();

            return new UserManager<VAC_TUser>(store, options, new PasswordHasher<VAC_TUser>(),
                userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(), sp, logger);
        }

        /// <summary>
        /// Create a testUser with a specific password
        /// </summary>
        /// <param name="user">the testUser</param>
        /// <param name="password">the password</param>
        /// <param name="roleName">either "ROLE_CANDIDATE", "ROLE_EMPLOYER" or "ROLE_ADMIN"</param>
        public async Task AddUserWithPasswordAndRoleAsync(VAC_TUser user, string password, string roleName)
        {
            var result = await UserManager.CreateAsync(user, password);

            Assert.That(result.Errors.Select(err => err.Description), Is.Empty, "Errors creating {0}", user.Name);

            var assignResult = await UserManager.AddToRoleAsync(user, roleName);

            Assert.That(result.Errors.Select(err => err.Description), Is.Empty, "Errors assigning role to {0}", user.Name);

            await SaveChangesAsync();
        }
        #endregion
    }
}
