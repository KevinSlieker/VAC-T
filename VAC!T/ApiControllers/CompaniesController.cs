using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VAC_T.Models;
using VAC_T.Data.DTO;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace VAC_T.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly CompanyService _service;

        public CompaniesController(CompanyService service, IMapper mapper)
        {
            _mapper = mapper;
            _service = service;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyDTO>>> GetAllCompaniesAsync([FromQuery] string? searchName)
        {
            try
            {
                IEnumerable<Company> companies = await _service.GetCompaniesAsync(searchName);
                var result = _mapper.Map<List<CompanyDTO>>(companies);
                return Ok(result);
            } 
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDTO>> GetCompanyByIdAsync(int id)
        {
            try
            {
                var company = await _service.GetCompanyAsync(id, User);                
                if (company == null)
                {
                    return NotFound();
                }
                var result = _mapper.Map<CompanyDTO>(company);

                return Ok(result);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // POST: api/Companies
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> PostAsync([FromBody] CompanyDTO company)
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                var companyEntity = _mapper.Map<Company>(company); 

                companyEntity = await _service.CreateCompanyWithUserAsync(companyEntity);

                 var newCompany = _mapper.Map<CompanyDTO>(companyEntity);
                int id = companyEntity.Id;
                return CreatedAtAction(nameof(GetCompanyByIdAsync), new { id }, newCompany);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }

        // Put: api/Companies/5

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> PutAsync(int id, [FromBody] CompanyDTOForUpdate company)
        {
            if (!(User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_EMPLOYER")))
            {
                return Unauthorized("Not the correct roles.");
            }

            if (id != company.Id)
            {
                ModelState.AddModelError("Id", "Does not match Id in URL");
                return BadRequest(ModelState);
            }
            try { 
                var companyEntity = await _service.GetCompanyAsync(id, User);
                if (companyEntity == null)
                {
                    return NotFound($"No company with Id: {id} in the database");
                }

                _mapper.Map(company, companyEntity);

                await _service.UpdateCompanyAsync(companyEntity);
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
            return NoContent();
        }


        // Delete: api/Companies/5
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> DeleteCompanyAsync(int id)
        {
            if (!User.IsInRole("ROLE_ADMIN"))
            {
                return Unauthorized("Not the correct roles.");
            }
            try
            {
                if (!await _service.DoesCompanyExistsAsync(id))
                {
                    return NotFound($"No company with Id: {id} in the database");
                }

                await _service.DeleteCompanyAsync(id);
                return Ok();
            }
            catch (InternalServerException)
            {
                return Problem("Database not connected");
            }
        }
    }
}
