using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitIssueManager.Core.Models 
{
    /// <summary>
    /// Represents a request to create or update an issue in a Git repository.
    /// </summary>
    public class IssueRequest 
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string RepositoryOwner { get; set; }
        public string RepositoryName { get; set; }
    }
}
