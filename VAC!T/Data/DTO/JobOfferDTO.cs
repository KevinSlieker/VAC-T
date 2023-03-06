using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VAC_T.Models;

namespace VAC_T.Data.DTO
{
    public class JobOfferDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoURL { get; set; }
        //public CompanyDTO Company { get; set; }
        public string CompanyName { get; set; }
        public int CompanyId { get; set; }
        public string Level { get; set; }
        public string? Residence { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        //public List<SolicitationDTO>? Solicitations { get; set; }

    }
}
