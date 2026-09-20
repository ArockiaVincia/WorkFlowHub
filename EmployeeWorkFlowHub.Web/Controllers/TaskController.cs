using Microsoft.AspNetCore.Mvc;

namespace EmployeeWorkFlowHub.Controllers
{
    /// <summary>
    /// MVC Controller serving the presentation views for Task management.
    /// </summary>
    public class TaskController : Controller
    {
        /// <summary>
        /// Default route redirecting to the task workflow board view.
        /// </summary>
        /// <returns>Redirect to ViewTask action.</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(ViewTask));
        }

        /// <summary>
        /// Renders the interactive Kanban workflow board and task table view.
        /// </summary>
        /// <returns>ViewTask.cshtml Razor view shell.</returns>
        [HttpGet]
        public IActionResult ViewTask()
        {
            return View();
        }

        /// <summary>
        /// Renders the task creation page. Accessible to Managers and Team Leads.
        /// </summary>
        /// <returns>AddTask.cshtml Razor view shell.</returns>
        [HttpGet]
        public IActionResult AddTask()
        {
            return View();
        }
    }
}
