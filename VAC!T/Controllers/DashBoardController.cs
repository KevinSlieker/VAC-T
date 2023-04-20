using Microsoft.AspNetCore.Mvc;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Models;

namespace VAC_T.Controllers
{
    public class DashBoardController : Controller
    {
        private DashBoardService _service;

        public DashBoardController(DashBoardService service)
        {
            _service = service;
        }

        public async Task<IActionResult> DashBoardCandidate()
        {
            try
            {
                var solicitations = await _service.GetSolicitationsAsync(User);
                return View(solicitations);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Solicitation' is null.");
            }
        }

        public async Task<IActionResult> DashBoardCompany()
        {
            try
            {
                var solicitations = await _service.GetSolicitationsAsync(User);
                var companyInfo = await _service.GetCompanyAsync(User);
                var repeatAppointmentsAmount = await _service.GetAmountRepeatAppointmentsLast30DaysAsync(companyInfo.Id);
                var viewModel = new CompanyDashBoardViewModel() { Company = companyInfo, Solicitations = solicitations, repeatAppointmentsAmount = repeatAppointmentsAmount };
                return View(viewModel);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Solicitation' is null.");
            }
        }

        //public async Task<ActionResult<WebImage?>> DrawCandidateChart1() 
        //{
        //    try
        //    {
        //        var solicitations = await _service.GetSolicitationsAsync();
        //        var total = solicitations.Count();
        //        var invited = solicitations.Where(s => s.Selected == true).Count();
        //        var percentage = (int)Math.Round((double)(100 * invited) / total);
        //        var solicitationsTime = solicitations.Where(s => s.DateAppointmentSelected != null).ToList();
        //        var averageTimeDiff = solicitationsTime.Select(s => ((s.DateAppointmentSelected!.Value - s.Date).TotalDays)).Average();

        //        var myChart = new Chart(width: 600, height: 400)
        //            .AddTitle("Solicitaties")
        //            .AddSeries(
        //                name: "Naam",
        //                xValue: new[] { "Aantal solicitaties", "Percentage uitgenodigd", "Tijd tussen solicitatie en afspraakplannen" },
        //                yValues: new[] { total, percentage, averageTimeDiff })
        //            .ToWebImage("jpng");
        //        return myChart;
        //    }
        //    catch (InternalServerException)
        //    {
        //        return Problem("Entity set 'ApplicationDbContext.Solicitation' is null.");
        //    }
        //}
    }
}
