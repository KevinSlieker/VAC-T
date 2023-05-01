using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VAC_T.DAL.Exceptions;
using VAC_T.Data;
using VAC_T.Models;

namespace VAC_T.Business
{
    public class AppointmentService
    {
        private readonly IVact_TDbContext _context;
        private readonly UserManager<VAC_TUser> _userManager;

        public AppointmentService(IVact_TDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsAsync(ClaimsPrincipal User)
        {

            if (_context.Appointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var appointments = _context.Appointment.Include(a => a.Company.User).Include(a => a.Solicitation.User).Include(a => a.JobOffer);
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                await DeleteOldOpenAppointmentsAsync(User);
                appointments = _context.Appointment.Where(a => a.Company.User == user).Include(a => a.Solicitation.User).Include(a => a.Company).Include(a => a.JobOffer);
            }
            return await appointments.OrderBy(a => a.Company.Name).ThenByDescending(a => a.Date).ThenByDescending(a => a.Time).ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentAsync(int id)
        {
            if (_context.Appointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            var appointment = await _context.Appointment
                .Include(a => a.Solicitation.User)
                .Include(a => a.Company)
                .Include(a => a.JobOffer)
                .FirstOrDefaultAsync(m => m.Id == id);
            return appointment;
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment, ClaimsPrincipal User)
        {
            if (_context.Appointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var company = await _context.Company.Where(c => c.User == user).FirstOrDefaultAsync();
            appointment.Company = company;
            appointment.CompanyId = company.Id;
            appointment.Time = appointment.Date.Add(appointment.Time.TimeOfDay);
            _context.Appointment.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<Company> GetCompanyAsync(ClaimsPrincipal User)
        {
            if (_context.Company == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var company = await _context.Company.Where(c => c.User == user).FirstOrDefaultAsync();
            return company;
        }

        public IEnumerable<JobOffer> GetJobOffersForSelectListAsync(Company company)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var jobOffers = _context.JobOffer.Where(j => j.Company == company);
            return jobOffers;
        }

        public async Task<IEnumerable<JobOffer>> GetJobOffersForSelectListWithUserAsync(ClaimsPrincipal User)
        {
            if (_context.JobOffer == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var jobOffers = _context.JobOffer.Where(j => j.Company.User == user);
            return jobOffers;
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            if (_context.Appointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            appointment.Time = appointment.Date.Add(appointment.Time.TimeOfDay);
            _context.Appointment.Update(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DoesAppointmentExistAsync(int id)
        {
            if (_context.Appointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.Appointment.AnyAsync(a => a.Id == id);
        }

        public async Task DeleteAppointmentAsync(int id)
        {
            if (_context.Appointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            var appointment = await _context.Appointment.FindAsync(id);
            if (appointment == null)
            {
                return;
            }
            _context.Appointment.Remove(appointment);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get a list of appointments that are available by the solicitation id.
        /// </summary>
        /// <returns>A list of entries or null. </returns>
        public async Task<IEnumerable<Appointment>?> GetAvailableAppointmentsAsync(int id) //solicitation id
        {
            if (_context.Appointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            var solicitation = await _context.Solicitation.Include(s => s.JobOffer.Company.User).FirstAsync(s => s.Id == id);
            if (solicitation == null)
            {
                return null;
            }
            var appointments = await _context.Appointment.Where(a => a.Company == solicitation.JobOffer.Company)
                .Where(a => a.JobOfferId == null || a.JobOfferId == solicitation.JobOffer.Id)
                .Where(a => a.Solicitation == null).Where(a => a.Date.CompareTo(DateTime.Now) == 1).OrderByDescending(a => a.Date).OrderByDescending(a => a.Time).ToListAsync();

            appointments = await GetAvailableRepeatAppointmentsAsync(appointments, solicitation.JobOffer.Company.Id);

            return appointments;
        }

        public async Task<bool> DoesSolicitationExistsAsync(int id)
        {
            if (_context.Solicitation == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.Solicitation.AnyAsync(c => c.Id == id);
        }

        public async Task SelectAppointmentAsync(int appointmentId, int solicitationId)
        {
            if (_context.Appointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            var appointment = await _context.Appointment.FindAsync(appointmentId);
            var solicitation = await _context.Solicitation.Include(s => s.JobOffer).FirstAsync(s => s.Id == solicitationId);
            if (appointment == null || solicitation == null)
            {
                return;
            }
            appointment.Solicitation = solicitation;
            //appointment.JobOffer = solicitation.JobOffer;
            solicitation.Appointment = appointment;
            solicitation.DateAppointmentSelected = DateTime.Now;
            _context.Appointment.Update(appointment);
            _context.Solicitation.Update(solicitation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOldOpenAppointmentsAsync(ClaimsPrincipal User)
        {
            if (_context.Appointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            DateTime dateTime = DateTime.Now;
            var user = await _userManager.GetUserAsync(User);
            var company = await _context.Company.Where(c => c.User == user).FirstAsync();
            var appointments = await _context.Appointment.Where(a => a.CompanyId == company.Id).Where(a => a.Solicitation == null)
                .Where(a => a.Date.CompareTo(dateTime) < 0).ToListAsync();
            _context.Appointment.RemoveRange(appointments);
            await _context.SaveChangesAsync();
        }

        public async Task<RepeatAppointment?> GetRepeatAppointmentAsync(int id)
        {
            if (_context.RepeatAppointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            var repeatAppointment = await _context.RepeatAppointment
                .Include(a => a.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
            return repeatAppointment;
        }
        public async Task<IEnumerable<RepeatAppointment>> GetRepeatAppointmentsAsync(ClaimsPrincipal User)
        {
            if (_context.RepeatAppointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            var user = await _userManager.GetUserAsync(User);
            var repeatAppointments = _context.RepeatAppointment
                .Include(a => a.Company);
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                await DeleteOldOpenAppointmentsAsync(User);
                repeatAppointments = _context.RepeatAppointment.Where(a => a.Company.User == user).Include(a => a.Company);
            }
            return await repeatAppointments.ToListAsync();
        }

        public async Task<RepeatAppointment> CreateRepeatAppointmentAsync(RepeatAppointment repeatAppointment, ClaimsPrincipal User)
        {
            if (_context.RepeatAppointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            var company = await GetCompanyAsync(User);
            repeatAppointment.CompanyId = company.Id;
            repeatAppointment.Company = company;
            _context.RepeatAppointment.Add(repeatAppointment);
            await _context.SaveChangesAsync();
            return repeatAppointment;
        }

        public async Task UpdateRepeatAppointmentAsync(RepeatAppointment repeatAppointment)
        {
            if (_context.RepeatAppointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            var old = await _context.RepeatAppointment.Where(ra => ra.Id == repeatAppointment.Id).Select(a => a.Repeats).FirstOrDefaultAsync();
            if (old != null)
            {
                var newRepeats = repeatAppointment.Repeats;
                if (old != newRepeats)
                {
                    repeatAppointment.RepeatsDay = null;
                    repeatAppointment.RepeatsRelativeWeek = null;
                    repeatAppointment.RepeatsWeekdays = null;
                }
            }

            _context.RepeatAppointment.Update(repeatAppointment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DoesRepeatAppointmentExistAsync(int id)
        {
            if (_context.RepeatAppointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            return await _context.RepeatAppointment.AnyAsync(a => a.Id == id);
        }

        public async Task DeleteRepeatAppointmentAsync(int id)
        {
            if (_context.RepeatAppointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            var repeatAppointment = await _context.RepeatAppointment.FindAsync(id);
            if (repeatAppointment == null)
            {
                return;
            }
            _context.RepeatAppointment.Remove(repeatAppointment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Appointment>?> GetAvailableRepeatAppointmentsAsync(List<Appointment>? appointments, int companyId)
        {
            if (_context.RepeatAppointment == null)
            {
                throw new InternalServerException("Database not found");
            }

            var repeatAppointments = await _context.RepeatAppointment.Where(ra => ra.CompanyId == companyId).Include(ra => ra.Company).ToListAsync();
            if (repeatAppointments == null)
            {
                return appointments;
            }
            var takenRepeatAppointments = await _context.Appointment.Where(a => a.RepeatAppointmentId != null).ToListAsync();

            var dateNow = DateTime.Today;
            var threeWeeksFromNow = dateNow.AddDays(21);
            foreach (var repeatAppointment in repeatAppointments)
            {
                var date = dateNow;
                while (date <= threeWeeksFromNow)
                {
                    if (repeatAppointment.Repeats == RepeatAppointment.RepeatsType.Daily)
                    {
                        date = date.AddDays(1);
                        if (takenRepeatAppointments.Where(a => a.Date == date).Where(a => a.RepeatAppointmentId == repeatAppointment.Id).Any())
                        {
                            continue;
                        }
                        if (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
                        {
                            if (date <= threeWeeksFromNow)
                            {
                                var time = date.Date.Add(repeatAppointment.Time.TimeOfDay);
                                var appointment = new Appointment()
                                {
                                    Date = date,
                                    Time = time,
                                    Duration = repeatAppointment.Duration,
                                    IsOnline = repeatAppointment.IsOnline,
                                    Company = repeatAppointment.Company,
                                    CompanyId = companyId,
                                    RepeatAppointmentId = repeatAppointment.Id,
                                    RepeatAppointment = repeatAppointment,
                                };
                                appointments.Add(appointment);
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
                        if (takenRepeatAppointments.Where(a => a.Date == date).Where(a => a.RepeatAppointmentId == repeatAppointment.Id).Any())
                        {
                            continue;
                        }
                        var weekday = date.DayOfWeek;
                        var mask = 1 << ((int)weekday - 1);
                        if (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
                        {
                            if ((mask & (int)repeatAppointment.RepeatsWeekdays) == mask)
                            {
                                if (date <= threeWeeksFromNow)
                                {
                                    var time = date.Date.Add(repeatAppointment.Time.TimeOfDay);
                                    var appointment = new Appointment()
                                    {
                                        Date = date,
                                        Time = time,
                                        Duration = repeatAppointment.Duration,
                                        IsOnline = repeatAppointment.IsOnline,
                                        Company = repeatAppointment.Company,
                                        CompanyId = companyId,
                                        RepeatAppointmentId = repeatAppointment.Id,
                                        RepeatAppointment = repeatAppointment,
                                    };
                                    appointments.Add(appointment);
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
                        if (takenRepeatAppointments.Where(a => a.Date == date).Where(a => a.RepeatAppointmentId == repeatAppointment.Id).Any())
                        {
                            continue;
                        }
                        while (repeatAppointment.RepeatsDay != date.Day)
                        {
                            date = date.AddDays(1);
                        }
                        if (date.DayOfWeek != DayOfWeek.Sunday && date.DayOfWeek != DayOfWeek.Saturday)
                        {
                            if (date <= threeWeeksFromNow)
                            {
                                var time = date.Date.Add(repeatAppointment.Time.TimeOfDay);
                                var appointment = new Appointment()
                                {
                                    Date = date,
                                    Time = time,
                                    Duration = repeatAppointment.Duration,
                                    IsOnline = repeatAppointment.IsOnline,
                                    Company = repeatAppointment.Company,
                                    CompanyId = companyId,
                                    RepeatAppointmentId = repeatAppointment.Id,
                                    RepeatAppointment = repeatAppointment,
                                };
                                appointments.Add(appointment);
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
                        if (takenRepeatAppointments.Where(a => a.Date == date).Where(a => a.RepeatAppointmentId == repeatAppointment.Id).Any())
                        {
                            continue;
                        }
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
                                    if (date <= threeWeeksFromNow)
                                    {
                                        var time = date.Date.Add(repeatAppointment.Time.TimeOfDay);
                                        var appointment = new Appointment()
                                        {
                                            Date = date,
                                            Time = time,
                                            Duration = repeatAppointment.Duration,
                                            IsOnline = repeatAppointment.IsOnline,
                                            Company = repeatAppointment.Company,
                                            CompanyId = companyId,
                                            RepeatAppointmentId = repeatAppointment.Id,
                                            RepeatAppointment = repeatAppointment,
                                        };
                                        appointments.Add(appointment);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return appointments.OrderBy(a => a.Time).ToList();
        }

        public async Task SelectRepeatAppointmentAsync(int repeatAppointmentId, DateTime date, int solicitationId)
        {
            if (_context.Appointment == null)
            {
                throw new InternalServerException("Database not found");
            }
            var repeatAppointment = await _context.RepeatAppointment.Include(ra => ra.Company).FirstOrDefaultAsync(ra => ra.Id == repeatAppointmentId);
            var solicitation = await _context.Solicitation.Include(s => s.JobOffer).FirstOrDefaultAsync(s => s.Id == solicitationId);
            if (repeatAppointment == null || solicitation == null)
            {
                return;
            }
            var appointment = new Appointment()
            {
                Date = date.Date,
                Time = date,
                Duration = repeatAppointment.Duration,
                IsOnline = repeatAppointment.IsOnline,
                Company = repeatAppointment.Company,
                CompanyId = repeatAppointment.CompanyId,
                RepeatAppointment = repeatAppointment,
                RepeatAppointmentId= repeatAppointment.Id,
                Solicitation = solicitation,
                //appointment.JobOffer = solicitation.JobOffer;

            };
            solicitation.Appointment = appointment;
            solicitation.DateAppointmentSelected = DateTime.Now;
            await _context.Appointment.AddAsync(appointment);
            _context.Solicitation.Update(solicitation);
            await _context.SaveChangesAsync();
        }

    }
}
