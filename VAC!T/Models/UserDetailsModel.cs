using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAC_T.Models
{
    [NotMapped]
    public class UserDetailsModel
    {
        public string Id { get; set; }

        [Display(Name = "Naam")]
        public string? Name { get; set; }

        [Phone]
        [Display(Name = "Telefoonnummer")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Geboortedatum")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Adres")]
        public string? Address { get; set; }

        [DataType(DataType.PostalCode)]
        [Display(Name = "Postcode")]
        public string? Postcode { get; set; }

        [Display(Name = "Woonplaats")]
        public string? Residence { get; set; }

        [DataType(DataType.ImageUrl)]
        public string? ProfilePicture { get; set; }

        [Display(Name = "Motivatie")]
        public string? Motivation { get; set; }

        [DataType(DataType.Url)]
        public string? CV { get; set; }

        public string Role { get; set; }
    }
}
