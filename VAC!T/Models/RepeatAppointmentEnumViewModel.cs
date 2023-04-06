using System.ComponentModel.DataAnnotations;
using static VAC_T.Models.RepeatAppointment;

namespace VAC_T.Models
{
    public class RepeatAppointmentEnumViewModel
    {
        public int Id { get; set; } // repeatAppointment Id
        public RepeatsType Repeats { get; set; }
        [Range(1, 31, ErrorMessage = "De dag moet minimaal tussen {1} en {2} zijn.")]
        [Display(Name ="Dag:")]
        public int? RepeatsDay { get; set; } = null;
        [Display(Name = "Maandag")]
        public bool IsMonday { get; set; } = false;
        [Display(Name = "Dinsdag")]
        public bool IsTuesday { get; set; } = false;
        [Display(Name = "Woensdag")]
        public bool IsWednesday { get; set; } = false;
        [Display(Name = "Donderdag")]
        public bool IsThursday { get; set; } = false;
        [Display(Name = "Vrijdag")]
        public bool IsFriday { get; set; } = false;
        [Display(Name = "Eerste")]
        public bool IsFirst { get; set; } = false;
        [Display(Name = "Tweede")]
        public bool IsSecond { get; set; } = false;
        [Display(Name = "Derde")]
        public bool IsThird { get; set; } = false;
        [Display(Name = "Vierde")]
        public bool IsFourth { get; set; } = false;
        [Display(Name = "Laatste")]
        public bool IsLast { get; set; } = false;

    }
}
