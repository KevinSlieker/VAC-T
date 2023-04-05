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
            Monday = 1,
            Tuesday = 2,
            Wednesday = 4,
            Thursday = 8,
            Friday = 16,
        }

        [Flags]
        public enum Repeats_Relative_Week
        {
            Frist = 1,
            Second = 2,
            Third = 4,
            Fourth = 8,
            Last = 16,
        }
    }
}
