using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitIssueManager.Core.Models 
{
    /// <summary>
    /// Represents an issue in a Git repository.
    /// </summary>
    public class Issue 
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string State { get; set; } // f.e. "open", "closed"
        public string RepositoryOwner { get; set; }
        public string RepositoryName { get; set; }
        public string ServiceType { get; set; } // f.e. "GitHub", "GitLab"

        // Additional information
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
