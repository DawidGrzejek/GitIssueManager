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
    public class GitHubClientGetIssuesTests
    {
        [Fact]
        public async Task GetIssuesAsync_ReturnsIssues_WhenApiResponseIsValid()
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
                    Content = new StringContent("[{\"number\": 1, \"title\": \"Test Issue\", \"body\": \"Description\"}]")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new GitHubClient(httpClient, "fake-token");

            // Act
            var issues = await client.GetIssuesAsync("owner", "repo");

            // Assert
            Assert.Single(issues);
            Assert.Equal("Test Issue", issues[0].Title);
        }

        [Fact]
        public async Task GetIssuesAsync_ThrowsHttpRequestException_WhenUnauthorized()
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
                    StatusCode = HttpStatusCode.Unauthorized
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new GitHubClient(httpClient, "invalid-token");

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => client.GetIssuesAsync("owner", "repo"));
        }
    }

}
