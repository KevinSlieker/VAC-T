using System.ComponentModel;

namespace VAC_T.Models
{
    public class AnswerViewModel
    {
        public int Id { get; set; }
        public Question Question { get; set; }
        public int QuestionId { get; set; }
        public JobOffer JobOffer { get; set; }
        public int JobOfferId { get; set; }
        public VAC_TUser User { get; set; }
        public string UserId { get; set; }
        [DisplayName("Meerkeuze antwoorden")]
        public string[] MultipleChoiceAnswers { get; set; }
        [DisplayName("Antwoord")]
        public string AnswerText { get; set; }
        [DisplayName("Antwoord")]
        public string? DisplayAnswerText { get; set; }
        [DisplayName("Uitleg")]
        public string? Explanation { get; set; }
    }
}
