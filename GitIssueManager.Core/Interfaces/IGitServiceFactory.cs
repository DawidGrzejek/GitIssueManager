using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitIssueManager.Core.Interfaces
{
    /// <summary>
    /// Factory interface for creating instances of Git service clients.
    /// </summary>
    public interface IGitServiceFactory
    {
        IGitServiceClient CreateClient(string serviceType);
        void RegisterClientFactory(string serviceType, Func<IConfiguration, IGitServiceClient> factory);
    }
}
