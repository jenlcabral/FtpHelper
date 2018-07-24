using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Web.Administration;
using Microsoft.Extensions.Logging;

namespace FtpHelper
{
    public class DeployHelperTask : IMaintenanceTask
    {
        private readonly SiteSettings settings;
        IServiceContext context;

        public DeployHelperTask(IOptions<SiteSettings> options, IServiceContext context)
        {
            settings = options.Value;
            this.context = context;
        }

        public bool Execute()
        {
            string path = settings.FtpFolder;
            if (Directory.EnumerateFileSystemEntries(path).Any())
            {
                IEnumerable<string> files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
                if (files.Count() > 0)
                {
                    //turn off IIS - transfer files to iis location, turn IIS back on after transfer is done
                    context.Logger.LogInformation("There are files to transfer");
                    ServerManager server = new ServerManager();
                    Site site = server.Sites.FirstOrDefault(s => s.Name == settings.WebsiteName);
                    if (site != null)
                    {
                        context.Logger.LogInformation("turning off IIS");
                        site.Stop();
                        if (site.State == ObjectState.Stopped)
                        {
                            IEnumerable<Process> dotnetProcesses = Process.GetProcesses().Where(pr => pr.ProcessName == "dotnet");
                            if (dotnetProcesses.Count() > 1)
                            {
                                //This app also uses dotnet, the first one should be the longest running
                                //if the site was down for any other reason, the dotnet running the site 
                                //might have been previously killed do not need to kill it again
                                Process theOneToKill = dotnetProcesses.Where(process => process.StartTime == dotnetProcesses.Min(pr => pr.StartTime)).First();
                                theOneToKill.Kill();
                            }
                            context.Logger.LogInformation("moving files");
                            foreach (string file in files)
                            {
                                OnReceiveFiles(file, path);
                            }
                        }
                        else
                        {
                            Exception exception = new InvalidOperationException("Could not stop website!");
                            context.Logger.LogError("IIS Error", exception);
                        }
                        site.Start();
                    }
                    else
                    {
                        Exception exception = new InvalidOperationException("Could not find website!");
                        context.Logger.LogError("Site Name Error", exception);
                    }
                }
                else
                {
                    context.Logger.LogInformation("Nothing to transfer!");
                }
            }
            return true;
        }

        public void OnReceiveFiles(string filePathMove, string currentPath)
        {
            string destinationPath = settings.SiteFolder;
            FileInfo fileToMove = new FileInfo(filePathMove);
            string dirToAdd = fileToMove.Directory.ToString().Remove(0, currentPath.Length);
            destinationPath = destinationPath + dirToAdd;
            string fullDestinationPath = destinationPath + "\\" + fileToMove.Name;
            WaitForFinishWrite(filePathMove, fullDestinationPath);

            context.Logger.LogInformation("File done writing " + filePathMove + " move to " + fullDestinationPath);
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            if (File.Exists(fullDestinationPath))
            {
                File.Delete(fullDestinationPath);
            }
            File.Move(filePathMove, fullDestinationPath);
        }

        // Function to wait until a file is done being written before it will attempt to move the file
        private static void WaitForFinishWrite(string sourcePath, string destinationPath)
        {
            while (true)
            {
                try
                {
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
