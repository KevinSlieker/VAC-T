using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VAC_T.Data;
using VAC_T.Models;

namespace VAC_T.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<VAC_TUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<VAC_TUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }

            var user = await _userManager.GetUserAsync(User);
            var applications = _context.Appointment.Include(a => a.Company.User).Include(a => a.Solicitation.User).Include(a => a.JobOffer);
            if (User.IsInRole("ROLE_EMPLOYER"))
            {
                applications = _context.Appointment.Where(a => a.Company.User == user).Include(a => a.Solicitation.User).Include(a => a.Company).Include(a => a.JobOffer);
            }
            return View(await applications.ToListAsync());
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Appointment == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointment
                .Include(a => a.Solicitation.User)
                .Include(a => a.Company)
                .Include(a => a.JobOffer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create()
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            var appointment = new Appointment();
            var user = await _userManager.GetUserAsync(User);
            var company = await _context.Company.Where(c => c.User == user).FirstAsync();
            appointment.Company = company;
            appointment.CompanyId = company.Id;
            ViewData["JobOfferId"] = new SelectList(_context.JobOffer.Where(j => j.Company == company), "Id", "Name");
            //appointment.Date = DateTime.Now.Date;
            return View(appointment);
        }

        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Time,Duration,IsOnline,CompanyId,SolicitationId,JobOfferId")] Appointment appointment)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var user = await _userManager.GetUserAsync(User);
            ViewData["JobOfferId"] = new SelectList(_context.JobOffer.Where(j => j.Company.User == user), "Id", "Name");
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            if (id == null || _context.Appointment == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointment.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(User);
            ViewData["JobOfferId"] = new SelectList(_context.JobOffer.Where(j => j.Company.User == user), "Id", "Name");
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Time,Duration,IsOnline,CompanyId,SolicitationId,JobOfferId")] Appointment appointment)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            if (id != appointment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var user = await _userManager.GetUserAsync(User);
            ViewData["JobOfferId"] = new SelectList(_context.JobOffer.Where(j => j.Company.User == user), "Id", "Name");
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            if (id == null || _context.Appointment == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointment
                .Include(a => a.Solicitation.User)
                .Include(a => a.Company)
                .Include(a => a.JobOffer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            if (_context.Appointment == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointment'  is null.");
            }
            var appointment = await _context.Appointment.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointment.Remove(appointment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Select(int id) // solicitation id
        {
            if (_context.Appointment == null)
            {
                return NotFound();
            }
            var solicitation = await _context.Solicitation.Include(s => s.JobOffer.Company.User).FirstAsync(s => s.Id == id);

            var appointments = await _context.Appointment.Where(a => a.Company == solicitation.JobOffer.Company)
                .Where(a => a.JobOfferId == null || a.JobOfferId == solicitation.JobOffer.Id)
                .Where(a => a.Solicitation == null).OrderByDescending(a => a.Date).OrderByDescending(a => a.Time).ToListAsync();
            if (appointments == null)
            {
                return NotFound();
            }
            ViewData["SolicitationId"] = solicitation.Id;
            //ViewData["AppointmentId"] = new SelectList(appointments, "Id", "Date");
            //ViewBag.Appointments = appointments;
            return View(appointments);
        }

        [HttpPost, ActionName("Select")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectConfirmed(int appointmentId, int solicitationId)
        {
            if (_context.Appointment == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointment'  is null.");
            }
            if (appointmentId == 0 || solicitationId == 0)
            {
                return RedirectToAction(nameof(Select), solicitationId);
            }
            var appointment = await _context.Appointment.FindAsync(appointmentId);
            var solicitation = await _context.Solicitation.Include(s => s.JobOffer).FirstAsync(s => s.Id == solicitationId);
            if (appointment == null || solicitation == null)
            {
                return NotFound();
            }
            appointment.SolicitationId= solicitation.Id;
            appointment.JobOfferId = solicitation.JobOffer.Id;

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            solicitation.AppointmentId= appointment.Id;
            _context.Update(solicitation);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Solicitations");
        }

        private bool AppointmentExists(int id)
        {
          return (_context.Appointment?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
