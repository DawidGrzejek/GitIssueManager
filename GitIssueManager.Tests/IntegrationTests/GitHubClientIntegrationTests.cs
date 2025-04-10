using GitIssueManager.Core.Interfaces;
using GitIssueManager.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitIssueManager.Tests.IntegrationTests
{
    public class GitHubClientIntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetIssuesAsync_ReturnsIssues_FromGitHubApi()
        {
            // Arrange
            var factory = ServiceProvider.GetRequiredService<IGitServiceFactory>();
            var client = factory.CreateClient("github");

            // Act
            var issues = await client.GetIssuesAsync("dawidgrzejek", "gitissuemanagerissues");

            // Assert
            Assert.NotNull(issues);
            Assert.NotEmpty(issues);
        }
    }

    public class GitLabClientIntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetIssuesAsync_ReturnsIssues_FromGitLabApi()
        {
            // Arrange
            var factory = ServiceProvider.GetRequiredService<IGitServiceFactory>();
            var client = factory.CreateClient("gitlab");

            // Act
            var issues = await client.GetIssuesAsync("dawid5694815", "gitissuemanager");

            // Assert
            Assert.NotNull(issues);
            Assert.NotEmpty(issues);
        }
    }
}
