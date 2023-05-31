namespace VAC_T.Data.DTO
{
    public class AnswerDTOForCreate : AnswerDTOSmall
    {
        public int QuestionId { get; set; }
        public string[] MultipleChoiceAnswers { get; set; }
    }
}
