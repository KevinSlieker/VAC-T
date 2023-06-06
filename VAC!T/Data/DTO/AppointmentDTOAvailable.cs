using System.ComponentModel.DataAnnotations;

namespace VAC_T.Data.DTO
{
    public class AppointmentDTOAvailable
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MMMM-yyyy}", ApplyFormatInEditMode = false)]
        public DateTime Date { get; set; } = DateTime.Now;
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Time { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsOnline { get; set; }
        public int CompanyId { get; set; }
        public int? RepeatAppointmentId { get; set; }
        public string InputForSelect { get; set; }
    }
}
