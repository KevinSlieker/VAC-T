using Highsoft.Web.Mvc.Charts;

namespace VAC_T.Data.DTO
{
    public class DashBoardAdminDTO
    {
        public double AverageJobOfferLifeSpan { get; set; }
        public double PercentageInvited { get; set; }
        public double PercentageActiveReactionJobOffer { get; set; }
        public List<ColumnSeriesData> AverageSolicitationsPerJobOfferLevel { get; set; } = new List<ColumnSeriesData>();
    }
}
