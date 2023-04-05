namespace VAC_T.Models
{
    public class AppointmentViewModel
    {
        public IEnumerable<Appointment> Appointments { get; set; }
        public IEnumerable<RepeatAppointment> RepeatAppointments { get; set; }
    }
}
