using System.ComponentModel.DataAnnotations.Schema;

namespace VAC_T.Models
{
    public class Company
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoURL { get; set; }
        public string WebsiteURL { get; set; }
        public string Address { get; set; }
        public VAC_TUser User { get; set; }
        public ICollection<JobOffer> JobOffers { get; set; }
    }
}
