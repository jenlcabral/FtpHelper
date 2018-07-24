using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FtpHelper
{
    public class ServiceContext : IServiceContext
    {
        public ILogger Logger => logger;
        public SiteSettings Settings => settings;

        private ILogger logger;
        private SiteSettings settings;

        public ServiceContext( ILogger<ServiceContext> logger, IOptions<SiteSettings> settings)
        {
            this.logger = logger;
            this.settings = settings.Value;
        }
    }
}
