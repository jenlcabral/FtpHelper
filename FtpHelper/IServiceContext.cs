using System;
using System.Security.Principal;
using Microsoft.Extensions.Logging;

namespace FtpHelper
{
    public interface IServiceContext
    {
        IPrincipal Principal { get; }
        ILogger Logger { get; }
        FolderSettings Settings { get; }
    }
}
