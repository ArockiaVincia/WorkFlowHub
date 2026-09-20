using Microsoft.AspNetCore.Mvc;

namespace EmployeeWorkFlowHub.Controllers
{
    /// <summary>
    /// MVC Controller serving the presentation views for Employee management.
    /// </summary>
    public class EmployeeController : Controller
    {
        /// <summary>
        /// Default route redirecting to the employee directory view.
        /// </summary>
        /// <returns>Redirect to ViewEmployee action.</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(ViewEmployee));
        }

        /// <summary>
        /// Renders the employee list view with data tables, search filters, and action triggers.
        /// </summary>
        /// <returns>ViewEmployee.cshtml Razor view shell.</returns>
        [HttpGet]
        public IActionResult ViewEmployee()
        {
            return View();
        }

        /// <summary>
        /// Renders the employee registration form for onboarding new employees.
        /// </summary>
        /// <returns>AddEmployee.cshtml Razor view shell.</returns>
        [HttpGet]
        public IActionResult AddEmployee()
        {
            return View();
        }
    }
}
