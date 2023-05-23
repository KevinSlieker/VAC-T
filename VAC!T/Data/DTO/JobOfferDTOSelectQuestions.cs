namespace VAC_T.Data.DTO
{
    public class JobOfferDTOSelectQuestions
    {
        public int Id { get; set; }
        public ICollection<QuestionDTOId>? Questions { get; set; }
    }
}
