using System.ComponentModel.DataAnnotations;

namespace VAC_T.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MMMM-yyyy}", ApplyFormatInEditMode = false)]
        [Display(Name = "Datum")]
        public DateTime Date { get; set; } = DateTime.Now;
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start tijd")]
        public DateTime Time { get; set; }
        [Display(Name = "Duur")]
        public TimeSpan Duration { get; set; }
        [Display(Name = "Online")]
        public bool IsOnline { get; set; }
        public bool Available { get; set; } = true;
        public VAC_TUser? Employer { get; set; }
        public string? EmployerId { get; set; }
        public VAC_TUser? Candidate { get; set; }
        public string? CandidateId { get; set; }
        public JobOffer? JobOffer { get; set; }
        public int? JobOfferId { get;set; }

    }
}
