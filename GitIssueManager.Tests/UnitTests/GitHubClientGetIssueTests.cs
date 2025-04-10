using GitIssueManager.Core.Services;
using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GitIssueManager.Tests.UnitTests
{
    public class GitHubClientGetIssueTests
    {
        [Fact]
        public async Task GetIssueAsync_ReturnsCorrectIssue_WhenApiResponseIsValid()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"number\": 1, \"title\": \"Test Issue\", \"body\": \"Description\"}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new GitHubClient(httpClient, "fake-token");

            // Act
            var issue = await client.GetIssueAsync("owner", "repo", "1");

            // Assert
            Assert.NotNull(issue);
            Assert.Equal("1", issue.Id);
            Assert.Equal("Test Issue", issue.Title);
            Assert.Equal("Description", issue.Description);
        }
    }

}
