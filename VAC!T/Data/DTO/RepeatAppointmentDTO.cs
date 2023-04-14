using static VAC_T.Models.RepeatAppointment;
using System.ComponentModel.DataAnnotations;

namespace VAC_T.Data.DTO
{
    public class RepeatAppointmentDTO
    {
        public int Id { get; set; }
        public CompanyDTOSmall Company { get; set; }
        public int CompanyId { get; set; }
        public RepeatsType Repeats { get; set; }
        public Repeats_Weekdays? RepeatsWeekdays { get; set; } = null;
        public int? RepeatsDay { get; set; } = null;
        public Repeats_Relative_Week? RepeatsRelativeWeek { get; set; } = null;// 1st , 2nd or ...(monday or ...) of the month (monthlyRelative)
        public DateTime Time { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsOnline { get; set; }
    }
}
