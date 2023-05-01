using Highsoft.Web.Mvc.Charts;
using VAC_T.Models;

namespace VAC_T.Data.DTO
{
    public class DashBoardCompanyDTO
    {
        public CompanyDTOSmall? Company { get; set; }
        public double JobOffers { get; set; }
        public double AverageJobOfferLifeSpan { get; set; }
        public double AverageSolicitationsPerJobOffer { get; set; }
        public double PercentageInvited { get; set; }
        public double PercentageSolicitationsWAppointment { get; set; }
        public List<string> List { get; set; } = new List<string>();
        public List<ColumnSeriesData> SelectedRepeatAppointments { get; set; } = new List<ColumnSeriesData>();
        public List<ColumnSeriesData> NotSelectedRepeatAppointments { get; set; } = new List<ColumnSeriesData>();
    }
}
