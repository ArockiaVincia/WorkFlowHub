using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeWorkFlowHub.Common;
using EmployeeWorkFlowHub.Models;
using EmployeeWorkFlowHub.Service.Interfaces;

namespace EmployeeWorkFlowHub.Controllers.Api
{
    /// <summary>
    /// RESTful Web API Controller for dynamic support data / master lookups.
    /// Injects ILookupService via dependency injection.
    /// </summary>
    [Produces(AuthAPIController.InputType.ApplicationJson)]
    [ApiController]
    [Route("api/supportdata")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SupportDataApiController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public SupportDataApiController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        /// <summary>
        /// Retrieves all active support data items across all categories.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<SupportDataItem>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _lookupService.GetAllAsync();
            if (response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, new { message = response.StatusMessage });
            }
            return Ok(response.ResultData);
        }

        /// <summary>
        /// Retrieves active support data items belonging to a specific category.
        /// </summary>
        [HttpGet("{type}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<SupportDataItem>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByType(string type)
        {
            var response = await _lookupService.GetByTypeAsync(type);
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
    }
}
