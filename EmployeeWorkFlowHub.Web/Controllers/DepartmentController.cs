using Microsoft.AspNetCore.Mvc;

namespace EmployeeWorkFlowHub.Controllers
{
    /// <summary>
    /// MVC Controller serving the presentation views for Department management.
    /// </summary>
    public class DepartmentController : Controller
    {
        /// <summary>
        /// Default route redirecting to the primary department view page.
        /// </summary>
        /// <returns>Redirect to ViewDepartment action.</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(ViewDepartment));
        }

        /// <summary>
        /// Renders the primary department listing and management grid page (viewDepartment).
        /// </summary>
        /// <returns>ViewDepartment.cshtml Razor view shell.</returns>
        [HttpGet]
        public IActionResult ViewDepartment()
        {
            return View();
        }

        /// <summary>
        /// Renders the dedicated Add Department form page (AddDepart).
        /// </summary>
        /// <returns>AddDepart.cshtml Razor view shell.</returns>
        [HttpGet]
        public IActionResult AddDepart()
        {
            return View();
        }
    }
}
