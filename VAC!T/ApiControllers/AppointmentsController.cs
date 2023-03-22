using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using VAC_T.Data.DTO;
using VAC_T.Models;

namespace VAC_T.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AppointmentsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly AppointmentService _service;

        public AppointmentsController(IMapper mapper, AppointmentService service)
        {
            _mapper = mapper;
            _service = service;
        }

        // Get: api/Appointments
        [HttpGet]
        public async Task<ActionResult<AppointmentDTO>> GetAllAppointmentsAsync()
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var appointments = await _service.GetAppointmentsAsync(User);
                var result = _mapper.Map<List<AppointmentDTO>>(appointments);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // GET: api/Appointments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDTO>> GetAppointmentByIdAsync(int id)
        {
            try
            {
                var appointment = await _service.GetAppointmentAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }
                var result = _mapper.Map<AppointmentDTO>(appointment);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // POST: api/Appointments
        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody]AppointmentDTOForCreate appointment)
        {
            if (!User.IsInRole("ROLE_EMPLOYER"))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var appointmentEntity = _mapper.Map<Appointment>(appointment);

                appointmentEntity = await _service.CreateAppointmentAsync(appointmentEntity, User);

                var newAppointment = _mapper.Map<AppointmentDTO>(appointmentEntity);
                int id = appointmentEntity.Id;
                return CreatedAtAction(nameof(GetAppointmentByIdAsync), new { id }, newAppointment);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // PUT: api/Appointments/4
        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(int id, [FromBody]AppointmentDTOForCreate appointment)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            if (id != appointment.Id)
            {
                ModelState.AddModelError("Id", "Does not match Id in URL");
                return BadRequest(ModelState);
            }
            try
            {
                var appointmentEntity = await _service.GetAppointmentAsync(id);
                if (appointmentEntity == null)
                {
                    return NotFound($"No appointment with Id: {id} in the database");
                }
                
                _mapper.Map(appointment, appointmentEntity);
                await _service.UpdateAppointmentAsync(appointmentEntity);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
            return NoContent();
        }

        // DELETE: api/Appointments/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAppointmentAsync(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                if (!await _service.DoesAppointmentExistAsync(id))
                {
                    return NotFound($"No appointment with Id: {id} in the database");
                }

                await _service.DeleteAppointmentAsync(id);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // PUT: api/Appointments/5/7
        [HttpPut("{appointmentId}/{solicitationId}")]
        public async Task<ActionResult> PutSelectAppointmentAsync(int appointmentId, int solicitationId)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                if (!(await _service.DoesSolicitationExistsAsync(solicitationId) || await _service.DoesAppointmentExistAsync(appointmentId)))
                {
                    return NotFound("AppointId or solicitationId does not exist.");
                }

                await _service.SelectAppointmentAsync(appointmentId, solicitationId);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }
    }
}