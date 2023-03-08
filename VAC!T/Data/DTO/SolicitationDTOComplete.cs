namespace VAC_T.Data.DTO
{
    public class SolicitationDTOComplete : SolicitationDTOSmall
    {
        public JobOfferDTOSmall JobOffer { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
