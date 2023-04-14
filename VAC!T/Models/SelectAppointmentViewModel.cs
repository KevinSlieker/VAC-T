namespace VAC_T.Models
{
    public class SelectAppointmentViewModel
    {
        public IEnumerable<Appointment>? Appointments { get; set; }
        public int SolicitationId { get; set; }
        public string SelectedAppointmentId { get; set; }

    }
}
