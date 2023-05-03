using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAC_T.Models
{
    public class Question
    {
        public int Id { get; set; }
        public Company? Company { get; set; }
        public int? CompanyId { get; set; }
        public string Type { get; set; } = string.Empty; // Open, Multiple choice and standpoint between 2.
        public string QuestionText { get; set; } = string.Empty;
        public bool? MultipleOptions { get; set; }
        public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
        public string ExplanationType { get; set; } = string.Empty;
        public int OptionsAmount { get; set; } = 2;
        public ICollection<JobOffer> JobOffers { get; set; } = new List<JobOffer>();
        public ICollection<Answer>? Answers { get; set; }
    }
}
