using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VAC_T.Business;
using VAC_T.Models;
using VAC_T.UnitTest.TestObjects;
using VACT.Data.Migrations;

namespace VAC_T.UnitTest.Services
{
    internal class CompanyServiceTest
    {
        private SqliteConnection _inMemoryDb;
        private TestDbContext _context;
        private CompanyService _service;
        private int? testCompanyId;
        private int? someCompanyId;

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
                testCompanyId = context.TestCompanyId;
                someCompanyId = context.SomeOtherCompanyId;
            }
            _context = new TestDbContext(_inMemoryDb);
            _service = new CompanyService(_context, _context.UserManager);
        }

        /// <summary>
        /// First test
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestGetCompany()
        {
            // prepare
            int id = testCompanyId!.Value;

            // run 
            var company = await _service.GetCompanyAsync(id);

            // validate
            Assert.That(company, Is.Not.Null);
            var companyEntry = _context.Entry(company!);

            Assert.That(company.Name, Is.EqualTo("TestCompany"));
            // Test eager and lazy loaded fields
            Assert.That(companyEntry.Reference(c => c.User).IsLoaded, Is.True);
            Assert.That(companyEntry.Collection(c => c.JobOffers!).IsLoaded, Is.True);
            Assert.That(companyEntry.Collection(c => c.Appointments!).IsLoaded, Is.False);

            Assert.That(company.User, Is.Not.Null);
            Assert.That(company.User.Name, Is.EqualTo("testCompanyUser"));

            Assert.That(company.JobOffers, Is.Not.Empty);                
            Assert.That(company.Appointments, Is.Null); 
        }

        [Test]
        public async Task TestGetCompanyWrongId()
        {
            // prepare
            int id = 0;

            // run
            var company = await _service.GetCompanyAsync(id);

            //
            Assert.That(company, Is.Null);
        }

        [Test]
        public async Task TestGetCompaniesNoSearchString()
        {
            // prepare
            var searchstring = "";

            // run
            var companies = await _service.GetCompaniesAsync(searchstring);

            // validate
            Assert.That(companies, Is.Not.Null);
            var companiesEntry = _context.Entry(companies.First()!);

            // Test eager and lazy loaded fields
            Assert.That(companiesEntry.Reference(c => c.User).IsLoaded, Is.False);
            Assert.That(companiesEntry.Collection(c => c.JobOffers!).IsLoaded, Is.False);
            Assert.That(companiesEntry.Collection(c => c.Appointments!).IsLoaded, Is.False);

            Assert.That(companies.First().JobOffers, Is.Null);
            Assert.That(companies.First().User, Is.Null);
            Assert.That(companies.First().Appointments, Is.Null);
            Assert.That(companies.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task TestGetCompaniesWithSearchString()
        {
            // prepare
            var searchstring = "Other";

            // run
            var companies = await _service.GetCompaniesAsync(searchstring);

            // validate
            Assert.That(companies, Is.Not.Null);
            var companiesEntry = _context.Entry(companies.First()!);

            Assert.That(companies.First().Name, Is.EqualTo("Some Other Company"));
            // Test eager and lazy loaded fields
            Assert.That(companiesEntry.Reference(c => c.User).IsLoaded, Is.False);
            Assert.That(companiesEntry.Collection(c => c.JobOffers!).IsLoaded, Is.False);
            Assert.That(companiesEntry.Collection(c => c.Appointments!).IsLoaded, Is.False);

            Assert.That(companies.First().JobOffers, Is.Null);
            Assert.That(companies.First().User, Is.Null);
            Assert.That(companies.First().Appointments, Is.Null);
            Assert.That(companies.Count, Is.EqualTo(1));
        }


        [Test]
        public async Task TestGetCompaniesWithSearchStringNoResult()
        {
            // prepare
            var searchstring = "NO RESULT";

            // run
            var companies = await _service.GetCompaniesAsync(searchstring);

            // validate
            Assert.That(companies, Is.Empty);
        }

        [Test]
        public async Task TestDoesCompanyExsists()
        {
            // prepare
            int id = testCompanyId!.Value;

            // run
            var result = await _service.DoesCompanyExistsAsync(id);

            // validate
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task TestDoesCompanyExsistsWrongId()
        {
            // prepare
            int id = 123456789;

            // run
            var result = await _service.DoesCompanyExistsAsync(id);

            // validate
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TestDoesCompanyExsistsIdIsZero()
        {
            // prepare
            int id = 0;

            // run
            var result = await _service.DoesCompanyExistsAsync(id);

            // validate
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task TestGetCompanyForUser()
        {
            // prepare
            var user = _context.Users.Where(u => u.Name == "testCompanyUser").First();
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
            var company = await _service.GetCompanyForUserAsync(claimsPrincipal);

            // validate
            Assert.That(company, Is.Not.Null);
            var companyEntry = _context.Entry(company!);

            Assert.That(company.Name, Is.EqualTo("TestCompany"));
            // Test eager and lazy loaded fields
            Assert.That(companyEntry.Reference(c => c.User).IsLoaded, Is.True);
            Assert.That(companyEntry.Collection(c => c.JobOffers!).IsLoaded, Is.True);
            Assert.That(companyEntry.Collection(c => c.Appointments!).IsLoaded, Is.False);

            Assert.That(company.User, Is.Not.Null);
            Assert.That(company.User.Name, Is.EqualTo("testCompanyUser"));

            Assert.That(company.JobOffers, Is.Not.Empty);
            Assert.That(company.Appointments, Is.Null);
        }

        [Test]
        public async Task TestGetCompanyForUserWrongUser()
        {
            // prepare
            var user = _context.Users.Where(u => u.Name == "testUser").First();
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
            var company = await _service.GetCompanyForUserAsync(claimsPrincipal);

            // validate
            Assert.That(company, Is.Null);
        }

        [Test]
        public async Task TestCreateCompanyWithUser()
        {
            // prepare
            var company = new Company() 
            {   Name = "TestCreate",
                Description = "A TestCreate Company",
                LogoURL = "assets/img/company/testcompany.jpg",
                WebsiteURL = "http://testCreateCompany.com",
                Address = "Createstreet 11",
                Postcode = "5262LZ",
                Residence = "TestCity",
            };

            // run
            var newCompany = await _service.CreateCompanyWithUserAsync(company);

            // validate
            Assert.That(newCompany, Is.Not.Null);
            var companyEntry = _context.Entry(newCompany!);

            Assert.That(newCompany.Name, Is.EqualTo("TestCreate"));
            // Test eager and lazy loaded fields
            Assert.That(companyEntry.Reference(c => c.User).IsLoaded, Is.True);
            Assert.That(companyEntry.Collection(c => c.JobOffers!).IsLoaded, Is.False);
            Assert.That(companyEntry.Collection(c => c.Appointments!).IsLoaded, Is.False);

            Assert.That(newCompany.JobOffers, Is.Null);
            Assert.That(newCompany.User, Is.Not.Null);
            Assert.That(newCompany.User.Name, Is.EqualTo("EmployerTestCreate"));
            Assert.That(newCompany.Appointments, Is.Null);
        }

        [Test]
        public async Task TestUpdateCompany()
        {
            // prepare
            int id = testCompanyId!.Value;
            var company = _context.Company.Find(id)!;
            company.Name = "TestUpdate";
            company.WebsiteURL = "http://testUpdateCompany.com";
            company.Address = "Updatestreet 11";

            // run
            await _service.UpdateCompanyAsync(company);

            // validate
            company = _context.Company.FirstOrDefault(c => c.Name == "TestUpdate");
            Assert.That(company, Is.Not.Null);
            Assert.That(company.Name, Is.EqualTo("TestUpdate"));
            Assert.That(company.WebsiteURL, Is.EqualTo("http://testUpdateCompany.com"));
            Assert.That(company.Address, Is.EqualTo("Updatestreet 11"));
        }

        [Test]
        public async Task TestDeleteCompany()
        {
            // prepare
            int id = testCompanyId!.Value;

            // run
            await _service.DeleteCompanyAsync(id);

            // validate
            var company = _context.Company.FirstOrDefault(c => c.Id == id);
            var user = _context.Users.FirstOrDefault(u => u.Name == "testCompanyUser");

            Assert.That(company, Is.Null);
            Assert.That(user, Is.Null);
        }

        [Test]
        public async Task TestDeleteCompanyWrongId()
        {
            // prepare
            int id = 123456;

            // run
            await _service.DeleteCompanyAsync(id);

            // validate
            var company = await _context.Company.ToListAsync();

            Assert.That(company, Is.Not.Null);
            Assert.That(company.Count, Is.EqualTo(2));
        }

    }
}
