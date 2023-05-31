namespace VAC_T.Data.DTO
{
    public class AnswerDTOExtended : AnswerDTOComplete
    {
        public string[] MultipleChoiceAnswers { get; set; }
        public string? DisplayAnswerText { get; set; }
    }
}
