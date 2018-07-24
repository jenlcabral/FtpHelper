using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using log4net.Core;
using Microsoft.Extensions.Logging;

namespace FtpHelper.Logging
{
    public static class LoggingExtensions
    {
        #region Events
        public static bool IsEventEnabled(this ILog log)
        {
            return log.Logger.IsEnabledFor(CustomLogLevels.EventLevel);
        }

        #endregion

        #region Performance

        public static bool IsPerformanceEnabled(this ILog log)
        {
            return log.Logger.IsEnabledFor(CustomLogLevels.PerformanceLevel);
        }

        public static void Performance(this ILog log, string message)
        {
            CallLogger(log, CustomLogLevels.PerformanceLevel, message, null);
        }

        public static void PerformanceFormat(this ILog log, string message, params object[] args)
        {
            CallLogger(log, CustomLogLevels.PerformanceLevel, message, null, args);
        }

        #endregion

        #region Sql

        public static bool IsSqlEnabled(this ILog log)
        {
            return log.Logger.IsEnabledFor(CustomLogLevels.SqlLevel);
        }

        public static void Sql(this ILog log, string message)
        {
            CallLogger(log, CustomLogLevels.SqlLevel, message, null);
        }

        public static void SqlFormat(this ILog log, string message, params object[] args)
        {
            CallLogger(log, CustomLogLevels.SqlLevel, message, null, args);
        }

        #endregion

        #region Communication

        public static bool IsCommunicationEnabled(this ILog log)
        {
            return log.Logger.IsEnabledFor(CustomLogLevels.CommunicationLevel);
        }

        public static void Communication(this ILog log, string message)
        {
            CallLogger(log, CustomLogLevels.CommunicationLevel, message, null);
        }

        public static void CommunicationFormat(this ILog log, string message, params object[] args)
        {
            CallLogger(log, CustomLogLevels.CommunicationLevel, message, null, args);
        }

        #endregion

        #region CommandExecution

        public static bool IsCommandExecutionEnabled(this ILog log)
        {
            return log.Logger.IsEnabledFor(CustomLogLevels.CommandExecutionLevel);
        }

        public static void CommandExecution(this ILog log, string message)
        {
            CallLogger(log, CustomLogLevels.CommandExecutionLevel, message, null);
        }

        public static void CommandExecutionFormat(this ILog log, string message, params object[] args)
        {
            CallLogger(log, CustomLogLevels.CommandExecutionLevel, message, null, args);
        }

        #endregion

        #region Audit

        public static bool IsAuditEnabled(this ILog log)
        {
            return log.Logger.IsEnabledFor(CustomLogLevels.AuditLevel);
        }

        public static void Audit(this ILog log, string message)
        {
            CallLogger(log, CustomLogLevels.AuditLevel, message, null);
        }

        public static void AuditFormat(this ILog log, string message, params object[] args)
        {
            CallLogger(log, CustomLogLevels.AuditLevel, message, null, args);
        }

        #endregion

        #region SupportRequest

        public static bool IsSupportRequestEnabled(this ILog log)
        {
            return log.Logger.IsEnabledFor(CustomLogLevels.SupportRequestLevel);
        }

        public static void SupportRequest(this ILog log, string message)
        {
            CallLogger(log, CustomLogLevels.SupportRequestLevel, message, null);
        }

        public static void SupportRequest(this ILog log, string message, Exception exception)
        {
            CallLogger(log, CustomLogLevels.SupportRequestLevel, message, exception);
        }

        public static void SupportRequestFormat(this ILog log, string message, params object[] args)
        {
            CallLogger(log, CustomLogLevels.SupportRequestLevel, message, null, args);
        }

        #endregion

        #region Trace

        public static bool IsTraceEnabled(this ILog log)
        {
            return log.Logger.IsEnabledFor(Level.Trace);
        }

        public static void Trace(this ILog log, string message)
        {
            CallLogger(log, Level.Trace, message, null);
        }

        public static void TraceFormat(this ILog log, string message, params object[] args)
        {
            CallLogger(log, Level.Trace, message, null, args);
        }

        #endregion

        /// <summary>
        /// Dump a stream to the log.
        /// NOTE: The caller is repsonsible for cleaning up the stream.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="level"></param>
        /// <param name="tag"></param>
        /// <param name="reader"></param>
        public static void DumpStream(this ILog log, Level level, string tag, StreamReader reader)
        {
            int lineNumber = 1;
            CallLogger(log, level, "**** start of {0} ****", null, tag);
            while (!reader.EndOfStream)
            {
                CallLogger(log, level, "{0}:{1,4} {2}", null, tag, lineNumber, reader.ReadLine());
                lineNumber++;
            }
            CallLogger(log, level, "**** end of {0}   ****", null, tag);

        }

        #region Private Methods

        private static void CallLogger(ILog log, Level level, string message, Exception exception, params object[] args)
        {
            string formattedMessage = string.Empty;

            if (args != null && args.Length > 0)
            {
                formattedMessage = string.Format(message, args);
            }
            else
            {
                formattedMessage = message;
            }

            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, level, formattedMessage, exception);
        }

        #endregion

        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, string log4NetConfigFile)
        {
            factory.AddProvider(new Log4NetProvider(log4NetConfigFile));
            return factory;
        }

        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory)
        {
            factory.AddProvider(new Log4NetProvider("log4net.config"));
            return factory;
        }
    }
}