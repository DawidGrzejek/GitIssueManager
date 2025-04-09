using GitIssueManager.Core.Factories;
using GitIssueManager.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace GitIssueManager.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>(optional: true);

            builder.Services.AddLogging();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Git Issue Manager API",
                    Version = "v1",
                    Description = "API for managing Git issues across multiple platforms."
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            builder.Services.AddGitServices();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Convert enums to strings
                    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve property names
                });

            // Add ability to server static files for a simple web UI
            builder.Services.AddDirectoryBrowser();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");

            // Serve static files for the web UI
            app.UseStaticFiles();
            app.UseDefaultFiles();

            // API Endpoints
            app.MapGet("/", () => Results.Redirect("/index.html")).ExcludeFromDescription();

            app.MapGet("/api/services/{service}/repos/{repositoryOwner}/{repositoryName}/issues", async (
                string service,
                string repositoryOwner,
                string repositoryName,
                GitServiceFactory factory,
                ILogger<Program> logger,
                [FromQuery] int page = 1,
                [FromQuery] int per_page = 10) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(repositoryOwner) || string.IsNullOrWhiteSpace(repositoryName))
                    {
                        return Results.BadRequest(new { error = "Repository owner and name are required" });
                    }

                    var client = factory.CreateClient(service);

                    logger.LogInformation("Fetching issues for {Service} repo {Owner}/{Repo}",
                        service, repositoryOwner, repositoryName);

                    var issues = await client.GetIssuesAsync(repositoryOwner, repositoryName, page, per_page);
                    return Results.Ok(issues);
                }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex, "Invalid argument when fetching issues");
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "HTTP error when calling Git service API");
                    return Results.Problem(
                        title: "Git Service API Error",
                        detail: ex.Message,
                        statusCode: (int?)ex.StatusCode ?? 500);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error fetching issues");
                    return Results.Problem(
                        title: "Unexpected Error",
                        detail: "An unexpected error occurred while processing your request",
                        statusCode: 500);
                }
            })
            .WithName("GetIssues")
            .WithOpenApi();


            // Get a specific issue - improved
            app.MapGet("/api/services/{service}/repos/{repositoryOwner}/{repositoryName}/issues/{issueNumber}", async (
                string service,
                string repositoryOwner,
                string repositoryName,
                string issueNumber,
                GitServiceFactory factory,
                ILogger<Program> logger) =>
            {
                try
                {
                    // Validate inputs
                    if (string.IsNullOrWhiteSpace(repositoryOwner) || string.IsNullOrWhiteSpace(repositoryName))
                    {
                        return Results.BadRequest(new { error = "Repository owner and name are required" });
                    }

                    if (string.IsNullOrWhiteSpace(issueNumber))
                    {
                        return Results.BadRequest(new { error = "Issue number is required" });
                    }

                    logger.LogInformation("Fetching issue {IssueNumber} from {Service} repo {Owner}/{Repo}",
                        issueNumber, service, repositoryOwner, repositoryName);

                    var client = factory.CreateClient(service);
                    var issue = await client.GetIssueAsync(repositoryOwner, repositoryName, issueNumber);

                    if (issue == null)
                    {
                        return Results.NotFound(new { error = $"Issue {issueNumber} not found" });
                    }

                    return Results.Ok(issue);
                }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex, "Invalid argument when fetching issue {IssueNumber}", issueNumber);
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "HTTP error when calling Git service API");
                    return Results.Problem(
                        title: "Git Service API Error",
                        detail: ex.Message,
                        statusCode: (int?)ex.StatusCode ?? 500);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error fetching issue {IssueNumber}", issueNumber);
                    return Results.Problem(
                        title: "Unexpected Error",
                        detail: "An unexpected error occurred while processing your request",
                        statusCode: 500);
                }
            })
            .WithName("GetIssue")
            .WithOpenApi();

            // Create a new issue - improved
            app.MapPost("/api/services/{service}/repos/{repositoryOwner}/{repositoryName}/issues", async (
                string service,
                string repositoryOwner,
                string repositoryName,
                [FromBody] IssueRequest request,
                GitServiceFactory factory,
                ILogger<Program> logger) =>
            {
                try
                {
                    // Validate inputs
                    if (string.IsNullOrWhiteSpace(repositoryOwner) || string.IsNullOrWhiteSpace(repositoryName))
                    {
                        return Results.BadRequest(new { error = "Repository owner and name are required" });
                    }

                    if (request == null)
                    {
                        return Results.BadRequest(new { error = "Issue request body is required" });
                    }

                    if (string.IsNullOrWhiteSpace(request.Title))
                    {
                        return Results.BadRequest(new { error = "Issue title is required" });
                    }

                    logger.LogInformation("Creating new issue in {Service} repo {Owner}/{Repo}",
                        service, repositoryOwner, repositoryName);

                    var client = factory.CreateClient(service);
                    var issue = await client.CreateIssueAsync(repositoryOwner, repositoryName, request);

                    logger.LogInformation("Successfully created issue {IssueId} in {Service} repo {Owner}/{Repo}",
                        issue.Id, service, repositoryOwner, repositoryName);

                    return Results.Created($"/api/services/{service}/repos/{repositoryOwner}/{repositoryName}/issues/{issue.Id}", issue);
                }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex, "Invalid argument when creating issue");
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "HTTP error when calling Git service API");
                    return Results.Problem(
                        title: "Git Service API Error",
                        detail: ex.Message,
                        statusCode: (int?)ex.StatusCode ?? 500);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error creating issue");
                    return Results.Problem(
                        title: "Unexpected Error",
                        detail: "An unexpected error occurred while processing your request",
                        statusCode: 500);
                }
            })
            .WithName("CreateIssue")
            .WithOpenApi();

            // Update an issue - improved
            app.MapPut("/api/services/{service}/repos/{repositoryOwner}/{repositoryName}/issues/{issueNumber}", async (
                string service,
                string repositoryOwner,
                string repositoryName,
                string issueNumber,
                [FromBody] IssueRequest request,
                GitServiceFactory factory,
                ILogger<Program> logger) =>
            {
                try
                {
                    // Validate inputs
                    if (string.IsNullOrWhiteSpace(repositoryOwner) || string.IsNullOrWhiteSpace(repositoryName))
                    {
                        return Results.BadRequest(new { error = "Repository owner and name are required" });
                    }

                    if (string.IsNullOrWhiteSpace(issueNumber))
                    {
                        return Results.BadRequest(new { error = "Issue number is required" });
                    }

                    if (request == null)
                    {
                        return Results.BadRequest(new { error = "Issue request body is required" });
                    }

                    if (string.IsNullOrWhiteSpace(request.Title))
                    {
                        return Results.BadRequest(new { error = "Issue title is required" });
                    }

                    logger.LogInformation("Updating issue {IssueNumber} in {Service} repo {Owner}/{Repo}",
                        issueNumber, service, repositoryOwner, repositoryName);

                    var client = factory.CreateClient(service);
                    var issue = await client.UpdateIssueAsync(repositoryOwner, repositoryName, issueNumber, request);

                    logger.LogInformation("Successfully updated issue {IssueId} in {Service} repo {Owner}/{Repo}",
                        issue.Id, service, repositoryOwner, repositoryName);

                    return Results.Ok(issue);
                }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex, "Invalid argument when updating issue {IssueNumber}", issueNumber);
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "HTTP error when calling Git service API");
                    return Results.Problem(
                        title: "Git Service API Error",
                        detail: ex.Message,
                        statusCode: (int?)ex.StatusCode ?? 500);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error updating issue {IssueNumber}", issueNumber);
                    return Results.Problem(
                        title: "Unexpected Error",
                        detail: "An unexpected error occurred while processing your request",
                        statusCode: 500);
                }
            })
            .WithName("UpdateIssue")
            .WithOpenApi();

            // Close an issue - improved
            app.MapPost("/api/services/{service}/repos/{repositoryOwner}/{repositoryName}/issues/{issueNumber}/close", async (
                string service,
                string repositoryOwner,
                string repositoryName,
                string issueNumber,
                GitServiceFactory factory,
                ILogger<Program> logger) =>
            {
                try
                {
                    // Validate inputs
                    if (string.IsNullOrWhiteSpace(repositoryOwner) || string.IsNullOrWhiteSpace(repositoryName))
                    {
                        return Results.BadRequest(new { error = "Repository owner and name are required" });
                    }

                    if (string.IsNullOrWhiteSpace(issueNumber))
                    {
                        return Results.BadRequest(new { error = "Issue number is required" });
                    }

                    logger.LogInformation("Closing issue {IssueNumber} in {Service} repo {Owner}/{Repo}",
                        issueNumber, service, repositoryOwner, repositoryName);

                    var client = factory.CreateClient(service);
                    var issue = await client.CloseIssueAsync(repositoryOwner, repositoryName, issueNumber);

                    logger.LogInformation("Successfully closed issue {IssueId} in {Service} repo {Owner}/{Repo}",
                        issue.Id, service, repositoryOwner, repositoryName);

                    return Results.Ok(issue);
                }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex, "Invalid argument when closing issue {IssueNumber}", issueNumber);
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "HTTP error when calling Git service API");
                    return Results.Problem(
                        title: "Git Service API Error",
                        detail: ex.Message,
                        statusCode: (int?)ex.StatusCode ?? 500);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error closing issue {IssueNumber}", issueNumber);
                    return Results.Problem(
                        title: "Unexpected Error",
                        detail: "An unexpected error occurred while processing your request",
                        statusCode: 500);
                }
            })
            .WithName("CloseIssue")
            .WithOpenApi();

            // Validate credentials - improved
            app.MapGet("/api/services/{service}/validate", async (
                string service,
                GitServiceFactory factory,
                ILogger<Program> logger) =>
            {
                try
                {
                    // Validate inputs
                    if (string.IsNullOrWhiteSpace(service))
                    {
                        return Results.BadRequest(new { error = "Service type is required" });
                    }

                    logger.LogInformation("Validating credentials for {Service}", service);

                    var client = factory.CreateClient(service);
                    var isValid = await client.ValidateCredentialsAsync();

                    logger.LogInformation("Credential validation for {Service}: {IsValid}", service, isValid);

                    return Results.Ok(new { isValid });
                }
                catch (ArgumentException ex)
                {
                    logger.LogWarning(ex, "Invalid argument when validating credentials for {Service}", service);
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "HTTP error when calling Git service API");
                    return Results.Problem(
                        title: "Git Service API Error",
                        detail: ex.Message,
                        statusCode: (int?)ex.StatusCode ?? 500);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error validating credentials for {Service}", service);
                    return Results.Problem(
                        title: "Unexpected Error",
                        detail: "An unexpected error occurred while processing your request",
                        statusCode: 500);
                }
            })
            .WithName("ValidateCredentials")
            .WithOpenApi();
        }
    }
}