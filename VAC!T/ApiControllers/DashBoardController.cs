﻿using AutoMapper;
using Highsoft.Web.Mvc.Charts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Data.DTO;
using VAC_T.Models;
using VAC_T.Services;

namespace VAC_T.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DashBoardController : Controller
    {
        private readonly IMapper _mapper;
        private readonly DashBoardService _service;

        public DashBoardController(DashBoardService service, IMapper mapper)
        {
            _mapper = mapper;
            _service = service;
        }

        /// <summary>
        /// Get the info to create an highchart for the logged in candidate.
        /// </summary>
        /// <returns>the info for the highchart</returns>
        /// <remarks>
        /// This data would be processed in the view.
        /// </remarks>
        [HttpGet("Candidate")]
        public async Task<ActionResult<DashBoardCandidateDTO>> GetDashBoardCandidateData()
        {
            if (!User.IsInRole("ROLE_CANDIDATE"))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                var solicitations = await _service.GetSolicitationsAsync(User);
                var total = solicitations != null ? solicitations.Count() : 0;
                var invited = solicitations != null && solicitations.Where(s => s.Selected == true) != null ? solicitations.Where(s => s.Selected == true).Count() : 0;
                var percentage = total != 0 ? Math.Round((double)(100 * invited) / total, 1) : 0;
                var solicitations2 = solicitations != null ? solicitations.Where(s => s.DateAppointmentSelected != null).ToList() : null;
                var averageTimeDiff = solicitations2.Count() != 0 ? Math.Round((double)solicitations2.Select(s => ((s.DateAppointmentSelected!.Value - s.Date).TotalDays)).Average(), 1) : 0;
                //ViewData["total"] = (double)total;
                //ViewData["invited"] = (double)invited;
                //ViewData["percentage"] = percentage;
                //ViewData["AverageTimeDiff"] = averageTimeDiff;
                var result = _mapper.Map<List<SolicitationDTOComplete>>(solicitations);
                var dto = new DashBoardCandidateDTO() { Solicitations = result, Total = (double)total, Invited = (double)invited , Percentage = percentage, AverageTimeDiff = averageTimeDiff};
                return Ok(dto);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        /// <summary>
        /// Get the info to create an highchart for the logged in employer.
        /// </summary>
        /// <returns>the info for the highchart</returns>
        /// <remarks>
        /// This data would be processed in the view.
        /// </remarks>
        [HttpGet("Company")]
        public async Task<ActionResult<DashBoardCompanyDTO>> GetDashBoardCompanyData()
        {
            if (!User.IsInRole("ROLE_EMPLOYER"))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                var solicitations = await _service.GetSolicitationsAsync(User);
                var companyInfo = await _service.GetCompanyAsync(User);
                var repeatAppointmentsAmount = await _service.GetAmountRepeatAppointmentsLast6MonthsAsync(companyInfo.Id);
                var appointments = await _service.GetCompanyAppointmentsAsync(User);

                var jobOffers = companyInfo.JobOffers != null ? companyInfo.JobOffers.Count() : 0;
                var solicitationsCount = solicitations != null ? solicitations.Count() : 0;
                var averageSolicitationsPerJobOffer = solicitations != null ? Math.Round((double)solicitationsCount / jobOffers, 1) : 0;
                var invited = solicitations != null ? solicitations.Where(s => s.Selected == true).Count() : 0;
                var percentageInvited = solicitationsCount != 0 ? Math.Round((double)(100 * invited) / solicitationsCount, 1) : 100;
                var solicitationsWAppointment = solicitations != null ? solicitations.Where(s => s.DateAppointmentSelected != null).Count() : 0;
                var percentageSolicitationsWAppointment = solicitationsCount != 0 ? Math.Round((double)(100 * solicitationsWAppointment) / invited, 1) : 100;
                var averageJobOfferLifeSpan = companyInfo.JobOffers != null ? (companyInfo.JobOffers.Any(j => j.Closed != null) ? Math.Round((double)companyInfo.JobOffers.Where(j => j.Closed != null).Select(j => ((j.Closed!.Value - j.Created).TotalDays)).Average(), 1) : 0) : 0;

                //ViewData["jobOffers"] = (double)jobOffers;
                //ViewData["averageJobOfferLifeSpan"] = averageJobOfferLifeSpan;
                //ViewData["averageSolicitationsPerJobOffer"] = averageSolicitationsPerJobOffer;
                //ViewData["percentageInvited"] = percentageInvited;
                //ViewData["percentageSolicitationsWAppointment"] = percentageSolicitationsWAppointment;

                var enums = Enum.GetValues(typeof(RepeatAppointment.RepeatsType));
                List<string> list = new List<string>();
                foreach (RepeatAppointment.RepeatsType name in enums)
                {
                    list.Add(name.GetSingleDisplayName()!);
                }
                List<ColumnSeriesData> selectedRepeatAppointments = new List<ColumnSeriesData>();
                List<ColumnSeriesData> notSelectedRepeatAppointments = new List<ColumnSeriesData>();
                foreach (KeyValuePair<RepeatAppointment.RepeatsType, int> s in repeatAppointmentsAmount)
                {
                    var valueSelected = 0;
                    if (appointments != null)
                    {
                        var wRepeat = appointments.Where(a => a.RepeatAppointment != null).Where(a => a.Date.CompareTo(DateTime.Today.AddMonths(-6)) == 1).Any() ? appointments.Where(a => a.RepeatAppointment != null).Where(a => a.Date.CompareTo(DateTime.Today.AddMonths(-6)) == 1).ToList() : null;
                        var selected = wRepeat != null ? (wRepeat.Any(a => a.RepeatAppointment!.Repeats == s.Key) ? wRepeat.Where(a => a.RepeatAppointment!.Repeats == s.Key).ToList() : null) : null;
                        valueSelected = selected != null ? selected.Count() : 0;

                    }
                    selectedRepeatAppointments.Add(new ColumnSeriesData { Y = valueSelected });
                    notSelectedRepeatAppointments.Add(new ColumnSeriesData { Y = (s.Value - valueSelected) });
                };
                //ViewData["list"] = list;
                //ViewData["selectedRepeatAppointments"] = selectedRepeatAppointments;
                //ViewData["notSelectedRepeatAppointments"] = notSelectedRepeatAppointments;
                var result = _mapper.Map<CompanyDTOSmall>(companyInfo);
                var dto = new DashBoardCompanyDTO() { Company = result, JobOffers = (double)jobOffers, AverageJobOfferLifeSpan = averageJobOfferLifeSpan,
                AverageSolicitationsPerJobOffer = averageSolicitationsPerJobOffer, PercentageInvited = percentageInvited, PercentageSolicitationsWAppointment = percentageSolicitationsWAppointment,
                List = list, SelectedRepeatAppointments = selectedRepeatAppointments, NotSelectedRepeatAppointments = notSelectedRepeatAppointments};
                return Ok(dto);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        /// <summary>
        /// Get the info to create an highchart for the logged in admin.
        /// </summary>
        /// <returns>the info for the highchart</returns>
        /// <remarks>
        /// This data would be processed in the view.
        /// </remarks>
        [HttpGet("Admin")]
        public async Task<ActionResult<DashBoardAdminDTO>> GetDashBoardAdminData()
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                var jobOffers = await _service.GetJobOffersAsync();
                var averageJobOfferLifeSpan = jobOffers != null ? Math.Round((double)jobOffers.Where(j => j.Closed != null).Select(j => ((j.Closed!.Value - j.Created).TotalDays)).Average(), 1) : 0;
                var solicitations = await _service.GetSolicitationsAsync(User);
                var total = solicitations != null ? solicitations.Count() : 0;
                var invited = solicitations != null ? solicitations.Where(s => s.Selected == true).Count() : 0;
                var percentageInvited = total != 0 ? Math.Round((double)(100 * invited) / total, 1) : 0;
                var solicitationsWAppointment = solicitations != null ? solicitations.Where(s => s.DateAppointmentSelected != null).ToList() : null;
                var activeReactionsToJobOffers = solicitationsWAppointment != null ? solicitationsWAppointment.Where(s => s.DateAppointmentSelected!.Value.CompareTo(s.DateSelectedIsTrue!.Value.AddDays(14)) <= 0).Count() : 0;
                var percentageActiveReactionJobOffer = invited != 0 ? Math.Round((double)(100 * activeReactionsToJobOffers) / invited, 1) : 100;

                var jobOfferLevels = jobOffers.Select(s => s.Level).Distinct().ToList();
                List<ColumnSeriesData> averageSolicitationsPerJobOfferLevel = new List<ColumnSeriesData>();

                foreach (var level in jobOfferLevels)
                {
                    var jobOffersPerLevelCount = jobOffers.Where(j => j.Level == level).Count();
                    var solicitationsPerLevel = solicitations != null ? solicitations.Where(s => s.JobOffer.Level == level).Count() : 0;
                    var averageSolicitationPerLevel = solicitationsPerLevel != 0 ? Math.Round((double)(solicitationsPerLevel) / jobOffersPerLevelCount, 1) : 0;
                    averageSolicitationsPerJobOfferLevel.Add(new ColumnSeriesData { Y = averageSolicitationPerLevel, Name = level });
                }

                //ViewData["averageJobOfferLifeSpan"] = averageJobOfferLifeSpan;
                //ViewData["percentageInvited"] = percentageInvited;
                //ViewData["percentageActiveReactionJobOffer"] = percentageActiveReactionJobOffer;
                //ViewData["jobOfferLevels"] = jobOfferLevels;
                //ViewData["averageSolicitationsPerJobOfferLevel"] = averageSolicitationsPerJobOfferLevel;
                var dto = new DashBoardAdminDTO() { AverageJobOfferLifeSpan = averageJobOfferLifeSpan, PercentageInvited = percentageInvited,
                PercentageActiveReactionJobOffer = percentageActiveReactionJobOffer, AverageSolicitationsPerJobOfferLevel = averageSolicitationsPerJobOfferLevel};
                return Ok(dto);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }
    }
}
