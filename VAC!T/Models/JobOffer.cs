using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAC_T.Models
{
    public class JobOffer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [DataType(DataType.ImageUrl)]
        public string LogoURL { get; set; }
        public Company Company { get; set; }

        public string Level { get; set; }

        public string? Residence { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get; set; } = DateTime.Now;
        public ICollection<Solicitation> Solicitations { get; set; }
    }
}
