using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Web.Administration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FtpHelper
{
    class DeployHelperManager
    {
        private static IServiceProvider serviceProvider;
        //private static ILogger logger;
        static void Main(string[] args)
        {
            try
            {
                IConfiguration configuration = GetConfiguration();
                InitializeProviders(configuration);
                serviceProvider.GetService<ILoggerFactory>().CreateLogger<DeployHelperManager>();
                Execute();
            }
            catch (Exception exception)
            {
                try
                {
                    //logger.LogError(exception, "{0}.{1}:  error starting order confirmation sender. {2}:{3} {4} {5}", nameof(DeployHelperManager), nameof(Main), exception.GetType(), exception.Message, Environment.NewLine, exception.StackTrace);
                }
                catch (Exception loggingException)
                {
                    Console.Out.Write("{0}.{1}: error starting order confirmation sender. {2}:{3} {4} {5}", nameof(DeployHelperManager), nameof(Main), exception.GetType(), exception.Message, Environment.NewLine, exception.StackTrace);
                    Console.Out.Write("{0}.{1}: error finding logger service for order confirmation sender. {2}:{3} {4} {5}", nameof(DeployHelperManager), nameof(Main), loggingException.GetType(), loggingException.Message, Environment.NewLine, loggingException.StackTrace);
                }
            }
        }

        public static bool Execute()
        {
            IMaintenanceTask task = serviceProvider.GetService<IMaintenanceTask>();
            return task.Execute();
        }

        private static IConfiguration GetConfiguration()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            // Set up configuration sources.
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables();
            return builder.Build();
        }

        private static void InitializeProviders(IConfiguration configuration)
        {
            IServiceCollection serviceCollection = new ServiceCollection()
            .AddSingleton<ILoggerFactory, LoggerFactory>()
            .AddLogging()
            .Configure<FolderSettings>(configuration.GetSection("FolderSettings"))
            .AddTransient<IMaintenanceTask, DeployHelperTask>(); ;
            
            serviceProvider = serviceCollection.BuildServiceProvider();
            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddConsole(configuration.GetSection("Logging"));
            serviceProvider.GetService<ILoggerFactory>().CreateLogger<DeployHelperManager>();
        }



        
    }
}
