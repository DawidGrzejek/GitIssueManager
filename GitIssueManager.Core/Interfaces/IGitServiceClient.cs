using GitIssueManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitIssueManager.Core.Interfaces
{
    /// <summary>
    /// Interface for Git service clients.
    /// </summary>
    public interface IGitServiceClient
    {
        Task<Issue> GetIssueAsync(string repositoryOwner, string repositoryName, string issueId);

        /// <summary>
        /// Gets a paginated list of issues from a repository
        /// </summary>
        /// <param name="repositoryOwner">Owner of the repository</param>
        /// <param name="repositoryName">Name of the repository</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="perPage">Number of items per page</param>
        /// <returns>List of issues</returns>
        Task<List<Issue>> GetIssuesAsync(string repositoryOwner, string repositoryName, int page = 1, int perPage = 30);
        Task<Issue> CreateIssueAsync(string repositoryOwner, string repositoryName, IssueRequest issueRequest);
        Task<Issue> UpdateIssueAsync(string repositoryOwner, string repositoryName, string issueId, IssueRequest issueRequest);
        Task<Issue> CloseIssueAsync(string repositoryOwner, string repositoryName, string issueId);
        Task<bool> ValidateCredentialsAsync();
        string ServiceType { get; }
    }
}