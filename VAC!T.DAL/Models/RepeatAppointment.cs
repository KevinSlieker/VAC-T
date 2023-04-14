using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAC_T.Models;

namespace VAC_T.Models
{
    public class RepeatAppointment
    {
        public int Id { get; set; }
        public Company Company { get; set; }
        public int CompanyId { get; set; }
        [Display(Name = "Herhaling")]
        public RepeatsType Repeats { get; set; }
        public Repeats_Weekdays? RepeatsWeekdays { get; set; } = null;
        [Range(1, 31, ErrorMessage = "De dag moet minimaal tussen {1} en {2} zijn.")]
        public int? RepeatsDay { get; set; } = null;
        public Repeats_Relative_Week? RepeatsRelativeWeek { get; set; } = null;// 1st , 2nd or ...(monday or ...) of the month (monthlyRelative
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start tijd")]
        public DateTime Time { get; set; }
        [Display(Name = "Duur")]
        public TimeSpan Duration { get; set; }
        public bool IsOnline { get; set; }
        virtual public ICollection<Appointment>? Appointments { get; set; }

        public enum RepeatsType
        {
            [Display(Name = "Dagelijks")]
            Daily = 1,
            [Display(Name = "Weekelijks")]
            Weekly = 2,
            [Display(Name = "Maandelijks")]
            Monthly = 3,
            [Display(Name = "MaandelijksRelatief")]
            MonthlyRelative = 4,
        }

        [Flags]
        public enum Repeats_Weekdays
        {
            [Display(Name = "Maandag")]
            Monday = 1,
            [Display(Name = "Dinsdag")]
            Tuesday = 2,
            [Display(Name = "Woensdag")]
            Wednesday = 4,
            [Display(Name = "Donderdag")]
            Thursday = 8,
            [Display(Name = "Vrijdag")]
            Friday = 16,
        }

        [Flags]
        public enum Repeats_Relative_Week
        {
            [Display(Name = "Eerste")]
            First = 1,
            [Display(Name = "Tweede")]
            Second = 2,
            [Display(Name = "Derde")]
            Third = 4,
            [Display(Name = "Vierde")]
            Fourth = 8,
            [Display(Name = "Laatste")]
            Last = 16,
        }
    }
}
