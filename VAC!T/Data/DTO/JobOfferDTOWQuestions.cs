namespace VAC_T.Data.DTO
{
    public class JobOfferDTOWQuestions : JobOfferDTOSmall
    {
        public ICollection<QuestionDTOSmall>? Questions { get; set; }
    }
}
