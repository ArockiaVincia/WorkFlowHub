using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeWorkFlowHub.Common;
using EmployeeWorkFlowHub.Models;
using EmployeeWorkFlowHub.Service.Interfaces;

namespace EmployeeWorkFlowHub.Controllers.Api
{
    /// <summary>
    /// RESTful Web API Controller for Department operations.
    /// Injects IDepartmentService via dependency injection.
    /// </summary>
    [Produces(AuthAPIController.InputType.ApplicationJson)]
    [ApiController]
    [Route("api/department")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DepartmentApiController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentApiController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Department>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _departmentService.GetAllAsync();
            if (response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, new { message = response.StatusMessage });
            }
            return Ok(response.ResultData);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Department))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _departmentService.GetByIdAsync(id);
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
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(DepartmentResponseDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] DepartmentDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var department = new Department { Name = dto.Name };
            var response = await _departmentService.CreateAsync(department);
            if (response.StatusCode == 400)
            {
                return BadRequest(new { message = response.StatusMessage });
            }
            if (response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, new { message = response.StatusMessage });
            }

            var created = (Department)response.ResultData!;
            var responseDto = new DepartmentResponseDTO
            {
                Id = created.Id,
                Name = created.Name
            };
            return CreatedAtAction(nameof(GetById), new { id = responseDto.Id }, responseDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DepartmentResponseDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var department = new Department { Id = id, Name = dto.Name };
            var response = await _departmentService.UpdateAsync(department);
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

            var updated = (Department)response.ResultData!;
            var responseDto = new DepartmentResponseDTO
            {
                Id = updated.Id,
                Name = updated.Name
            };
            return Ok(responseDto);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _departmentService.DeleteAsync(id);
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
