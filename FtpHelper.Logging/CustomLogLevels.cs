using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using log4net.Core;

namespace FtpHelper.Logging
{
    public static class CustomLogLevels
    {
        const string EventLevelName = "EVENT";
        const string SqlLevelName = "SQL";
        const string PerformanceLevelName = "PERF";
        const string CommunicationLevelName = "COMM";
        const string CommandExecutionLevelName = "EXEC";
        const string AuditLevelName = "AUDIT";
        const string SupportRequestName = "SUPPORT";

        public static Level EventLevel { get; private set; }

        /// <summary>
        /// The level to use for sql logging
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static Level SqlLevel { get; private set; }

        /// <summary>
        /// The level to use for performance logging
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static Level PerformanceLevel { get; private set; }

        /// <summary>
        /// The level to use for communication logging
        /// </summary>
        public static Level CommunicationLevel { get; private set; }

        /// <summary>
        /// The level to use for command execution
        /// </summary>
        public static Level CommandExecutionLevel { get; private set; }

        /// <summary>
        /// The level to use for auditing the system
        /// </summary>
        public static Level AuditLevel { get; private set; }

        /// <summary>
        /// The level to use for support requests
        /// </summary>
        public static Level SupportRequestLevel { get; private set; }

        /// <summary>
        /// Initialize the logger to ensure the custom levels are within the system
        /// </summary>
        public static void Initialize()
        {
            // Add custom levels to logger
            EventLevel = new Level(Level.Info.Value + 1, EventLevelName);     // logging at Info level will capture events
            SqlLevel = new Level(Level.Debug.Value + 2, SqlLevelName);
            PerformanceLevel = new Level(Level.Debug.Value + 3, PerformanceLevelName);
            CommunicationLevel = new Level(Level.Info.Value + 2, CommunicationLevelName); // logging at Info level will capture communication
            CommandExecutionLevel = new Level(Level.Info.Value + 3, CommandExecutionLevelName); // logging at Info level will capture command execution
            AuditLevel = new Level(Level.Info.Value + 4, AuditLevelName); //logging at Info level will capture audits
            SupportRequestLevel = new Level(Level.Error.Value + 5, SupportRequestName); //logging at error level will capture support requests

            LogManager.GetLogger(typeof(CustomLogLevels)).Info("log initialization complete");
        }
    }
}