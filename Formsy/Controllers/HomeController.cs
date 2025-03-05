using System.Diagnostics;
using Formsy.Models;
using Microsoft.AspNetCore.Mvc;

namespace Formsy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Constructor to initialize the logger
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // GET: Displays the home page
        public IActionResult Index()
        {
            return View();
        }

        // GET: Handles error page, displaying error details
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
