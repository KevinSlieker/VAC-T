using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAC_T.Models
{
    public class Company
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [DataType(DataType.ImageUrl)]
        public string LogoURL { get; set; }

        [DataType(DataType.Url)]
        public string WebsiteURL { get; set; }
        public string Address { get; set; }

        [DataType(DataType.PostalCode)]
        public string? Postcode { get; set; }

        public string? Residence { get; set; }
        public VAC_TUser User { get; set; }

        public ICollection<JobOffer> JobOffers { get; set; }
    }
}
