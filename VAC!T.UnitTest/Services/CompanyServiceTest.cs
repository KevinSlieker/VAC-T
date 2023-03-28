using VAC_T.Business;
using VAC_T.UnitTest.TestObjects;

namespace VAC_T.UnitTest.Services
{
    internal class CompanyServiceTest
    {
        private TestDbContext _context;
        private CompanyService _service;
        private int? testCompanyId;
        private int? someCompanyId;

        [SetUp]
        public async Task Setup()
        {
            // Setup the database in a different context
            using (TestDbContext context = new TestDbContext())
            {
                await context.SetupDatabase();
                await context.AddTestUsersAsync();
                await context.AddTestCompaniesAsync();
                await context.AddTestSolictations();
                // save the id's for later use
                testCompanyId = context.TestCompanyId;
                someCompanyId = context.SomeOtherCompanyId;
            }
            _context = new TestDbContext();
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
    }
}
