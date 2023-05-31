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
        /// <summary>
        /// Get a list of companies (optional) that match a search string
        /// </summary>
        /// <param name="searchName">(optional) string to match the name of the companies to </param>
        /// <returns>a list of entries or null </returns>
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
        /// <summary>
        /// Get a company by id.
        /// </summary>
        /// <param name="id">The id of the company to find</param>
        /// <returns>a company</returns>
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
        /// <summary>
        /// Create a company and an user for that company
        /// </summary>
        /// <param name="company">the details of the company to create</param>
        /// <returns>the created company</returns>
        /// <remarks>
        /// The LogoURL needs to be put as default. You can change this/upload your own logo later.
        /// 
        /// 
        /// Sample request:
        /// 
        ///     POST /api/Companies
        ///     {
        ///         "name": "Test Api2",
        ///         "description": "Test",
        ///         "logoURL": "assets\\img\\company\\default.png",
        ///         "websiteURL": "http://test.nl",
        ///         "address": "TEST",
        ///         "postcode": "1234TE",
        ///         "residence": "Test"
        ///     }
        ///     
        /// </remarks>
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
        /// <summary>
        /// Update a company of the given id
        /// </summary>
        /// <param name="id">The id of the company to update</param>
        /// <param name="company">the details of the company to update</param>
        /// <returns>No content</returns>
        /// <remarks>
        /// The LogoURL needs to be put the same. You can change this/upload your own logo later.
        /// 
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/Companies/5
        ///     {
        ///         "id": 5,
        ///         "name": "Test Api Update",
        ///         "description": "Test Update",
        ///         "logoURL": "assets\\img\\company\\default.png",
        ///         "websiteURL": "http://test.nl",
        ///         "address": "TEST",
        ///         "postcode": "1234TE",
        ///         "residence": "Test"
        ///     }
        ///     
        /// </remarks>
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
        /// <summary>
        /// Delete a company and connected user by the given id.
        /// </summary>
        /// <param name="id">the id of the company to delete</param>
        /// <returns>Ok</returns>
        /// <remarks>
        /// When deleting a company all other data connected to the company will also be deleted. This includes the employer of the company.
        /// </remarks>
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
