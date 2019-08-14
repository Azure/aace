using CognitiveSearch.Web.Configuration;
using CognitiveSearch.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace CognitiveSearch.Web.Controllers
{
    [Route("[controller]")]
    public class SearchController : Controller
    {
        private readonly AppConfig _appConfig;

        public SearchController(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        [HttpGet]
        [HttpPost]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                query = "";
            }

            var viewModel = new SearchViewModel
            {
                AppConfig = _appConfig,
                Query = query,
                SearchId = string.Empty
            };

            return View(viewModel);
        }

        [HttpGet("results/{view}")]
        public IActionResult GetResultsListView(string view)
        {
            var partialView = view == "entitymap"
                ? "_EntityMap"
                : "_SearchResults";

            return PartialView(partialView);
        }
    }
}