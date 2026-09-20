using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeWorkFlowHub.Common;
using EmployeeWorkFlowHub.Models;
using EmployeeWorkFlowHub.Service.Interfaces;

namespace EmployeeWorkFlowHub.Controllers.Api
{
    /// <summary>
    /// RESTful Web API Controller for Project operations.
    /// Injects IProjectService via dependency injection.
    /// </summary>
    [Produces(AuthAPIController.InputType.ApplicationJson)]
    [ApiController]
    [Route("api/project")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProjectApiController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectApiController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        private string CurrentUserRole => User.FindFirst(ClaimTypes.Role)?.Value 
                                       ?? HttpContext.Session.GetString(CommonVariable.SessionField.Role) 
                                       ?? string.Empty;

        private int CurrentEmployeeId => (int.TryParse(User.FindFirst(CommonVariable.SessionField.EmployeeId)?.Value, out int id) && id > 0) 
                                       ? id 
                                       : (HttpContext.Session.GetInt32(CommonVariable.SessionField.EmployeeId) ?? 0);

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Project>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _projectService.GetAllAsync(CurrentUserRole, CurrentEmployeeId);
            if (response.StatusCode == 403)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = response.StatusMessage });
            }
            if (response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, new { message = response.StatusMessage });
            }
            return Ok(response.ResultData);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Project))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _projectService.GetByIdAsync(id, CurrentUserRole, CurrentEmployeeId);
            if (response.StatusCode == 403)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = response.StatusMessage });
            }
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
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Project))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] ProjectDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var project = new Project
            {
                Name = dto.Name,
                DepartmentId = dto.DepartmentId,
                ProjectManagerId = dto.ProjectManagerId,
                TeamMembers = dto.TeamMembers,
                Status = dto.Status,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            var response = await _projectService.CreateAsync(project, CurrentUserRole);
            if (response.StatusCode == 403)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = response.StatusMessage });
            }
            if (response.StatusCode == 400)
            {
                return BadRequest(new { message = response.StatusMessage });
            }
            if (response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, new { message = response.StatusMessage });
            }

            var created = (Project)response.ResultData!;
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Project))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] ProjectDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var project = new Project
            {
                Id = id,
                Name = dto.Name,
                DepartmentId = dto.DepartmentId,
                ProjectManagerId = dto.ProjectManagerId,
                TeamMembers = dto.TeamMembers,
                Status = dto.Status,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            var response = await _projectService.UpdateAsync(project, CurrentUserRole, CurrentEmployeeId);
            if (response.StatusCode == 403)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = response.StatusMessage });
            }
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _projectService.DeleteAsync(id, CurrentUserRole);
            if (response.StatusCode == 403)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = response.StatusMessage });
            }
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
