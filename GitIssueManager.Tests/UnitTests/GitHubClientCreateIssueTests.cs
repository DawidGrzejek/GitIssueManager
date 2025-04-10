using GitIssueManager.Core.Models;
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
    public class GitHubClientCreateIssueTests
    {
        [Fact]
        public async Task CreateIssueAsync_SendsCorrectPayload_AndReturnsCreatedIssue()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.Content.ReadAsStringAsync().Result.Contains("\"title\":\"New Issue\"") &&
                        req.Content.ReadAsStringAsync().Result.Contains("\"body\":\"Issue description\"")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent("{\"number\": 1, \"title\": \"New Issue\", \"body\": \"Issue description\"}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new GitHubClient(httpClient, "fake-token");

            var issueRequest = new IssueRequest
            {
                Title = "New Issue",
                Description = "Issue description"
            };

            // Act
            var createdIssue = await client.CreateIssueAsync("owner", "repo", issueRequest);

            // Assert
            Assert.NotNull(createdIssue);
            Assert.Equal("New Issue", createdIssue.Title);
            Assert.Equal("Issue description", createdIssue.Description);
        }
    }

}
