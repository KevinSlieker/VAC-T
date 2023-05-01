using VAC_T.Models;

namespace VAC_T.Data.DTO
{
    public class DashBoardCandidateDTO
    {
        public ICollection<SolicitationDTOComplete>? Solicitations { get; set; }
        public double Total { get; set; }
        public double Invited { get; set; }
        public double Percentage { get; set; }
        public double AverageTimeDiff { get; set; }
    }
}
