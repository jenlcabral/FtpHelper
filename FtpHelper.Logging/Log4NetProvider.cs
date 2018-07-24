using System.Collections.Concurrent;
using System.IO;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace FtpHelper.Logging
{
    public class Log4NetProvider : ILoggerProvider
    {
        private readonly string configurationFile;
        private readonly ConcurrentDictionary<string, Log4NetLogger> loggersByName = new ConcurrentDictionary<string, Log4NetLogger>();

        public Log4NetProvider(string log4NetConfigFile)
        {
            configurationFile = log4NetConfigFile;
        }

        public void Dispose()
        {
            loggersByName.Clear();
        }

        private Log4NetLogger CreateLoggerImplementation(string name)
        {
            return new Log4NetLogger(name, GetConfigurationFromFile(configurationFile));
        }

        private static XmlElement GetConfigurationFromFile(string filename)
        {
            XmlDocument log4netConfig = new XmlDocument();
            using (FileStream file = File.OpenRead(filename))
            {
                log4netConfig.Load(file);
            }
            return log4netConfig["log4net"];
        }

        ILogger ILoggerProvider.CreateLogger(string categoryName)
        {
            return loggersByName.GetOrAdd(categoryName, CreateLoggerImplementation);
        }
    }
}