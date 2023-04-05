using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Models;

namespace VAC_T.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly AppointmentService _service;

        public AppointmentsController(AppointmentService service)
        {
            _service = service;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                AppointmentViewModel mymodel = new AppointmentViewModel();
                mymodel.Appointments = await _service.GetAppointmentsAsync(User);
                mymodel.RepeatAppointments = await _service.GetRepeatAppointmentsAsync();
                return View(mymodel);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointment' is null.");
            }
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var appointment = await _service.GetAppointmentAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }
                return View(appointment);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointment' is null.");
            }
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create()
        {
            if (!User.IsInRole("ROLE_EMPLOYER"))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var appointment = new Appointment();
                var company = await _service.GetCompanyAsync(User);
                if (company == null)
                {
                    return NotFound();
                }
                appointment.Company = company;
                appointment.CompanyId = company.Id;
                ViewData["JobOfferId"] = new SelectList(_service.GetJobOffersForSelectListAsync(company), "Id", "Name");
                return View(appointment);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointment' is null.");
            }
        }

        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Time,Duration,IsOnline,CompanyId,JobOfferId")] Appointment appointment)
        {
            if (!User.IsInRole("ROLE_EMPLOYER"))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                ModelState.Remove("Company");
                if (ModelState.IsValid)
                {
                    appointment = await _service.CreateAppointmentAsync(appointment, User);
                    return RedirectToAction(nameof(Index));
                }
                ViewData["JobOfferId"] = new SelectList(await _service.GetJobOffersForSelectListWithUserAsync(User), "Id", "Name");
                return View(appointment);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointment' is null.");
            }
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var appointment = await _service.GetAppointmentAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }
                ViewData["JobOfferId"] = new SelectList(await _service.GetJobOffersForSelectListWithUserAsync(User), "Id", "Name");
                return View(appointment);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointment' is null.");
            }
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Time,Duration,IsOnline,CompanyId,JobOfferId")] Appointment appointment)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            if (id != appointment.Id)
            {
                return NotFound();
            }
            try
            {
                ModelState.Remove("Company");
                if (ModelState.IsValid)
                {
                    if (!await _service.DoesAppointmentExistAsync(id))
                    {
                        return NotFound();
                    }
                    await _service.UpdateAppointmentAsync(appointment);
                    return RedirectToAction(nameof(Index));
                }
                ViewData["JobOfferId"] = new SelectList(await _service.GetJobOffersForSelectListWithUserAsync(User), "Id", "Name");
                return View(appointment);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointment' is null.");
            }
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var appointment = await _service.GetAppointmentAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }
                return View(appointment);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointment' is null.");
            }
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
            try
            {
                await _service.DeleteAppointmentAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointment' is null.");
            }
        }

        public async Task<IActionResult> Select(int id) // solicitation id
        {
            try
            {
                if (!await _service.DoesSolicitationExistsAsync(id))
                {
                    return NotFound();
                }
                var appointments = await _service.GetAvailableAppointmentsAsync(id);
                if (appointments == null)
                {
                    return NotFound();
                }
                ViewData["SolicitationId"] = id;
                return View(appointments);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointment' is null.");
            }
        }

        [HttpPost, ActionName("Select")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectConfirmed(int appointmentId, int solicitationId)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                if (!(await _service.DoesSolicitationExistsAsync(solicitationId) || await _service.DoesAppointmentExistAsync(appointmentId)))
                {
                    return RedirectToAction(nameof(Select), solicitationId);
                }
                await _service.SelectAppointmentAsync(appointmentId, solicitationId);
                return RedirectToAction(nameof(Index), "Solicitations");
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.Appointment' is null.");
            }
        }

        public async Task<IActionResult> DetailsRepeatAppointment(int id)
        {
            try
            {
                var repeatAppointment = await _service.GetRepeatAppointmentAsync(id);
                if (repeatAppointment == null)
                {
                    return NotFound();
                }
                return View(repeatAppointment);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.RepeatAppointment' is null.");
            }
        }

        // GET: Appointments/CreateRepeatAppointment
        public async Task<IActionResult> CreateRepeatAppointment()
        {
            if (!User.IsInRole("ROLE_EMPLOYER"))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var repeatAppointment = new RepeatAppointment();
                var company = await _service.GetCompanyAsync(User);
                if (company == null)
                {
                    return NotFound();
                }
                repeatAppointment.Company = company;
                repeatAppointment.CompanyId = company.Id;
                return View(repeatAppointment);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.RepeatAppointment' is null.");
            }
        }

        // POST: Appointments/CreateRepeatAppointment
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRepeatAppointment([Bind("Id,CompanyId,Repeats, RepeatsVar,Time,Duration")] RepeatAppointment repeatAppointment)
        {
            if (!User.IsInRole("ROLE_EMPLOYER"))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                ModelState.Remove("Company");
                if (ModelState.IsValid)
                {
                    repeatAppointment = await _service.CreateRepeatAppointmentAsync(repeatAppointment, User);
                    return RedirectToAction(nameof(Index));
                }
                return View(repeatAppointment);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.RepeatAppointment' is null.");
            }
        }

        public async Task<IActionResult> SetRepeatAppointmentRepeatInfoAsync(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var repeatAppointment = await _service.GetRepeatAppointmentAsync(id);
                if (repeatAppointment == null)
                {
                    return NotFound();
                }
                return View(repeatAppointment);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.RepeatAppointment' is null.");
            }
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> EditRepeatAppointment(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var repeatAppointment = await _service.GetRepeatAppointmentAsync(id);
                if (repeatAppointment == null)
                {
                    return NotFound();
                }
                return View(repeatAppointment);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.RepeatAppointment' is null.");
            }
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRepeatAppointment(int id, [Bind("Id,Date,Time,Duration,IsOnline,CompanyId,JobOfferId")] RepeatAppointment repeatAppointment)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            if (id != repeatAppointment.Id)
            {
                return NotFound();
            }
            try
            {
                ModelState.Remove("Company");
                if (ModelState.IsValid)
                {
                    if (!await _service.DoesRepeatAppointmentExistAsync(id))
                    {
                        return NotFound();
                    }
                    await _service.UpdateRepeatAppointmentAsync(repeatAppointment);
                    return RedirectToAction(nameof(Index));
                }
                return View(repeatAppointment);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.RepeatAppointment' is null.");
            }
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> DeleteRepeatAppointment(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var repeatAppointment = await _service.GetRepeatAppointmentAsync(id);
                if (repeatAppointment == null)
                {
                    return NotFound();
                }
                return View(repeatAppointment);
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.RepeatAppointment' is null.");
            }
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("DeleteRepeatAppointment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedRepeatAppointment(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                await _service.DeleteRepeatAppointmentAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (InternalServerException)
            {
                return Problem("Entity set 'ApplicationDbContext.RepeatAppointment' is null.");
            }
        }
    }
}
