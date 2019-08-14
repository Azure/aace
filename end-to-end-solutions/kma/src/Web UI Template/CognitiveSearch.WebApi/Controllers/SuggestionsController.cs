using CognitiveSearch.Azure.AppInsights;
using CognitiveSearch.Azure.Search;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveSearch.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class SuggestionsController : ControllerBase
    {
        private readonly AppInsightsConfig _appInsightsConfig;
        private readonly SearchConfig _searchConfig;
        private readonly SearchClient _searchClient;
        private readonly TelemetryClient _telemetryClient;

        public SuggestionsController(AppInsightsConfig appInsightsConfig, SearchConfig searchConfig, TelemetryClient telemetryClient)
        {
            _appInsightsConfig = appInsightsConfig;
            _searchConfig = searchConfig;
            _telemetryClient = telemetryClient;
            _telemetryClient.InstrumentationKey = _appInsightsConfig.InstrumentationKey;
            _searchClient = new SearchClient(_searchConfig, _telemetryClient);
        }

        [HttpGet]
        public async Task<IActionResult> Get(string search = "")
        {
            if(string.IsNullOrWhiteSpace(search))
            {
                return new JsonResult("");
            }

            search = search.Replace("-", "").Replace("?", "");

            var response =  await _searchClient.Suggest(search, false);
            var list = response.Results.Select(r => r.Text);
            return await Task.FromResult(new JsonResult(list));
        }
    }
}