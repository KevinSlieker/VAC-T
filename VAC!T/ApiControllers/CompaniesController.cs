using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VAC_T.Models;
using VAC_T.Data.DTO;
using VAC_T.Business;
using VAC_T.DAL.Exceptions;

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
                var result = _mapper.Map<CompanyDTO>(companies);
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
                // Include(c => c.JobOffers) gives an error. It sais it's a loop and postman can process it.
                var company = await _service.GetCompanyAsync(id);                
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

        //public async Task<IActionResult> DetailsForEmployer()
        //{
        //    var user = await _userManager.GetUserAsync(User);

        //    if (_context.Company == null)
        //    {
        //        return NotFound();
        //    }

        //    var id = _context.Company.Include(x => x.User).Where(x => x.User == user).FirstOrDefault().Id;
        //    if (id == 0)
        //    {
        //        return NotFound();
        //    }

        //    var company = await _context.Company.Include(c => c.JobOffers)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (company == null)
        //    {
        //        return NotFound();
        //    }

        //    return View("Details", company);
        //}

        // GET: Companies/Create
        //public IActionResult Create()
        //{
        //    var company = new Company();
        //    company.LogoURL = "assets/img/company/default.png";
        //    return View(company);
        //}

        // POST: Companies/Create
        [HttpPost]
        public async Task<ActionResult> CreateAsync([FromBody] CompanyDTO company)
        {
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

        // GET: Companies/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _context.Company == null)
        //    {
        //        return NotFound();
        //    }

        //    var company = await _context.Company.FindAsync(id);
        //    if (company == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(company);
        //}

        // POST: Companies/Edit/5

        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(int id, [FromBody] CompanyDTOForUpdate company)
        {
            if (id != company.Id)
            {
                ModelState.AddModelError("Id", "Does not match Id in URL");
                return BadRequest(ModelState);
            }
            try { 
                var companyEntity = await _service.GetCompanyAsync(id);
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
                
        // POST: Companies/Delete/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            try
            {
                if (!await _service.DoesCompanyExistsAsync(id))
                {
                    return NotFound();
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
