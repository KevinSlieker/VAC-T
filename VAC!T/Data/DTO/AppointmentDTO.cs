using System.ComponentModel.DataAnnotations;
using VAC_T.Models;

namespace VAC_T.Data.DTO
{
    public class AppointmentDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime Time { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsOnline { get; set; }
        public CompanyDTOSmall Company { get; set; }
        public int CompanyId { get; set; }
        public JobOfferDTOSmall? JobOffer { get; set; }
        public int? JobOfferId { get; set; }
        virtual public SolicitationDTOSmall? Solicitation { get; set; }
        public int? RepeatAppointmentId { get; set; }
    }
}
