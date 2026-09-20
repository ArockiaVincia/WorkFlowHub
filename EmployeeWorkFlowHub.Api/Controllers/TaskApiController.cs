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
    /// RESTful Web API Controller for Task management operations.
    /// Injects ITaskService via dependency injection.
    /// </summary>
    [Produces(AuthAPIController.InputType.ApplicationJson)]
    [ApiController]
    [Route("api/task")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TaskApiController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskApiController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private string CurrentUserRole => User.FindFirst(ClaimTypes.Role)?.Value 
                                       ?? HttpContext.Session.GetString(CommonVariable.SessionField.Role) 
                                       ?? string.Empty;

        private int CurrentEmployeeId => (int.TryParse(User.FindFirst(CommonVariable.SessionField.EmployeeId)?.Value, out int id) && id > 0) 
                                       ? id 
                                       : (HttpContext.Session.GetInt32(CommonVariable.SessionField.EmployeeId) ?? 0);

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TaskItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _taskService.GetAllAsync(CurrentUserRole, CurrentEmployeeId);
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
        [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _taskService.GetByIdAsync(id, CurrentUserRole, CurrentEmployeeId);
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
        [ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] TaskDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                ProjectId = dto.ProjectId,
                EmployeeId = dto.EmployeeId,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                Status = dto.Status
            };

            var response = await _taskService.CreateAsync(task, CurrentUserRole, CurrentEmployeeId);
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

            var created = (TaskItem)response.ResultData!;
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] TaskDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var task = new TaskItem
            {
                Id = id,
                Title = dto.Title,
                Description = dto.Description,
                ProjectId = dto.ProjectId,
                EmployeeId = dto.EmployeeId,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                Status = dto.Status
            };

            var response = await _taskService.UpdateAsync(task, CurrentUserRole, CurrentEmployeeId);
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

        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Status))
            {
                return BadRequest(new { message = "Status value is required." });
            }

            var response = await _taskService.UpdateStatusAsync(id, request.Status, CurrentUserRole, CurrentEmployeeId);
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

            return Ok(new { message = "Task status updated successfully." });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _taskService.DeleteAsync(id, CurrentUserRole);
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
