using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAC_T.Business;
using VAC_T.UnitTest.TestObjects;

namespace VAC_T.UnitTest.Services
{
    internal class CompanyServiceTest
    {
        private TestDbContext _context;
        private CompanyService _service;

        [SetUp]
        public async Task Setup()
        {
            _context = new TestDbContext();
            await _context.SetupDatabase();
            await _context.AddTestUsersAsync();
            await _context.AddTestCompaniesAsync();

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
            var id = _context.Company.First().Id;

            // run 
            var company = await _service.GetCompanyAsync(id);

            // validate
            Assert.That(company, Is.Not.Null);
            Assert.That(company.Name, Is.EqualTo("TestCompany"));
            Assert.That(company.User, Is.Not.Null);
            Assert.That(company.User.Name, Is.EqualTo("TestCompanyUser"));
            Assert.That(company.Appointments, Is.Null);
        }
    }
}
