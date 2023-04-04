//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Data.Sqlite;
//using Moq;
//using VAC_T.Business;
//using VAC_T.UnitTest.TestObjects;

//namespace VAC_T.UnitTest.Services
//{
//    internal class FileUploadServiceTest
//    {
//        private SqliteConnection _inMemoryDb;
//        private TestDbContext _context;
//        private FileUploadService _service;
//        private int? testJobOffer1Id;
//        private int? testCompanyId;
//        private int? testUserSolicitationId1;
//        private int? testUserSolicitationId2;
//        private int? testAppointmentJobOffer1Id;
//        private int? testAppointmentAnyJobOfferId;

//        [SetUp]
//        public async Task Setup()
//        {
//            _inMemoryDb = new SqliteConnection("Filename=:memory:");
//            _inMemoryDb.Open();

//            // Setup the database in a different context
//            using (TestDbContext context = new TestDbContext(_inMemoryDb))
//            {
//                await context.SetupDatabase();
//                await context.AddTestUsersAsync();
//                await context.AddTestCompaniesAsync();
//                await context.AddTestSolictations();
//                // save the id's for later use
//                testJobOffer1Id = context.TestCompanyJobOffer1Id;
//                testCompanyId = context.TestCompanyId;
//                testUserSolicitationId1 = context.TestUserSolicitationTestCompanyWithAppointmentForJobOffer1Id;
//                testUserSolicitationId2 = context.TestUserSolicitationTestCompanyForJobOffer2Id;
//                testAppointmentJobOffer1Id = context.TestCompanyAppointmentForJobOffer1Id;
//                testAppointmentAnyJobOfferId = context.TestCompanyAppointmentForAnyJobOfferId;

//            }
//            _context = new TestDbContext(_inMemoryDb);
//            _service = new FileUploadService(_context, _context.UserManager);
//        }

//        [Test]
//        public async Task TestUploadProfilePicture()
//        {
//            // prepare
//            var user = _context.Users.FirstOrDefault(u => u.Name == "testUser");

//            var fileMock = new Mock<IFormFile>();
//            //Setup mock file using a memory stream
//            var content = "Hello World from a Fake File";
//            var fileName = "test.pdf";
//            var ms = new MemoryStream();
//            var writer = new StreamWriter(ms);
//            writer.Write(content);
//            writer.Flush();
//            ms.Position = 0;
//            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
//            fileMock.Setup(_ => _.FileName).Returns(fileName);
//            fileMock.Setup(_ => _.Length).Returns(ms.Length);
//            fileMock.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
//            .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream))
//            .Verifiable();
//            fileMock.Setup(_ => _.ContentDisposition).Returns(string.Format("inline; filename={0}", fileName));
//            var file = fileMock.Object;

//            // run
//            await _service.UploadProfilePictureAsync(user, file);

//            // validate
//            var updatedUser = _context.Users.FirstOrDefault(u => u.Name == "testUser");
//            Assert.That(updatedUser, Is.Not.Null);
//            Assert.That(updatedUser.ProfilePicture, Is.EqualTo("assets" + "img" + "user" + user.Id + ".pdf"));
//        }
//    }
//}
