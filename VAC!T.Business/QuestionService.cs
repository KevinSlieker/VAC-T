using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VAC_T.DAL.Exceptions;
using VAC_T.Data;
using VAC_T.Models;

namespace VAC_T.Business
{
    public class QuestionService
    {
        private readonly IVact_TDbContext _context;
        private readonly UserManager<VAC_TUser> _userManager;

        public QuestionService(IVact_TDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Question>> GetQuestionsAsync(ClaimsPrincipal User)
        {
            if (_context.Question == null)
            {
                throw new InternalServerException("Database not found");
            }
            var questions = from q in _context.Question.Include(q => q.Options).Include(q => q.Company) select q;
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                var user = await _userManager.GetUserAsync(User);
                var company = await _context.Company.FirstAsync(c => c.User == user);
                questions = questions.Where(q => q.CompanyId == company!.Id);
            }
            return await questions.ToListAsync();
        }

        public async Task<Question?> GetQuestionAsync(int Id, ClaimsPrincipal User)
        {
            if (_context.Question == null)
            {
                throw new InternalServerException("Database not found");
            }
            var question = from q in _context.Question.Include(q => q.Options).Include(q => q.Company) select q;
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                var user = await _userManager.GetUserAsync(User);
                var company = await _context.Company.FirstAsync(c => c.User == user);
                question = question.Where(q => q.CompanyId == company!.Id);
            }
            return await question.FirstOrDefaultAsync(q => q.Id == Id);
        }
        public async Task<List<string>> GetYesOrNoTextOptionsAsync()
        {
            if (_context.Question == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.Question.Where(q => q.CompanyId == null).Select(q => q.QuestionText).ToListAsync();
        }

        public async Task<QuestionOption?> GetQuestionOptionAsync(int Id, ClaimsPrincipal User)
        {
            if (_context.QuestionOption == null)
            {
                throw new InternalServerException("Database not found");
            }
            var questionOption = from q in _context.QuestionOption.Include(q => q.Question) select q;
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                var user = await _userManager.GetUserAsync(User);
                var company = await _context.Company.FirstAsync(c => c.User == user);
                questionOption = questionOption.Where(q => q.Question.CompanyId == company!.Id);
            }
            return await questionOption.FirstOrDefaultAsync(q => q.Id == Id);
        }

