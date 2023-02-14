using System.ComponentModel.DataAnnotations;

namespace VAC_T.Models
{
    public class UserDetailsModel
    {
        public string Id { get; set; }
        public string? Name { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        public string? Address { get; set; }

        [DataType(DataType.PostalCode)]
        public string? Postcode { get; set; }

        public string? Residence { get; set; }

        [DataType(DataType.ImageUrl)]
        public string? ProfilePicture { get; set; }

        public string? Motivation { get; set; }

        [DataType(DataType.Url)]
        public string? CV { get; set; }
    }
}
