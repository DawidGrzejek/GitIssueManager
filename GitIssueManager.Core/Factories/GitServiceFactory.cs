using GitIssueManager.Core.Interfaces;
using GitIssueManager.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitIssueManager.Core.Factories 
{
    /// <summary>
    /// Factory class for creating instances of Git service clients.
    /// </summary>
    public class GitServiceFactory : IGitServiceFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Dictionary<string, Func<IConfiguration, IHttpClientFactory, IGitServiceClient>> _clientFactories;

        public GitServiceFactory(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _clientFactories = new Dictionary<string, Func<IConfiguration, IHttpClientFactory, IGitServiceClient>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registers a factory method for creating a Git service client.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="factory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void RegisterClientFactory(string serviceType, Func<IConfiguration, IHttpClientFactory, IGitServiceClient> factory)
        {
            if (string.IsNullOrEmpty(serviceType))
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            _clientFactories[serviceType] = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Creates an instance of a Git service client based on the provided service type.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public IGitServiceClient CreateClient(string serviceType)
        {
            if (string.IsNullOrEmpty(serviceType))
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (_clientFactories.TryGetValue(serviceType, out var factory))
            {
                return factory(_configuration, _httpClientFactory);
            }

            throw new ArgumentException($"Unsupported Git service: {serviceType}");
        }
    }
}
