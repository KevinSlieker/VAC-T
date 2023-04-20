namespace VAC_T.Models
{
    public class CompanyDashBoardViewModel
    {
        public Company Company { get; set; } = new Company();
        public IEnumerable<Solicitation>? Solicitations { get; set; }
        public Dictionary<RepeatAppointment.RepeatsType, int> repeatAppointmentsAmount { get; set; } = new Dictionary<RepeatAppointment.RepeatsType, int>();
    }
}
