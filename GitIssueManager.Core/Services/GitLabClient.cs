﻿using GitIssueManager.Core.Interfaces;
using GitIssueManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace GitIssueManager.Core.Services
{
    public class GitLabClient : IGitServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiToken;
        private const string BaseUrl = "https://gitlab.com/api/v4";
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public string ServiceType => "GitLab";

        public GitLabClient(HttpClient httpClient, string apiToken)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiToken = apiToken ?? throw new ArgumentNullException(nameof(apiToken));
        }

        /// <inheritdoc>/>
        public async Task<List<Issue>> GetIssuesAsync(string owner, string repository, int page = 1, int perPage = 30)
        {
            // In GitLab, we need the project ID which is usually owner/repository in URL-encoded form
            string projectId = HttpUtility.UrlEncode($"{owner}/{repository}");

            // Ensure valid pagination
            page = Math.Max(1, page);
            perPage = Math.Clamp(perPage, 1, 100);

            // Build query string with pagination parameters
            // GitLab uses "opened" instead of "open" for state
            var url = $"{BaseUrl}/projects/{projectId}/issues?page={page}&per_page={perPage}&state=opened";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var issues = JsonSerializer.Deserialize<List<JsonElement>>(content, _jsonOptions);

            var result = new List<Issue>();
            foreach (var issue in issues)
            {
                result.Add(new Issue
                {
                    Id = issue.GetStringOrDefault("iid"),
                    Title = issue.GetStringOrDefault("title"),
                    Description = issue.GetStringOrDefault("description"),
                    State = MapGitLabStateToGenericState(issue.GetStringOrDefault("state")),
                    RepositoryOwner = owner,
                    RepositoryName = repository,
                    ServiceType = ServiceType,
                    // Parse additional information
                    CreatedBy = GetAuthorName(issue),
                    CreatedAt = GetDateTime(issue, "created_at"),
                    UpdatedAt = GetDateTime(issue, "updated_at"),
                    ClosedAt = GetDateTime(issue, "closed_at")
                });
            }

            return result;
        }

        /// <summary>
        /// Maps GitLab issue states to a generic state representation.
        /// </summary>
        /// <param name="gitlabState">The state of the issue in GitLab (e.g., "opened", "closed").</param>
        /// <returns>A string representing the generic state (e.g., "open", "closed").</returns>
        private static string MapGitLabStateToGenericState(string gitlabState)
        {
            // Use a dictionary for better extensibility and performance
            var stateMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "opened", "open" },
                { "closed", "closed" }
            };

            // Return the mapped value or the original state if not found
            return stateMapping.TryGetValue(gitlabState, out var genericState) ? genericState : gitlabState;
        }

        /// <summary>
        /// Gets the author name from the issue JSON element.
        /// </summary>
        /// <param name="issue">The JSON element representing the issue.</param>
        /// <returns>The name of the author if available; otherwise, "Unknown".</returns>
        private static string GetAuthorName(JsonElement issue)
        {
            if (issue.TryGetProperty("author", out JsonElement author))
            {
                // Try name first, then username as fallback
                if (author.TryGetProperty("name", out JsonElement name) &&
                    name.ValueKind != JsonValueKind.Null)
                {
                    return name.ToString();
                }

                if (author.TryGetProperty("username", out JsonElement username) &&
                    username.ValueKind != JsonValueKind.Null)
                {
                    return username.ToString();
                }
            }
            return "Unknown";
        }

        /// <summary>
        /// Gets a DateTime value from the JSON element.
        /// </summary>
        /// <param name="element">The JSON element containing the property.</param>
        /// <param name="propertyName">The name of the property to retrieve.</param>
        /// <returns>The DateTime value if successfully parsed; otherwise, null.</returns>
        private static DateTime? GetDateTime(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property) &&
                property.ValueKind != JsonValueKind.Null)
            {
                if (DateTime.TryParse(property.ToString(), out DateTime result))
                {
                    return result;
                }
            }
            return null;
        }

        /// <inheritdoc>/>
        public async Task<Issue> GetIssueAsync(string owner, string repository, string issueNumber)
        {
            string projectId = HttpUtility.UrlEncode($"{owner}/{repository}");

            var response = await _httpClient.GetAsync($"{BaseUrl}/projects/{projectId}/issues/{issueNumber}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var issue = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

            return new Issue
            {
                Id = issue.GetStringOrDefault("iid", issueNumber),
                Title = issue.GetStringOrDefault("title"),
                Description = issue.GetStringOrDefault("description"),
                State = MapGitLabStateToGenericState(issue.GetStringOrDefault("state")),
                RepositoryOwner = owner,
                RepositoryName = repository,
                ServiceType = ServiceType,
                // Parse additional information
                CreatedBy = GetAuthorName(issue),
                CreatedAt = GetDateTime(issue, "created_at"),
                UpdatedAt = GetDateTime(issue, "updated_at"),
                ClosedAt = GetDateTime(issue, "closed_at")
            };
        }

        /// <inheritdoc>/>
        public async Task<Issue> CreateIssueAsync(string owner, string repository, IssueRequest request)
        {
            string projectId = HttpUtility.UrlEncode($"{owner}/{repository}");

            var issueData = new
            {
                title = request.Title,
                description = request.Description
            };

            var json = JsonSerializer.Serialize(issueData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BaseUrl}/projects/{projectId}/issues", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdIssue = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);

            return new Issue
            {
                Id = createdIssue.GetStringOrDefault("iid"),
                Title = createdIssue.GetStringOrDefault("title", request.Title),
                Description = createdIssue.GetStringOrDefault("description", request.Description),
                State = MapGitLabStateToGenericState(createdIssue.GetStringOrDefault("state", "opened")),
                RepositoryOwner = owner,
                RepositoryName = repository,
                ServiceType = ServiceType,
                // Parse additional information
                CreatedBy = GetAuthorName(createdIssue),
                CreatedAt = GetDateTime(createdIssue, "created_at"),
                UpdatedAt = GetDateTime(createdIssue, "updated_at"),
                ClosedAt = GetDateTime(createdIssue, "closed_at")
            };
        }

        /// <inheritdoc>/>
        public async Task<Issue> UpdateIssueAsync(string owner, string repository, string issueNumber, IssueRequest request)
        {
            string projectId = HttpUtility.UrlEncode($"{owner}/{repository}");

            var issueData = new
            {
                title = request.Title,
                description = request.Description
            };

            var json = JsonSerializer.Serialize(issueData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{BaseUrl}/projects/{projectId}/issues/{issueNumber}", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedIssue = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);

            return new Issue
            {
                Id = updatedIssue.GetStringOrDefault("iid", issueNumber),
                Title = updatedIssue.GetStringOrDefault("title", request.Title),
                Description = updatedIssue.GetStringOrDefault("description", request.Description),
                State = MapGitLabStateToGenericState(updatedIssue.GetStringOrDefault("state", "opened")),
                RepositoryOwner = owner,
                RepositoryName = repository,
                ServiceType = ServiceType,
                // Parse additional information
                CreatedBy = GetAuthorName(updatedIssue),
                CreatedAt = GetDateTime(updatedIssue, "created_at"),
                UpdatedAt = GetDateTime(updatedIssue, "updated_at"),
                ClosedAt = GetDateTime(updatedIssue, "closed_at")
            };
        }

        /// <inheritdoc>/>
        public async Task<Issue> CloseIssueAsync(string owner, string repository, string issueNumber)
        {
            string projectId = HttpUtility.UrlEncode($"{owner}/{repository}");

            // In GitLab, to close an issue we update it with state_event=close
            var issueData = new
            {
                state_event = "close"
            };

            var json = JsonSerializer.Serialize(issueData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{BaseUrl}/projects/{projectId}/issues/{issueNumber}", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var closedIssue = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);

            return new Issue
            {
                Id = closedIssue.GetStringOrDefault("iid", issueNumber),
                Title = closedIssue.GetStringOrDefault("title"),
                Description = closedIssue.GetStringOrDefault("description"),
                State = MapGitLabStateToGenericState(closedIssue.GetStringOrDefault("state", "closed")),
                RepositoryOwner = owner,
                RepositoryName = repository,
                ServiceType = ServiceType,
                // Parse additional information
                CreatedBy = GetAuthorName(closedIssue),
                CreatedAt = GetDateTime(closedIssue, "created_at"),
                UpdatedAt = GetDateTime(closedIssue, "updated_at"),
                ClosedAt = GetDateTime(closedIssue, "closed_at")
            };
        }

        /// <inheritdoc>/>
        public async Task<bool> ValidateCredentialsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/user");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}