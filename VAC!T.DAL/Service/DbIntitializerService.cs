using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VAC_T.DAL.Exceptions;
using VAC_T.Data;
using VAC_T.Models;

namespace VAC_T.DAL.Service
{
    public class DbIntitializerService
    {
        private readonly IVact_TDbContext _context;

        public DbIntitializerService(IVact_TDbContext context)
        {
            _context = context;
        }

        public async Task<List<Appointment>?> GetAvailableAppointmentsAsync(int id, DateTime date) //solicitation id
        {
            var solicitation = await _context.Solicitation.Include(s => s.JobOffer.Company.User).FirstAsync(s => s.Id == id);
            if (solicitation == null)
            {
                return null;
            }
            var appointments = await _context.Appointment.Where(a => a.Company == solicitation.JobOffer.Company)
                .Where(a => a.JobOfferId == null || a.JobOfferId == solicitation.JobOffer.Id)
                .Where(a => a.Solicitation == null).OrderByDescending(a => a.Date).OrderByDescending(a => a.Time).ToListAsync();

            appointments = await GetAvailableRepeatAppointmentsAsync(appointments, solicitation.JobOffer.Company.Id, date);

            return appointments;
        }

        public async Task<List<Appointment>?> GetAvailableRepeatAppointmentsAsync(List<Appointment>? appointments, int companyId, DateTime dateGiven)
        {
            var repeatAppointments = await _context.RepeatAppointment.Where(ra => ra.CompanyId == companyId).Include(ra => ra.Company).ToListAsync();
            if (repeatAppointments == null)
            {
                return appointments;
            }
            var takenRepeatAppointments = await _context.Appointment.Where(a => a.RepeatAppointmentId != null).ToListAsync();

            var dateNow = dateGiven;
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
    }
}