        public async Task<Company> GetCompanyAsync(ClaimsPrincipal User)
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var company = await _context.Company.Where(c => c.User == user).FirstAsync();
            return company;
        }

        public async Task<IEnumerable<Company>> GetCompaniesAsync()
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }
            var companies = await _context.Company.ToListAsync();
            return companies;
        }

        public async Task<Question> CreateQuestionAsync(Question question)
        {
            if (_context.Question == null)
            {
                throw new InternalServerException("Database not found");
            }
            if (question.CompanyId != null)
            {
                question.Company = await _context.Company.FirstOrDefaultAsync(q => q.Id == question.CompanyId);
            }
            if (question.Type == "Ja/Nee")
            {
                question.CompanyId = null;
                question.Company = null;
                var options = new List<QuestionOption>
                {
                    new QuestionOption() { QuestionId = question.Id, Question = question, OptionLong = "Ja" },
                    new QuestionOption() { QuestionId = question.Id, Question = question, OptionLong = "Nee" }
                };
                _context.QuestionOption.AddRange(options);
            }
            if (question.Type != "Meerkeuze")
            {
                question.MultipleOptions = false;
                question.ExplanationType = string.Empty;
            }
            _context.Question.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<Question> CreateYesOrNoQuestionAsync(Question question)
        {
            if (_context.Question == null)
            {
                throw new InternalServerException("Database not found");
            }
            if (question.CompanyId != null)
            {
                question.Company = await _context.Company.FirstOrDefaultAsync(q => q.Id == question.CompanyId);
            }
            var options = new List<QuestionOption>
                {
                    new QuestionOption() { QuestionId = question.Id, Question = question, OptionLong = "Ja" },
                    new QuestionOption() { QuestionId = question.Id, Question = question, OptionLong = "Nee" }
                };
            _context.QuestionOption.AddRange(options);
            _context.Question.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<List<QuestionOption>> CreateQuestionOptionsAsync(List<QuestionOption> questionOptions)
        {
            if (_context.Question == null)
            {
                throw new InternalServerException("Database not found");
            }
            var question = await _context.Question.FirstAsync(q => q.Id == questionOptions.First().QuestionId);
            foreach (var option in questionOptions)
            {
                option.Question = question;
            }
            _context.QuestionOption.AddRange(questionOptions);
            await _context.SaveChangesAsync();
            return questionOptions;
        }

        public async Task<bool> DoesQuestionExistsAsync(int id)
        {
            if (_context.Question == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.Question.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> DoQuestionOptionsExistsAsync(IEnumerable<QuestionOption> questionOptions)
        {
            if (_context.QuestionOption == null)
            {
                throw new InternalServerException("Database not found");
            }
            var result = true;
            foreach (var option in questionOptions)
            {
                var check = await _context.QuestionOption.AnyAsync(c => c.Id == option.Id);
                if (check == false)
                {
                    result = false;
                }
            }
            return result;
        }

        public async Task<bool> DoesQuestionOptionExistsAsync(int id)
        {
            if (_context.QuestionOption == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.QuestionOption.AnyAsync(q => q.Id == id);
        }
        public async Task UpdateQuestionAsync(Question question)
        {
            if (_context.Question == null)
            {
                throw new InternalServerException("Database not found");
            }
            var oldType = await _context.Question.Where(q => q.Id == question.Id).Select(q => q.Type).FirstOrDefaultAsync();
            var oldExplanationType = await _context.Question.Where(q => q.Id == question.Id).Select(q => q.ExplanationType).FirstOrDefaultAsync();
            if (oldType != null)
            {
                if (oldType != question.Type)
                {
                    if (oldType == "Meerkeuze" || oldType == "Standpunt")
                    {
                        var oldOptions = await _context.QuestionOption.Where(q => q.QuestionId == question.Id).ToListAsync();
                        _context.QuestionOption.RemoveRange(oldOptions);
                    }
                }
            }
            if (question.Type != "Meerkeuze")
            {
                question.MultipleOptions = false;
                question.ExplanationType = string.Empty;
            }
            _context.Question.Update(question);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuestionOptionAsync(QuestionOption questionOption) // , ClaimsPrincipal User
        {
            if (_context.QuestionOption == null)
            {
                throw new InternalServerException("Database not found");
            }
            _context.QuestionOption.Update(questionOption);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuestionAsync(int id) // , ClaimsPrincipal User
        {
            if (_context.Question == null)
            {
                throw new InternalServerException("Database not found");
            }
            var question = await _context.Question.Include(q => q.Options).FirstOrDefaultAsync(q => q.Id == id);
            if (question == null)
            {
                return;
            }
            //if (User.IsInRole("ROLE_EMPLOYER"))
            //{
            //    var user = await _userManager.GetUserAsync(User);
            //    var company = _context.Company.Where(x => x.User == user).FirstAsync();
            //    if (question.CompanyId != company.Id)
            //    {
            //        return;
            //    }
            //}
            if (question.Options != null)
            {
                _context.QuestionOption.RemoveRange(question.Options);
            }
            _context.Question.Remove(question);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuestionOptionAsync(int id)
        {
            if (_context.QuestionOption == null)
            {
                throw new InternalServerException("Database not found");
            }
            var option = await _context.QuestionOption.Include(q => q.Question).FirstOrDefaultAsync(q => q.Id == id);
            if (option == null)
            {
                return;
            }
            if (option.Question.Type != "Meerkeuze")
            {
                return;
            }
            _context.QuestionOption.Remove(option);
            await _context.SaveChangesAsync();
        }
    }
}
