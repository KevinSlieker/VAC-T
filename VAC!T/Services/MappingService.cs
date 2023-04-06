using static VAC_T.Models.RepeatAppointment;
using VAC_T.Models;

namespace VAC_T.Services
{
    public class MappingService
    {
        public static Repeats_Weekdays MapRepeatsWeekdays(RepeatAppointmentEnumViewModel src)
        {
            Repeats_Weekdays result =
            (src.IsMonday ? Repeats_Weekdays.Monday : 0) |
            (src.IsTuesday ? Repeats_Weekdays.Tuesday : 0) |
            (src.IsWednesday ? Repeats_Weekdays.Wednesday : 0) |
            (src.IsThursday ? Repeats_Weekdays.Thursday : 0) |
            (src.IsFriday ? Repeats_Weekdays.Friday : 0);
            return result;
        }

        public static Repeats_Relative_Week MapRepeatsRelativeWeek(RepeatAppointmentEnumViewModel src)
        {
            Repeats_Relative_Week result =
            (src.IsFirst ? Repeats_Relative_Week.First : 0) |
            (src.IsSecond ? Repeats_Relative_Week.Second : 0) |
            (src.IsThird ? Repeats_Relative_Week.Third : 0) |
            (src.IsFourth ? Repeats_Relative_Week.Fourth : 0) |
            (src.IsLast ? Repeats_Relative_Week.Last : 0);
            return result;
        }
    }
}
