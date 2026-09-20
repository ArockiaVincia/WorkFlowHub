using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EmployeeWorkFlowHub.Models;

namespace EmployeeWorkFlowHub.Controllers;

/// <summary>
/// MVC Controller serving home dashboard, general utility, and error pages.
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Renders the main dashboard page.
    /// </summary>
    /// <returns>Index.cshtml Razor view shell containing the dashboard widgets.</returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Renders the application privacy policy page.
    /// </summary>
    /// <returns>Privacy.cshtml Razor view shell.</returns>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Renders standard error view with correlation request ID for unhandled exceptions.
    /// </summary>
    /// <returns>Error.cshtml Razor view with populated ErrorViewModel.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
