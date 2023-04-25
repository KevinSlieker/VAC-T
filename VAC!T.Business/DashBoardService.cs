using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VAC_T.DAL.Exceptions;
using VAC_T.Data;
using VAC_T.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace VAC_T.Business
{
    public class DashBoardService
    {
        private readonly IVact_TDbContext _context;
        private readonly UserManager<VAC_TUser> _userManager;

        public DashBoardService(IVact_TDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Solicitation>> GetSolicitationsAsync(ClaimsPrincipal User)
        {
            if (_context.Solicitation == null)
            {
                throw new InternalServerException("Database not found");
            }

            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                var company = await _context.Company.Where(c => c.User == user).FirstAsync();
                return await _context.Solicitation.Where(s => s.JobOffer.Company == company).ToListAsync();
            }
            if (User.IsInRole("ROLE_CANDIDATE"))
            {
                return await _context.Solicitation.Where(s => s.User == user).ToListAsync();
            }
            return await _context.Solicitation.Include(s => s.JobOffer).ToListAsync();
        }

        public async Task<Company> GetCompanyAsync(ClaimsPrincipal User)
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            return await _context.Company.Where(c => c.User == user).Include(c => c.JobOffers).Include(c => c.Appointments).Include(c => c.RepeatAppointments).FirstAsync();
        }

        public async Task<IEnumerable<Appointment>> GetCompanyAppointmentsAsync(ClaimsPrincipal User)
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var company = await _context.Company.Where(c => c.User == user).Include(c => c.JobOffers).Include(c => c.Appointments).Include(c => c.RepeatAppointments).FirstAsync();
            return await _context.Appointment.Where(a => a.Company == company).Include(a => a.RepeatAppointment).ToListAsync();
        }

        public async Task<Dictionary<RepeatAppointment.RepeatsType,int>> GetAmountRepeatAppointmentsLast6MonthsAsync(int companyId)
        {
            if (_context.RepeatAppointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            var appointments = new Dictionary<RepeatAppointment.RepeatsType,int>() 
            { 
                { RepeatAppointment.RepeatsType.Daily, 0 },
                { RepeatAppointment.RepeatsType.Weekly, 0 },
                { RepeatAppointment.RepeatsType.Monthly, 0 },
                { RepeatAppointment.RepeatsType.MonthlyRelative, 0 }
            };
            var repeatAppointments = await _context.RepeatAppointment.Where(ra => ra.CompanyId == companyId).Include(ra => ra.Company).ToListAsync();
            if (repeatAppointments == null)
            {
                return appointments;
            }
            var date30DaysAgo = DateTime.Today.AddMonths(-6);
            var now = DateTime.Today;
            foreach (var repeatAppointment in repeatAppointments)
            {
                var date = date30DaysAgo;
                while (date <= now)
                {
                    if (repeatAppointment.Repeats == RepeatAppointment.RepeatsType.Daily)
                    {
                        date = date.AddDays(1);
                        if (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
                        {
                            if (date <= now)
                            {
                                appointments[RepeatAppointment.RepeatsType.Daily] += 1;
                            }
                        }
                    }

                    if (repeatAppointment.Repeats == RepeatAppointment.RepeatsType.Weekly)
                    {
                        if (repeatAppointment.RepeatsWeekdays.HasValue == false)
                        {
                            break;
                        }
                        date = date.AddDays(1);
                        var weekday = date.DayOfWeek;
                        var mask = 1 << ((int)weekday - 1);
                        if (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
                        {
                            if ((mask & (int)repeatAppointment.RepeatsWeekdays) == mask)
                            {
                                if (date <= now)
                                {
                                    appointments[RepeatAppointment.RepeatsType.Weekly] += 1;
                                }
                            }
                        }
                    }

                    if (repeatAppointment.Repeats == RepeatAppointment.RepeatsType.Monthly)
                    {
                        if (repeatAppointment.RepeatsDay.HasValue == false)
                        {
                            break;
                        }
                        date = date.AddDays(1);
                        while (repeatAppointment.RepeatsDay != date.Day)
                        {
                            date = date.AddDays(1);
                        }
                        if (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
                        {
                            if (date <= now)
                            {
                                appointments[RepeatAppointment.RepeatsType.Monthly] += 1;
                            }
                        }
                    }

                    if (repeatAppointment.Repeats == RepeatAppointment.RepeatsType.MonthlyRelative)
                    {
                        if (repeatAppointment.RepeatsWeekdays.HasValue == false || repeatAppointment.RepeatsRelativeWeek.HasValue == false)
                        {
                            break;
                        }
                        date = date.AddDays(1);
                        var weekday = date.DayOfWeek;
                        var mask = 1 << ((int)weekday - 1);
                        if (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
                        {
                            if ((mask & (int)repeatAppointment.RepeatsWeekdays) == mask)
                            {

                                var day = date.Day;
                                var weeknumber = 0;
                                if (day <= 7)
                                {
                                    weeknumber = 1;
                                }
                                if (day > 7 && day <= 14)
                                {
                                    weeknumber = 2;
                                }
                                if (day > 14 && day <= 21)
                                {
                                    weeknumber = 3;
                                }
                                if (day > 21 && day <= 28)
                                {
                                    weeknumber = 4;
                                }
                                if (day > 28)
                                {
                                    weeknumber = 5;
                                }
                                if (repeatAppointment.RepeatsRelativeWeek.Value.HasFlag(RepeatAppointment.Repeats_Relative_Week.Last))
                                {
                                    if (date.Month != date.AddDays(7).Month)
                                    {
                                        weeknumber = 5;
                                    }
                                }
                                var mask2 = 1 << ((int)weeknumber - 1);
                                if ((mask2 & (int)repeatAppointment.RepeatsRelativeWeek) == mask2)
                                {
                                    if (date <= now)
                                    {
                                        appointments[RepeatAppointment.RepeatsType.MonthlyRelative] += 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return appointments;
        }

        public async Task<IEnumerable<JobOffer>> GetJobOffersAsync()
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.JobOffer.ToListAsync();
        }
    }
}
