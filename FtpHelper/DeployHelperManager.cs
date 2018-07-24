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
            // check if files are in directory. 
            // if files found,
            //// if not, turn off app
            string path = @"C:\Temp\ftptemp";
            if (Directory.EnumerateFileSystemEntries(path).Any())
            {
                
                //turn off IIS - transfer files to iis location, turn IIS back on after transfer is done
                IEnumerable<string> files = Directory.EnumerateFiles(path,"*", SearchOption.AllDirectories);
                if (files.Count() > 0)
                {
                    Console.WriteLine("there are files to transfer");
                    Console.WriteLine("turning off IIS");
                    Console.WriteLine("moving files");
                    ServerManager server = new ServerManager();
                    Site site = server.Sites.FirstOrDefault(s => s.Name == "testsite");
                    if (site != null)
                    {
                        site.Stop();
                        if (site.State == ObjectState.Stopped)
                        {
                            IEnumerable<Process> dotnetProcesses = Process.GetProcesses().Where(pr => pr.ProcessName == "dotnet");
                            if(dotnetProcesses.Count() > 1) {
                                //This app also uses dotnet, the first one should be the longest running
                                //if the site was down for any other reason, the dotnet running the site 
                                //might have been previously killed do not need to kill it again
                                Process theOneToKill = dotnetProcesses.Where(process => process.StartTime == dotnetProcesses.Min(pr => pr.StartTime)).First() ;
                                 
                                theOneToKill.Kill();
                            }
                            foreach (string file in files)
                            {
                                OnReceiveFiles(file, path);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("Could not stop website!");
                        }
                        site.Start();
                    }
                    else
                    {
                        throw new InvalidOperationException("Could not find website!");
                    }
                }
                else
                {
                    Console.WriteLine("Nothing to transfer!");
                }
               
            }
            Console.ReadLine();
            return true;
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
            .Configure<ApplicationSettings>(configuration);
            
            serviceProvider = serviceCollection.BuildServiceProvider();
            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.AddConsole(configuration.GetSection("Logging"));
            serviceProvider.GetService<ILoggerFactory>().CreateLogger<DeployHelperManager>();
        }



        public static void OnReceiveFiles(string filePathMove, string currentPath)
        {
            Console.WriteLine("Found file " + filePathMove);
            string destinationPath = @"C:\inetpub\wwwroot\TestAspNet";
            FileInfo fileToMove = new FileInfo(filePathMove);
            string dirToAdd = fileToMove.Directory.ToString().Remove(0, currentPath.Length);
            destinationPath = destinationPath + dirToAdd;
            string fullDestinationPath = destinationPath + "\\" + fileToMove.Name;
            WaitForFinishWrite(filePathMove, fullDestinationPath);

            Console.WriteLine("File done writing " + filePathMove + " move to " + fullDestinationPath);
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }
         
            if (File.Exists(fullDestinationPath)) { 
                File.Delete(fullDestinationPath);
            }
            File.Move(filePathMove, fullDestinationPath);
        }
    
        private static void WaitForFinishWrite(string sourcePath, string destinationPath)
        {
            while (true)
            {
                try {
                    using (StreamReader stream = new StreamReader(sourcePath))
                    {

                        

                        break;
                    }

                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
