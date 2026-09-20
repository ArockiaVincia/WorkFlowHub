using Microsoft.AspNetCore.Mvc;

namespace EmployeeWorkFlowHub.Controllers
{
    /// <summary>
    /// MVC presentation controller for authentication views (Login, Logout).
    /// Authentication state and token storage are managed client-side via JavaScript and Fetch API.
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Renders the user login page. If already authenticated in session, redirects to dashboard.
        /// </summary>
        /// <returns>Login.cshtml Razor view shell, or redirect to home dashboard if already logged in.</returns>
        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Token")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        /// <summary>
        /// Handles logout by clearing server-side session and redirecting to the login view.
        /// </summary>
        /// <returns>Redirect to the Login action.</returns>
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// Renders the Access Denied error page if unauthorized navigation occurs.
        /// </summary>
        /// <returns>AccessDenied.cshtml Razor view shell.</returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
