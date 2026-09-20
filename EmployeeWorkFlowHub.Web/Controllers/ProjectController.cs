using Microsoft.AspNetCore.Mvc;

namespace EmployeeWorkFlowHub.Controllers
{
    /// <summary>
    /// MVC Controller serving the presentation views for Project management.
    /// </summary>
    public class ProjectController : Controller
    {
        /// <summary>
        /// Default route redirecting to the project portfolio view.
        /// </summary>
        /// <returns>Redirect to ViewProject action.</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(ViewProject));
        }

        /// <summary>
        /// Renders the project portfolio view containing the project table and status overview.
        /// </summary>
        /// <returns>ViewProject.cshtml Razor view shell.</returns>
        [HttpGet]
        public IActionResult ViewProject()
        {
            return View();
        }

        /// <summary>
        /// Renders the project creation form view. Accessible only to Managers.
        /// </summary>
        /// <returns>AddProject.cshtml Razor view shell.</returns>
        [HttpGet]
        public IActionResult AddProject()
        {
            return View();
        }
    }
}
