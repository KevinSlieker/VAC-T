﻿using System.Globalization;
using System.Security.Claims;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VAC_T.DAL.Exceptions;
using VAC_T.Data;
using VAC_T.Models;

namespace VAC_T.Business
{
    public class AnswerService
    {
        private readonly IVact_TDbContext _context;
        private readonly UserManager<VAC_TUser> _userManager;

        public AnswerService(IVact_TDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Answer>?> GetAnswersAsync(ClaimsPrincipal User)
        {
            if (_context.Answer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var answers = from s in _context.Answer.Include(a => a.JobOffer).Include(a => a.Question).Include(a => a.User) select s;
            if (User.IsInRole("ROLE_CANDIDATE"))
            {
                answers = answers.Where(a => a.User == user);
            }
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                var company = await _context.Company.Where(a => a.User == user).FirstAsync();
                answers = answers.Where(a => a.JobOffer.CompanyId == company.Id);
            }
            return (await answers.ToListAsync()).DistinctBy(a => new { a.JobOffer, a.User });
            //var answers = await _context.Answer.Include(a => a.JobOffer).Include(a => a.Question).Include(a => a.User).ToListAsync();
            //return answers.DistinctBy(a => new { a.JobOffer, a.User });
            //await _context.JobOffer.SelectMany(j => j.Answers).DistinctBy(a => a.User).ToListAsync();
        }

        public async Task<Answer?> GetAnswerAsync(int id, ClaimsPrincipal User)
        {
            if (_context.Answer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var answer = from s in _context.Answer
                .Include(a => a.JobOffer)
                .Include(a => a.Question.Options)
                .Include(a => a.User)
                         select s;
            if (User.IsInRole("ROLE_CANDIDATE"))
            {
                var user = await _userManager.GetUserAsync(User);
                answer = answer.Where(a => a.User == user);
            }
            return await answer.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Question?> GetQuestionAsync(int id)
        {
            if (_context.Question == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.Question
                .Include(a => a.Options)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<JobOffer>> GetJobOffersAsync()
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.JobOffer.Where(j => j.Closed == null).Where(j => j.Questions.Count() >= 1).ToListAsync();
        }

        public async Task<VAC_TUser> GetUserAsync(ClaimsPrincipal User)
        {
            if (_context.Users == null)
            {
                throw new InternalServerException("Database not found");
            }
            return (await _userManager.GetUserAsync(User))!;
        }

        //public async Task<IEnumerable<Answer>?> GetAnswersForJobOfferAsync(int id, ClaimsPrincipal User)
        //{
        //    if (_context.Answer == null)
        //    {
        //        throw new InternalServerException("Database not found");
        //    }
        //    var user = await _userManager.GetUserAsync(User);
        //    return await _context.Answer.Include(a => a.JobOffer).Include(a => a.Question.Options).Where(a => a.JobOfferId == id).Where(a => a.UserId == user.Id).ToListAsync();
        //}

        public async Task<IEnumerable<Answer>?> GetAnswersForJobOfferAsync(int id, string userId, ClaimsPrincipal User) // jobOfferId
        {
            if (_context.Answer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var answers = from s in _context.Answer.Include(a => a.JobOffer).Include(a => a.Question.Options).Include(a => a.User).Where(a => a.JobOfferId == id).Where(a => a.UserId == userId) select s;
            if (User.IsInRole("ROLE_CANDIDATE"))
            {
               if (user.Id != userId)
                {
                    return null;
                }
            }
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                answers = answers.Where(a => a.JobOffer.Company.User == user);
            }
            return await answers.ToListAsync();
        }

        public async Task<bool> DoAnswersExistAsync(int id, ClaimsPrincipal User) // jobOffer id
        {
            if (_context.Answer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            return await _context.Answer.Where(a => a.JobOfferId == id).Where(a => a.User == user).AnyAsync();
        }

        public async Task<bool> DoAnswersExistAsync(int id, string userId, ClaimsPrincipal User) // jobOffer id
        {
            if (_context.Answer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var answers = from s in _context.Answer.Where(a => a.JobOfferId == id).Where(a => a.UserId == userId) select s;
            if (User.IsInRole("ROLE_CANDIDATE"))
            {
                var user = await _userManager.GetUserAsync(User);
                answers = answers.Where(a => a.User == user);
            }
            return await answers.AnyAsync();
        }

        //public async Task<bool> DoesAnswerExistAsync(int id)
        //{
        //    if (_context.Answer == null)
        //    {
        //        throw new InternalServerException("Database not found");
        //    }
        //    return await _context.Answer.AnyAsync(a => a.Id == id);
        //}

        //public async Task<IEnumerable<Answer>?> GetUserAnswersForJobOfferAsync(int id, ClaimsPrincipal User) // jobOffer id
        //{
        //    if (_context.Answer == null)
        //    {
        //        throw new InternalServerException("Database not found");
        //    }
        //    var user = await _userManager.GetUserAsync(User);
        //    return await _context.Answer.Include(a => a.JobOffer).Include(a => a.Question).Where(a => a.JobOfferId == id).Where(a => a.User == user).ToListAsync();
        //}

        public async Task<List<Answer>?> PrepareUserAnswersForCreateAsync(int id, ClaimsPrincipal User) // jobOffer id
        {
            if (_context.Answer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var answers = new List<Answer>();
            var jobOffer = await _context.JobOffer.Include(j => j.Questions).FirstOrDefaultAsync(j => j.Id == id);
            if (jobOffer == null || jobOffer.Questions.Count() == 0)
            {
                return null;
            }
            var user = await _userManager.GetUserAsync(User);
            foreach (var question in jobOffer.Questions)
            {
                var answer = new Answer()
                {
                    QuestionId = question.Id,
                    Question = await _context.Question.Include(q => q.Options).FirstAsync(q => q.Id == question.Id),
                    JobOfferId = id,
                    JobOffer = jobOffer,
                    UserId = user.Id,
                    User = user,
                };
                answers.Add(answer);
            }
            return answers;
        }

        public async Task<List<Answer>> CreateAnswersAsync(List<Answer> answers)
        {
            if (_context.Answer == null)
            {
                throw new InternalServerException("Database not found");
            }
            _context.Answer.AddRange(answers);
            await _context.SaveChangesAsync();
            return answers;
        }

        public async Task UpdateAnswerAsync(Answer answer)
        {
            if (_context.Answer == null)
            {
                throw new InternalServerException("Database not found");
            }

            var jobOffer = await _context.JobOffer.FindAsync(answer.JobOfferId);
            var user = await _context.Users.FindAsync(answer.UserId);
            var question = await _context.Question.FindAsync(answer.QuestionId);
            answer.JobOffer = jobOffer!;
            answer.User = user!;
            answer.Question = question!;
            _context.Answer.Update(answer);
            await _context.SaveChangesAsync();
        }

        //public async Task DeleteAnswerAsync(int id)
        //{
        //    if (_context.Answer == null)
        //    {
        //        throw new InternalServerException("Database not found");
        //    }
        //    var answer = await _context.Answer.FindAsync(id);
        //    _context.Answer.Remove(answer);
        //    await _context.SaveChangesAsync();
        //}

        public async Task DeleteUserAnswersForJobOfferAsync(int id, string userId) // jobOffer id
        {
            if (_context.Answer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var answers = await _context.Answer.Where(a => a.JobOfferId == id).Where(a => a.UserId == userId).ToListAsync();
            _context.Answer.RemoveRange(answers);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckUserSolicitatedAsync(int id, string userId) // jobOfferId
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.Solicitation.AnyAsync(s => s.UserId == userId && s.JobOffer.Id == id);
        }

        public string PrepareAnswerForDisplay(string answerString, Question question)
        {
            if (_context.Question == null)
            {
                throw new InternalServerException("Database not found");
            }
            switch (question.Type)
            {
                //case "Open":
                //    return answerString;
                case "Standpunt":
                    var returnAnswer = "0 " + question.Options.First().OptionLong + " - " + question.Options.Last().OptionLong + " 100" + ": " + answerString;
                    return returnAnswer;
                //case "Ja/Nee":
                //    return answerString;
                case "Meerkeuze":
                    if (question.MultipleOptions == false)
                    {
                        if (answerString == "Anders")
                        {
                            return answerString;
                        }
                        else
                        {
                            var id = int.Parse(answerString);
                            return question.Options.First(o => o.Id == int.Parse(answerString)).OptionLong;
                        }
                    }
                    else
                    {
                        var toReturn = "";
                        var answerIds = answerString.Split('_').ToList();
                        var anders = "";
                        if (answerIds.Contains("Anders"))
                        {
                            answerIds.Remove("Anders");
                            anders = " Anders";
                        }
                        foreach (var id in answerIds)
                        {
                            toReturn += " " + question.Options.First(o => o.Id == int.Parse(id)).OptionLong + ",";
                        }
                        if (!anders.IsNullOrEmpty())
                        {
                            toReturn += anders;
                        }
                        return toReturn.Trim(',').Trim();
                    }
                default:
                    return answerString;
            }
        }

        public string PrepareAnswerForCSV(string answerString, Question question)
        {
            if (_context.Question == null)
            {
                throw new InternalServerException("Database not found");
            }
            switch (question.Type)
            {
                //case "Open":
                //    return answerString;
                case "Standpunt":
                    var returnAnswer = "0 " + (question.Options.First().OptionShort == null ? question.Options.First().OptionLong : question.Options.First().OptionShort) +
                        " - " + (question.Options.Last().OptionShort == null ? question.Options.Last().OptionLong : question.Options.Last().OptionShort) + " 100" + ": " + answerString;
                    return returnAnswer;
                //case "Ja/Nee":
                //    return answerString;
                case "Meerkeuze":
                    if (question.MultipleOptions == false)
                    {
                        if (answerString == "Anders")
                        {
                            return answerString;
                        }
                        else
                        {
                            var id = int.Parse(answerString);
                            return question.Options!.First(o => o.Id == int.Parse(answerString)).OptionShort == null ?
                                question.Options!.First(o => o.Id == int.Parse(answerString)).OptionLong :
                                question.Options!.First(o => o.Id == int.Parse(answerString)).OptionShort!;
                        }
                    }
                    else
                    {
                        var toReturn = "";
                        var answerIds = answerString.Split('_').ToList();
                        var anders = "";
                        if (answerIds.Contains("Anders"))
                        {
                            answerIds.Remove("Anders");
                            anders = "Anders";
                        }
                        foreach (var id in answerIds)
                        {
                            toReturn += (question.Options!.First(o => o.Id == int.Parse(id)).OptionShort == null ?
                                question.Options!.First(o => o.Id == int.Parse(id)).OptionLong :
                                question.Options!.First(o => o.Id == int.Parse(id)).OptionShort) + ",";
                        }
                        if (!anders.IsNullOrEmpty())
                        {
                            toReturn += anders;
                        }
                        return toReturn.Trim(',').Trim();
                    }
                default:
                    return answerString;
            }
        }

        public async Task<MemoryStream> CreateCSVFileAsync(IEnumerable<Answer> answers)
        {
            var rptLines = new List<CSVLine>();
            rptLines = answers!.ToList().ConvertAll(a => new CSVLine()
            {
                Question = a.Question.QuestionText,
                Answer = PrepareAnswerForCSV(a.AnswerText, a.Question),
                Explanation = a.Explanation
            });
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
            };
            var ms = new MemoryStream();
            using (var sw = new StreamWriter(ms))
            {
                using (var csv = new CsvWriter(sw, config))
                {
                    csv.WriteField(answers.First().JobOffer.Name);
                    csv.WriteField(DateTime.Today.ToShortDateString());
                    csv.WriteField(answers.First().User.Name);
                    await csv.NextRecordAsync();
                    await csv.WriteRecordsAsync(rptLines);
                }
                return ms;
            }
        }
    }
}
