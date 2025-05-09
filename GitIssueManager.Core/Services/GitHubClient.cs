﻿using GitIssueManager.Core.Interfaces;
using GitIssueManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GitIssueManager.Core.Services
{
    public class GitHubClient : IGitServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiToken;
        private const string BaseUrl = "https://api.github.com";
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public string ServiceType => "GitHub";

        public GitHubClient(HttpClient httpClient, string apiToken)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiToken = apiToken ?? throw new ArgumentNullException(nameof(apiToken));
        }

        /// <inheritdoc/>
        public async Task<List<Issue>> GetIssuesAsync(string owner, string repository, int page = 1, int perPage = 30)
        {
            // Ensure valid pagination
            page = Math.Max(1, page);
            perPage = Math.Clamp(perPage, 1, 100);

            // Build query string with pagination parameters
            var url = $"{BaseUrl}/repos/{owner}/{repository}/issues?page={page}&per_page={perPage}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var issues = JsonSerializer.Deserialize<List<JsonElement>>(content, _jsonOptions);

            var result = new List<Issue>();
            foreach (var issue in issues)
            {
                result.Add(new Issue
                {
                    Id = issue.GetStringOrDefault("number"),
                    Title = issue.GetStringOrDefault("title"),
                    Description = issue.GetStringOrDefault("body"),
                    State = issue.GetStringOrDefault("state"),
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
        /// Gets the author name from the issue JSON element.
        /// </summary>
        /// <param name="issue">The JSON element representing the issue.</param>
        /// <returns>The author's login name if available; otherwise, "Unknown".</returns>
        private static string GetAuthorName(JsonElement issue)
        {
            if (issue.TryGetProperty("user", out JsonElement user) &&
                user.TryGetProperty("login", out JsonElement login))
            {
                return login.ToString();
            }
            return "Unknown";
        }

        /// <summary>
        /// Gets a DateTime value from the JSON element with a default fallback.
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

        /// <inheritdoc/>
        public async Task<Issue> GetIssueAsync(string owner, string repository, string issueNumber)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/repos/{owner}/{repository}/issues/{issueNumber}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var issue = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

            return new Issue
            {
                Id = issue.GetStringOrDefault("number", issueNumber),
                Title = issue.GetStringOrDefault("title"),
                Description = issue.GetStringOrDefault("body"),
                State = issue.GetStringOrDefault("state"),
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

        /// <inheritdoc/>
        public async Task<Issue> CreateIssueAsync(string owner, string repository, IssueRequest request)
        {
            var issueData = new
            {
                title = request.Title,
                body = request.Description
            };

            var json = JsonSerializer.Serialize(issueData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BaseUrl}/repos/{owner}/{repository}/issues", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdIssue = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);

            return new Issue
            {
                Id = createdIssue.GetStringOrDefault("number"),
                Title = createdIssue.GetStringOrDefault("title", request.Title),
                Description = createdIssue.GetStringOrDefault("body", request.Description),
                State = createdIssue.GetStringOrDefault("state", "open"),
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

        /// <inheritdoc/>
        public async Task<Issue> UpdateIssueAsync(string owner, string repository, string issueNumber, IssueRequest request)
        {
            var issueData = new
            {
                title = request.Title,
                body = request.Description
            };

            var json = JsonSerializer.Serialize(issueData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync($"{BaseUrl}/repos/{owner}/{repository}/issues/{issueNumber}", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedIssue = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);

            return new Issue
            {
                Id = updatedIssue.GetStringOrDefault("number", issueNumber),
                Title = updatedIssue.GetStringOrDefault("title", request.Title),
                Description = updatedIssue.GetStringOrDefault("body", request.Description),
                State = updatedIssue.GetStringOrDefault("state", "open"),
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

        /// <inheritdoc/>
        public async Task<Issue> CloseIssueAsync(string owner, string repository, string issueNumber)
        {
            var issueData = new
            {
                state = "closed"
            };

            var json = JsonSerializer.Serialize(issueData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync($"{BaseUrl}/repos/{owner}/{repository}/issues/{issueNumber}", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var closedIssue = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);

            return new Issue
            {
                Id = closedIssue.GetStringOrDefault("number", issueNumber),
                Title = closedIssue.GetStringOrDefault("title"),
                Description = closedIssue.GetStringOrDefault("body"),
                State = closedIssue.GetStringOrDefault("state", "closed"),
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

        /// <inheritdoc/>
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