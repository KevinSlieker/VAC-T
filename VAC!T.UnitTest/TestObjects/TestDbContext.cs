using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
        public UserManager<VAC_TUser> UserManager { get;  }

        public TestDbContext() : 
            base(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Vac!t").Options) {
            UserManager = CreateUserManager();
        }

        public async Task SetupDatabase()
        {
            await Database.EnsureCreatedAsync();
            Roles.AddRange(
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

        public async Task AddTestUsersAsync() {
            await AddUserWithPasswordAndRoleAsync(new VAC_TUser()
                {
                    Name = "TestUser",
                    UserName = "TestUser",
                    BirthDate = new DateTime(1980, 2, 28),
                    Address = "TestLane 44",
                    Postcode = "9988ZZ",
                    Motivation = "I'm very motivated",
                    ProfilePicture = "assets/user/1.jpg",
                    CV = "assets/user/1.pdf",
                    Email = "testuser@test.nl",
                    EmailConfirmed = true,
                }, 
                "p@ssWord1", "ROLE_CANDIDATE");

            await AddUserWithPasswordAndRoleAsync(new VAC_TUser()
                {
                    Name = "TestCompanyUser",
                    UserName = "TestCompanyUser",
                    Address = "TestStreet 3312",
                    Postcode = "1234ER",
                    Email = "info@company.nl",
                    EmailConfirmed = true,
                }, 
                "$3cretCode", "ROLE_EMPLOYER");

            await AddUserWithPasswordAndRoleAsync(new VAC_TUser()
                {
                    Name = "TestAdmin",
                    UserName = "TestAdmin",
                    Address = "TestSquare 3",
                    Postcode = "9742EE",
                    Email = "admin@test.nl",
                    EmailConfirmed = true,
                }, 
                "t0p$ecretCod3", "ROLE_ADMIN");

            await AddUserWithPasswordAndRoleAsync(new VAC_TUser()
                {
                    Name = "TestJustJoinedUser",
                    UserName = "TestJustJoinedUser",
                    ProfilePicture = "assets/img/user/profile.png",
                    Email = "testuser@test.nl",
                    EmailConfirmed = true,
                },
                "pas$C0de", "ROLE_CANDIDATE");
        }

        public async Task AddTestCompaniesAsync()
        {
            await Company.AddRangeAsync(new Company()
            {
                Name= "TestCompany",
                Description = "A Test Company",
                LogoURL = "assets/img/company/testcompany.jpg",
                WebsiteURL = "http://testCompany.com",
                Address = "Somewherestreet 11",
                Postcode = "1122AB",
                Residence = "TestCity",
                User = Users.First(u => u.Name == "TestCompanyUser")
            });
            await SaveChangesAsync();
        }
        /*
                public async Task AddTestSolictations()
                {



                    await SaveChangesAsync();
                }
        */






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
        /// Create a user with a specific password
        /// </summary>
        /// <param name="user">the user</param>
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
