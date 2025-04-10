using GitIssueManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitIssueManager.Tests.UnitTests
{
    public class ConstructorTests
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenApiTokenIsNull()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new GitHubClient(httpClient, null));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenHttpClientIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new GitHubClient(null, "fake-token"));
        }

    }
}
