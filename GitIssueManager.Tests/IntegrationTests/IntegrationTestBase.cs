using GitIssueManager.Core.Factories;
using GitIssueManager.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

public class IntegrationTestBase
{
    protected readonly ServiceProvider ServiceProvider;

    public IntegrationTestBase()
    {
        var serviceCollection = new ServiceCollection();

        // Add configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        // Register Git services
        serviceCollection.AddGitServices();

        // Build the service provider
        ServiceProvider = serviceCollection.BuildServiceProvider();
    }
}
