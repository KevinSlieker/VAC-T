namespace VAC_T.Models
{
    public class Solicitation
    {
        public int Id { get; set; }
        public VAC_TUser User { get; set; }
        public JobOffer JobOffer { get; set; }
        public bool Selected { get; set; }
        public DateTime Date { get; set; }

    }
}
