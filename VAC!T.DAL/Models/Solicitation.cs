using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAC_T.Models
{
    public class Solicitation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Sollicitant")]
        public VAC_TUser User { get; set; }
        public string UserId { get; set; }
        public JobOffer JobOffer { get; set; }

        [Display(Name = "Uitgenodigd")]
        public bool Selected { get; set; } = false;

        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Sollicitatie Gesprek")]
        public Appointment? Appointment { get; set; }
        public int? AppointmentId { get; set; }
        public DateTime? DateAppointmentSelected { get; set; } = null;
        public DateTime? DateSelectedIsTrue { get; set; } = null;
    }
}
