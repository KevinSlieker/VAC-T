namespace VAC_T.Models
{
    public class JobOffer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Company Company { get; set; }
        public DateTime Created { get; set; }
        public ICollection<Solicitation> Solicitations { get; set; }
    }
}
