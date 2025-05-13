# GitIssueManager Project Overview

## Project Structure

The solution is organized into three main projects:

GitIssueManager/ ├── GitIssueManager.Api/      

# ASP.NET Core Web API and static web UI 

├── GitIssueManager.Core/     

# Core domain models, interfaces, and factories ├── GitIssueManager.Tests/    

# xUnit test project for API and core logic


### 1. `GitIssueManager.Api`
- **Purpose:** Hosts the REST API and serves a static web UI (in `wwwroot/`).
- **Key files:**
  - `Program.cs`: Configures services, middleware, and API endpoints.
  - `wwwroot/index.html`: Bootstrap-based UI for interacting with the API.

### 2. `GitIssueManager.Core`
- **Purpose:** Contains core abstractions, models, and factories for Git service integration.
- **Key files:**
  - `IGitServiceClient`: Interface for Git service operations (get, create, update, close issues).
  - `IGitServiceFactory`/`GitServiceFactory`: Factory for creating service clients.
  - `Issue`, `IssueRequest`: Domain models for issues.

### 3. `GitIssueManager.Tests`
- **Purpose:** Contains automated tests for API and core logic.
- **Key files:**
  - Test classes using xUnit and Moq for mocking dependencies.

---

## Used Patterns

### Factory Pattern
- **Where:** `GitServiceFactory` implements `IGitServiceFactory`.
- **Why:** Allows dynamic creation of service clients (e.g., GitHub, GitLab) based on a string key.
- **Benefit:** Easily extendable to support new Git services without changing API logic.

### Dependency Injection
- **Where:** ASP.NET Core's DI container is used in `Program.cs`.
- **Why:** Decouples service implementations from their consumers, making the codebase more testable and maintainable.

### Minimal APIs
- **Where:** All API endpoints are defined using ASP.NET Core Minimal API syntax in `Program.cs`.
- **Why:** Reduces boilerplate and keeps endpoint logic concise.

---

## How It Works

1. **Startup:**
   - The API loads configuration, registers services (including the Git service factory), and sets up middleware (CORS, static files, Swagger).

2. **Web UI:**
   - Served from `wwwroot/index.html`.
   - Allows users to select a service, repository, and perform CRUD operations on issues via AJAX calls to the API.

3. **API Endpoints:**
   - `/api/services/{service}/repos/{owner}/{repo}/issues`: List, create, update, and close issues.
   - `/api/services/{service}/validate`: Validate credentials for a service.

4. **Service Abstraction:**
   - The API does not know the details of each Git service.
   - It uses `GitServiceFactory` to obtain an `IGitServiceClient` for the requested service.
   - All operations (get, create, update, close) are performed via this interface.

---

## Inheritance & Relationships

### Mermaid Diagram: Core Interfaces and Factory


classDiagram class IGitServiceClient { +Task<Issue> GetIssueAsync(...) +Task<List<Issue>> GetIssuesAsync(...) +Task<Issue> CreateIssueAsync(...) +Task<Issue> UpdateIssueAsync(...) +Task<Issue> CloseIssueAsync(...) +Task<bool> ValidateCredentialsAsync() +string ServiceType }
class IGitServiceFactory {
    +IGitServiceClient CreateClient(string)
    +void RegisterClientFactory(string, Func)
}

class GitServiceFactory {
    -IConfiguration _configuration
    -IHttpClientFactory _httpClientFactory
    -Dictionary _clientFactories
    +CreateClient(string)
    +RegisterClientFactory(string, Func)
}

IGitServiceFactory <|.. GitServiceFactory



---

## Extending the System

- **To add a new Git service (e.g., Bitbucket):**
  1. Implement `IGitServiceClient` for Bitbucket.
  2. Register the new client in `GitServiceFactory`.
  3. No changes needed in API endpoints or UI.

---

## Summary

- **Separation of concerns:** API, core logic, and tests are cleanly separated.
- **Extensible:** New Git services can be added with minimal changes.
- **Modern stack:** Uses .NET 8, C# 12, ASP.NET Core Minimal APIs, and Bootstrap for UI.
- **Testable:** Core logic is abstracted and easily mockable for unit testing.


