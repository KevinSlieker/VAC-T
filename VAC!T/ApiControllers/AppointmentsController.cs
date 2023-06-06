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
        /// <summary>
        /// Gets all appointments the logged in user is allowed to view.
        /// </summary>
        /// <returns>All appointments available</returns>
        /// <remarks>
        /// Employers can only see appointments that are connected to their company.
        /// </remarks>
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
        /// <summary>
        /// Gets one appointment by id.
        /// </summary>
        /// <param name="id">The id of the appointment</param>
        /// <returns>one appointment</returns>
        /// <remarks>
        /// This funcetion returns 1 appointment of the matching id if it exists.
        /// </remarks>
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDTO>> GetAppointmentByIdAsync(int id)
        {
            try
            {
                var appointment = await _service.GetAppointmentAsync(id, User);
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
        /// <summary>
        /// Creates an appointment.
        /// </summary>
        /// <param name="appointment">The information of the to be created appointment.</param>
        /// <returns>The created appointment</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/Appointments
        ///     {
        ///         "date": "2023-03-28T00:00:00",
        ///         "time": "2023-03-22T00:00:00",
        ///         "duration": "00:30:00",
        ///         "isOnline": true,
        ///         "jobOfferId": null
        ///     }
        ///     
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] AppointmentDTOForCreate appointment)
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
        /// <summary>
        /// Updates an appointment.
        /// </summary>
        /// <param name="id">The id of the appointment</param>
        /// <param name="appointment">The information of the to be updated appointment.</param>
        /// <returns>No content</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/Appointments/4
        ///     {
        ///         "id": 4,
        ///         "date": "2023-03-28T00:00:00",
        ///         "time": "2023-03-22T00:00:00",
        ///         "duration": "00:30:00",
        ///         "isOnline": true,
        ///         "jobOfferId": null
        ///     }
        ///     
        /// </remarks>
        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(int id, [FromBody] AppointmentDTOForCreate appointment)
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
                var appointmentEntity = await _service.GetAppointmentAsync(id, User);
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
        /// <summary>
        /// Deletes an appointment.
        /// </summary>
        /// <param name="id">The id of the appointment</param>
        /// <returns>Ok</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAppointmentAsync(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                if (!await _service.DoesAppointmentExistAsync(id, User))
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

        [HttpGet("Available/{id}")]
        public async Task<ActionResult<IEnumerable<AppointmentDTOAvailable>>> GetAvailableAppointmentsAsync(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                if (!await _service.DoesSolicitationExistAsync(id, User))
                {
                    return NotFound("Solicitation does not exist or you are not allowed to view data related to this solicitation");
                };
                var appointments = await _service.GetAvailableAppointmentsAsync(id);
                var result = _mapper.Map<List<AppointmentDTOAvailable>>(appointments);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // PUT: api/Appointments/14-4-2023 14:00:00_6/41
        /// <summary>
        /// Selects an appointment for the job interview.
        /// </summary>
        /// <param name="selectedAppointmentId">The id of the selected appointment.</param>
        /// <param name="solicitationId">The id of the solicitation the appointment will be connected to.</param>
        /// <returns>Ok</returns>
        /// <remarks>
        /// For selectedAppointmentId: If it is an repear appointment you put the DateTime infront of the id and connect it with: "_". So it will be like
        /// 14-4-2023 14:00:00_6. For nomal appointments you can just use the id.
        /// </remarks>
        [HttpPut("{selectedAppointmentId}/{solicitationId}")]
        public async Task<ActionResult> PutSelectAppointmentAsync(string selectedAppointmentId, int solicitationId) // example: 14-4-2023 14:00:00_6 or 27 for selectedAppointmentId
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_CANDIDATE")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                if (selectedAppointmentId.Contains("_"))
                {
                    var split = selectedAppointmentId.Split('_');
                    var repeatAppointmentId = Int32.Parse(split.LastOrDefault()!);
                    var date = DateTime.Parse(split.FirstOrDefault()!);
                    if (!await _service.DoesSolicitationExistAsync(solicitationId, User))
                    {
                        return NotFound("solicitationId does not exist.");
                    }
                    await _service.SelectRepeatAppointmentAsync(repeatAppointmentId, date, solicitationId);
                }
                else
                {
                    var appointmentId = Int32.Parse(selectedAppointmentId);
                    if (!(await _service.DoesSolicitationExistAsync(solicitationId, User) || await _service.DoesAppointmentExistAsync(appointmentId, User)))
                    {
                        return NotFound("AppointId or solicitationId does not exist.");
                    }
                    await _service.SelectAppointmentAsync(appointmentId, solicitationId);
                }
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // Get: api/Appointments/Repeat
        /// <summary>
        /// Gets all repeatAppointments the logged in user is allowed to view.
        /// </summary>
        /// <returns>All repeatAppointments available</returns>
        /// <remarks>
        /// Employers can only see repeatAppointments that are connected to their company.
        /// </remarks>
        [HttpGet("Repeat")]
        public async Task<ActionResult<RepeatAppointmentDTO>> GetAllRepeatAppointmentsAsync()
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var repeatAppointments = await _service.GetRepeatAppointmentsAsync(User);
                var result = _mapper.Map<List<RepeatAppointmentDTO>>(repeatAppointments);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // GET: api/Appointments/Repeat/5
        /// <summary>
        /// Gets one repeatAppointment by id.
        /// </summary>
        /// <returns>one repeatAppointment</returns>
        [HttpGet("Repeat/{id}")]
        public async Task<ActionResult<RepeatAppointmentDTO>> GetRepeatAppointmentByIdAsync(int id)
        {
            try
            {
                var repeatAppointment = await _service.GetRepeatAppointmentAsync(id, User);
                if (repeatAppointment == null)
                {
                    return NotFound();
                }
                var result = _mapper.Map<RepeatAppointmentDTO>(repeatAppointment);
                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // POST: api/Appointments/Repeat
        /// <summary>
        /// Creates a repeatAppointment
        /// </summary>
        /// <param name="repeatAppointment">The information of the to be created repeatAppointment</param>
        /// <returns>The created repeatAppointment</returns>
        /// <remarks>
        /// The repeatAppointment is created under the logged in user.
        /// The details about the repeatAppointment are made in the PUT function for Update Info repeatAppointment
        /// 
        /// For repeats: 1 = daily, 2 = weekly, 3 = monthly and 4 = monthlyRelative.
        /// 
        /// 
        /// Sample request:
        /// 
        ///     POST /api/Appointments/Repeat
        ///     {
        ///         "repeats": 2,
        ///         "time": "2023-03-22T21:00:00",
        ///         "duration": "00:30:00",
        ///         "isOnline": true
        ///     }
        /// </remarks>
        [HttpPost("Repeat")]
        public async Task<ActionResult> PostRepeatAppointmentAsync([FromBody] RepeatAppointmentDTOForCreate repeatAppointment)
        {
            if (!User.IsInRole("ROLE_EMPLOYER"))
            {
                return Unauthorized("Unauthorized");
            }
            try
            {
                var repeatAppointmentEntity = _mapper.Map<RepeatAppointment>(repeatAppointment);

                repeatAppointmentEntity = await _service.CreateRepeatAppointmentAsync(repeatAppointmentEntity, User);

                var newRepeatAppointment = _mapper.Map<RepeatAppointmentDTO>(repeatAppointmentEntity);
                int id = repeatAppointmentEntity.Id;
                return CreatedAtAction(nameof(GetRepeatAppointmentByIdAsync), new { id }, newRepeatAppointment);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // PUT: api/Appointments/Repeat/4
        /// <summary>
        /// Updates a repeatAppointment
        /// </summary>
        /// <param name="id">The id of the repeatAppointment</param>
        /// <param name="repeatAppointment">The information of the to be updated repeatAppointment</param>
        /// <returns>No content</returns>
        /// <remarks>
        /// The details about the repeatAppointment are made in the PUT function for Update Info repeatAppointment
        /// 
        /// For repeats: 1 = daily, 2 = weekly, 3 = monthly and 4 = monthlyRelative.
        /// 
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/Appointments/Repeat/4
        ///     {
        ///         "id": 4,
        ///         "repeats": 2,
        ///         "time": "2023-03-22T21:00:00",
        ///         "duration": "00:30:00",
        ///         "isOnline": true
        ///     }
        /// </remarks>
        [HttpPut("Repeat/{id}")]
        public async Task<ActionResult> PutRepeatAppointmentAsync(int id, [FromBody] RepeatAppointmentDTOForCreate repeatAppointment)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            if (id != repeatAppointment.Id)
            {
                ModelState.AddModelError("Id", "Does not match Id in URL");
                return BadRequest(ModelState);
            }
            try
            {
                var repeatAppointmentEntity = await _service.GetRepeatAppointmentAsync(id, User);
                if (repeatAppointmentEntity == null)
                {
                    return NotFound($"No repeatAppointment with Id: {id} in the database");
                }

                _mapper.Map(repeatAppointment, repeatAppointmentEntity);
                await _service.UpdateRepeatAppointmentAsync(repeatAppointmentEntity);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
            return NoContent();
        }

        // PUT: api/Appointments/Repeat/Info/4
        /// <summary>
        /// Creates/Updates the repeatAppointment info(details about occurense)
        /// </summary>
        /// <param name="id">The id of the repeatAppointment</param>
        /// <param name="repeatAppointment">The information of the to be created/updated repeatAppointment info</param>
        /// <returns>No content</returns>
        /// <remarks>
        /// The details about the repeatAppointment are made in the PUT function for Update Info repeatAppointment.
        /// 
        /// Keep repeats the same here.
        /// 
        /// Repeatsday: is for monthlyRelative(repeats: 4). This is the day of the month(example: 15). This is an integer.
        ///  
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/Appointments/Repeat/Info/4
        ///     {
        ///         "id": 4,
        ///         "repeats": 2,
        ///         "repeatsday": null,
        ///         "ismonday": true,
        ///         "istuesday": false,
        ///         "iswednesday": false,
        ///         "isthursday": false,
        ///         "isfriday": true,
        ///         "isfirst": false,
        ///         "issecond": false,
        ///         "isthird": false,
        ///         "isfourth": false,
        ///         "islast": false
        ///     }
        /// </remarks>
        [HttpPut("Repeat/Info/{id}")]
        public async Task<ActionResult> PutRepeatAppointmentInfoAsync(int id, [FromBody] RepeatAppointmentEnumViewModel repeatAppointment)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            if (id != repeatAppointment.Id)
            {
                ModelState.AddModelError("Id", "Does not match Id in URL");
                return BadRequest(ModelState);
            }
            try
            {
                var repeatAppointmentEntity = await _service.GetRepeatAppointmentAsync(id, User);
                if (repeatAppointmentEntity == null)
                {
                    return NotFound($"No repeatAppointment with Id: {id} in the database");
                }

                _mapper.Map(repeatAppointment, repeatAppointmentEntity);
                await _service.UpdateRepeatAppointmentAsync(repeatAppointmentEntity);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
            return NoContent();
        }

        // DELETE: api/Appointments/Repeat/5
        /// <summary>
        /// Deletes an repeatAppointment.
        /// </summary>
        /// <param name="id">The id of the repeatAppointment</param>
        /// <returns>Ok</returns>
        [HttpDelete("Repeat/{id}")]
        public async Task<ActionResult> DeleteRepeatAppointmentAsync(int id)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                if (!await _service.DoesRepeatAppointmentExistAsync(id, User))
                {
                    return NotFound($"No repeatAppointment with Id: {id} in the database");
                }

                await _service.DeleteRepeatAppointmentAsync(id);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }
    }
}