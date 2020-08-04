using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Luna.Clients.Azure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Luna.Clients.Azure.APIM
{
    public class APIMUtility: IAPIMUtility
    {
        private Guid _subscriptionId;
        private string _resourceGroupName;
        private string _apimServiceName;
        private ILogger<APIMUtility> _logger;
        private HttpClient _httpClient;
        
        [ActivatorUtilitiesConstructor]
        public APIMUtility(IOptionsMonitor<APIMConfigurationOption> options, 
                           ILogger<APIMUtility> logger,
                           HttpClient httpClient)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _subscriptionId = options.CurrentValue.Config.SubscriptionId;
            _resourceGroupName = options.CurrentValue.Config.ResourceGroupname;
            _apimServiceName = options.CurrentValue.Config.APIMServiceName;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }
    }
}
