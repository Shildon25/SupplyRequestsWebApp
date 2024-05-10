using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyManagement.Models.ViewModels;
using System.Diagnostics;

namespace SupplyManagement.WebApp.Areas.Login.Controllers
{
    [Area("Login")]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                // Display the index page
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while displaying the Home page.");
                // Handle the exception gracefully, perhaps return an error view
                return View("Error", new ErrorViewModel(String.Format("An error occurred while displaying the Home page. {0}", ex.Message)));
            }
        }

        [Authorize]
        public IActionResult Privacy()
        {
            try
            {
                // Display the privacy page
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while displaying the Privacy page.");
                // Handle the exception gracefully, perhaps return an error view
                return View("Error", new ErrorViewModel(String.Format("An error occurred while displaying the Privacy page. {0}", ex.Message)));
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            try
            {
                // Display the error page with error details
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while displaying the error page.");
                // Handle the exception gracefully, perhaps return an error view
                return View("Error", new ErrorViewModel(String.Format("An error occurred while displaying the Error page. {0}", ex.Message)));
            }
        }
    }
}
