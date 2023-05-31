namespace VAC_T.Data.DTO
{
    public class AnswerDTOMedium : AnswerDTOSmall
    {
        public int QuestionId { get; set; }
        public JobOfferDTOSmall JobOffer { get; set; }
        public int JobOfferId { get; set; }
        public UserDTOSmall User { get; set; }
        public string UserId { get; set; }
    }
}
