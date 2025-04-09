using GitIssueManager.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Register concrete type
            services.AddSingleton<GitServiceFactory>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var factory = new GitServiceFactory(configuration);

                // Register known Git service clients
                factory.RegisterClientFactory("github", config => new Services.GitHubClient(config["GitHub:ApiToken"]));
                factory.RegisterClientFactory("gitlab", config => new Services.GitLabClient(config["GitLab:ApiToken"]));

                return factory;
            });

            // // Register interface

            services.AddSingleton<IGitServiceFactory>(provider => provider.GetRequiredService<GitServiceFactory>());
        }
    }
}
