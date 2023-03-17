using System.ComponentModel.DataAnnotations;

namespace VAC_T.Data.DTO
{
    public class UserDetailsDTO
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? Postcode { get; set; }
        public string? Residence { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Motivation { get; set; }
        public string? CV { get; set; }
        public string Role { get; set; }
    }
}
