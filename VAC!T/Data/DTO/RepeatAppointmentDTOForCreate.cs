using static VAC_T.Models.RepeatAppointment;

namespace VAC_T.Data.DTO
{
    public class RepeatAppointmentDTOForCreate
    {
        public int Id { get; set; }
        public RepeatsType Repeats { get; set; }
        public DateTime Time { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsOnline { get; set; }
    }
}
