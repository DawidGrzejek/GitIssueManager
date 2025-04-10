using GitIssueManager.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GitIssueManager.Core.Services;

namespace GitIssueManager.Core.Factories
{

    // Extension method for service registration
    public static class GitServiceFactoryExtensions
    {
        /// <summary>
        /// Extension method to register Git service clients in the DI container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public static void AddGitServices(this IServiceCollection services)
        {
            // Register HttpClient
            services.AddHttpClient();

            // Register concrete type
            services.AddSingleton<GitServiceFactory>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var factory = new GitServiceFactory(configuration, httpClientFactory);

                // Register known Git service clients
                factory.RegisterClientFactory("github", (config, httpFactory) =>
                {
                    var client = httpFactory.CreateClient("GitHub");

                    // Set up HTTP client with required headers
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitIssueManager", "1.0"));

                    var apiToken = config["GitHub:ApiToken"];

                    if (!string.IsNullOrEmpty(apiToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
                    }

                    return new GitHubClient(client, apiToken);
                });


                factory.RegisterClientFactory("gitlab", (config, httpFactory) =>
                {
                    var client = httpFactory.CreateClient("GitLab");

                    // Set up HTTP client with required headers
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var apiToken = config["GitLab:ApiToken"];
                    if (!string.IsNullOrEmpty(apiToken))
                    {
                        client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", apiToken);
                    }

                    return new GitLabClient(client, apiToken);
                });

                return factory;
            });

            // // Register interface

            services.AddSingleton<IGitServiceFactory>(provider => provider.GetRequiredService<GitServiceFactory>());
        }
    }
}
