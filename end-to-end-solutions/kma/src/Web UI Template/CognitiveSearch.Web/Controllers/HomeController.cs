using CognitiveSearch.Web.Configuration;
using CognitiveSearch.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CognitiveSearch.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppConfig _appConfig;

        public HomeController(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult UseOfDatasets()
        {
            return View();
        }
    }
}