using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitIssueManager.Core.Interfaces
{
    /// <summary>
    /// Factory interface for creating and managing instances of Git service clients.
    /// </summary>
    public interface IGitServiceFactory
    {
        /// <summary>
        /// Creates a Git service client for the specified service type.
        /// </summary>
        /// <param name="serviceType">The type of the Git service (e.g., "GitHub", "GitLab").</param>
        /// <returns>An instance of <see cref="IGitServiceClient"/> configured for the specified service type.</returns>
        IGitServiceClient CreateClient(string serviceType);

        /// <summary>
        /// Registers a factory method for creating a Git service client.
        /// </summary>
        /// <param name="serviceType">The type of the Git service (e.g., "GitHub", "GitLab").</param>
        /// <param name="factory">
        /// A factory method that takes an <see cref="IConfiguration"/> and <see cref="IHttpClientFactory"/> 
        /// as input and returns an instance of <see cref="IGitServiceClient"/>.
        /// </param>
        void RegisterClientFactory(string serviceType, Func<IConfiguration, IHttpClientFactory, IGitServiceClient> factory);
    }
}
