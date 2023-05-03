using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace VAC_T.Models
{
    public class JobOffer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Vacature")]
        public string Name { get; set; } = "";

        [Display(Name = "beschrijving")]
        public string Description { get; set; } = "";

        [DataType(DataType.ImageUrl)]
        public string LogoURL { get; set; } = "";

        [JsonIgnore]
        public Company Company { get; set; }

        public int CompanyId { get; set; }

        [Display(Name = "Niveau")]
        public string Level { get; set; } = "";

        public string? Residence { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MMMM-yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Datum")]
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? Closed { get; set; } = null;
        public ICollection<Solicitation> Solicitations { get; set; } = new List<Solicitation>();
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
