using GitIssueManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitIssueManager.Tests.UnitTests
{
    public class GitHubClientConstructorTests
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenApiTokenIsNull()
        {
            var httpClient = new HttpClient();
            Assert.Throws<ArgumentNullException>(() => new GitHubClient(httpClient, null));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenHttpClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GitHubClient(null, "fake-token"));
        }
    }

}
