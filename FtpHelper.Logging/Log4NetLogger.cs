using System;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using log4net;
using log4net.Repository;
using Microsoft.Extensions.Logging;

namespace FtpHelper.Logging
{
    public class Log4NetLogger : ILogger
    {
        public ILog Logger { get { return log; } }
        private readonly ILog log;
        private readonly string name;
        private readonly XmlElement xmlElement;
        private ILoggerRepository loggerRepository;

        public Log4NetLogger(string name, XmlElement xmlElement)
        {
            this.name = name;
            this.xmlElement = xmlElement;

            GlobalContext.Properties["host"] = Environment.MachineName;
            GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
            GlobalContext.Properties["appName"] = name;
            InitializeRepository();
            log = LogManager.GetLogger(loggerRepository.Name, name);
            log4net.Config.XmlConfigurator.Configure(loggerRepository, xmlElement);
        }

        private void InitializeRepository()
        {
            loggerRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            // Add custom levels to logger            
            CustomLogLevels.Initialize();
            loggerRepository.LevelMap.Add(CustomLogLevels.EventLevel);
            loggerRepository.LevelMap.Add(CustomLogLevels.SqlLevel);
            loggerRepository.LevelMap.Add(CustomLogLevels.PerformanceLevel);
            loggerRepository.LevelMap.Add(CustomLogLevels.CommunicationLevel);
            loggerRepository.LevelMap.Add(CustomLogLevels.CommandExecutionLevel);
            loggerRepository.LevelMap.Add(CustomLogLevels.AuditLevel);
            loggerRepository.LevelMap.Add(CustomLogLevels.SupportRequestLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return log.IsFatalEnabled;
                case LogLevel.Debug:
                case LogLevel.Trace:
                    return log.IsDebugEnabled;
                case LogLevel.Error:
                    return log.IsErrorEnabled;
                case LogLevel.Information:
                    return log.IsInfoEnabled;
                case LogLevel.Warning:
                    return log.IsWarnEnabled;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));
            string message = null;
            if (null != formatter)
                message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                switch (logLevel)
                {
                    case LogLevel.Critical:
                        log.Fatal(message);
                        break;
                    case LogLevel.Debug:
                    case LogLevel.Trace:
                        log.Debug(message);
                        break;
                    case LogLevel.Error:
                        log.Error(message);
                        break;
                    case LogLevel.Information:
                        log.Info(message);
                        break;
                    case LogLevel.Warning:
                        log.Warn(message);
                        break;
                    default:
                        log.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
                        log.Info(message, exception);
                        break;
                }
            }
        }
    }
}
