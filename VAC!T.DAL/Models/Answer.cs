using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAC_T.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public Question Question { get; set; } = new Question();
        public int QuestionId { get; set; }
        public JobOffer JobOffer { get; set; } = new JobOffer();
        public int JobOfferId { get; set; }
        public VAC_TUser User { get; set; } = new VAC_TUser();
        public string UserId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public string? Explanation { get; set; }
    }
}
