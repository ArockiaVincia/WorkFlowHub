using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeWorkFlowHub.Common;
using EmployeeWorkFlowHub.Models;
using EmployeeWorkFlowHub.Service.Interfaces;

namespace EmployeeWorkFlowHub.Controllers.Api
{
    /// <summary>
    /// RESTful Web API Controller for Employee management.
    /// Injects IEmployeeService via dependency injection.
    /// </summary>
    [Produces(AuthAPIController.InputType.ApplicationJson)]
    [ApiController]
    [Route("api/employee")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EmployeeApiController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeApiController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Employee>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _employeeService.GetAllAsync();
            if (response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, new { message = response.StatusMessage });
            }
            return Ok(response.ResultData);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Employee))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _employeeService.GetByIdAsync(id);
            if (response.StatusCode == 404)
            {
                return NotFound(new { message = response.StatusMessage });
            }
            if (response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, new { message = response.StatusMessage });
            }
            return Ok(response.ResultData);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Employee))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] EmployeeDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var employee = new Employee
            {
                EmployeeCode = dto.EmployeeCode,
                FullName = dto.FullName,
                Email = dto.Email,
                DepartmentId = dto.DepartmentId,
                Designation = dto.Designation,
                IsActive = dto.IsActive,
                Role = dto.Role,
                Password = dto.Password,
                Username = dto.Username
            };

            var response = await _employeeService.CreateAsync(employee);
            if (response.StatusCode == 400)
            {
                return BadRequest(new { message = response.StatusMessage });
            }
            if (response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, new { message = response.StatusMessage });
            }

            var created = (Employee)response.ResultData!;
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Employee))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var employee = new Employee
            {
                Id = id,
                EmployeeCode = dto.EmployeeCode,
                FullName = dto.FullName,
                Email = dto.Email,
                DepartmentId = dto.DepartmentId,
                Designation = dto.Designation,
                IsActive = dto.IsActive,
                Role = dto.Role,
                Password = dto.Password,
                Username = dto.Username
            };

            var response = await _employeeService.UpdateAsync(employee);
            if (response.StatusCode == 404)
            {
                return NotFound(new { message = response.StatusMessage });
            }
            if (response.StatusCode == 400)
            {
                return BadRequest(new { message = response.StatusMessage });
            }
            if (response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, new { message = response.StatusMessage });
            }

            return Ok(response.ResultData);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _employeeService.DeleteAsync(id);
            if (response.StatusCode == 404)
            {
                return NotFound(new { message = response.StatusMessage });
            }
            if (response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, new { message = response.StatusMessage });
            }

            return NoContent();
        }
    }
}
