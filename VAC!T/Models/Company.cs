using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAC_T.Models
{
    public class Company
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Bedrijf")]
        public string Name { get; set; }

        [Display(Name = "Beschrijving")]
        public string Description { get; set; }

        [DataType(DataType.ImageUrl)]
        public string LogoURL { get; set; }

        [DataType(DataType.Url)]
        [Display(Name = "Website")]
        public string WebsiteURL { get; set; }

        [Display(Name = "Adres")]
        public string Address { get; set; }

        [DataType(DataType.PostalCode)]
        public string? Postcode { get; set; }

        [Display(Name = "Plaats")]
        public string? Residence { get; set; }
        public VAC_TUser? User { get; set; }

        public ICollection<JobOffer>? JobOffers { get; set; }
        public ICollection<Appointment>? Appointments { get; set;}
    }
}
