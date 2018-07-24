using Microsoft.Extensions.Logging;

namespace FtpHelper
{
    public interface IServiceContext
    {
        ILogger Logger { get; }
        SiteSettings Settings { get; }
    }
}
