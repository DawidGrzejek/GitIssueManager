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
        /// <summary>
        /// Retrieves a specific issue from a repository.
        /// </summary>
        /// <param name="repositoryOwner">The owner of the repository.</param>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="issueId">The unique identifier of the issue.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the issue details.</returns>
        Task<Issue> GetIssueAsync(string repositoryOwner, string repositoryName, string issueId);

        /// <summary>
        /// Gets a paginated list of issues from a repository.
        /// </summary>
        /// <param name="repositoryOwner">The owner of the repository.</param>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="page">The page number (1-based).</param>
        /// <param name="perPage">The number of items per page.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of issues.</returns>
        Task<List<Issue>> GetIssuesAsync(string repositoryOwner, string repositoryName, int page = 1, int perPage = 30);

        /// <summary>
        /// Creates a new issue in the specified repository.
        /// </summary>
        /// <param name="repositoryOwner">The owner of the repository.</param>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="issueRequest">The details of the issue to create.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created issue.</returns>
        Task<Issue> CreateIssueAsync(string repositoryOwner, string repositoryName, IssueRequest issueRequest);

        /// <summary>
        /// Updates an existing issue in the specified repository.
        /// </summary>
        /// <param name="repositoryOwner">The owner of the repository.</param>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="issueId">The unique identifier of the issue to update.</param>
        /// <param name="issueRequest">The updated details of the issue.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated issue.</returns>
        Task<Issue> UpdateIssueAsync(string repositoryOwner, string repositoryName, string issueId, IssueRequest issueRequest);

        /// <summary>
        /// Closes an existing issue in the specified repository.
        /// </summary>
        /// <param name="repositoryOwner">The owner of the repository.</param>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="issueId">The unique identifier of the issue to close.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the closed issue.</returns>
        Task<Issue> CloseIssueAsync(string repositoryOwner, string repositoryName, string issueId);

        /// <summary>
        /// Validates the credentials of the user.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the credentials are valid.</returns>
        Task<bool> ValidateCredentialsAsync();

        /// <summary>
        /// Gets the type of the Git service (e.g., GitHub, GitLab).
        /// </summary>
        string ServiceType { get; }
    }
}