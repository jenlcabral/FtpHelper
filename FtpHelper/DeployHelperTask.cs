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
            string mask = settings.FlagFileName;
            if (Directory.GetFiles(path,mask,SearchOption.AllDirectories).Any())
            {
                IEnumerable<string> files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
                if (files.Count() > 0)
                {
                    //turn off IIS - transfer files to iis location, turn IIS back on after transfer is done
                    context.Logger.LogInformation("There are files to transfer in folder {a}", path);
                    ServerManager server = new ServerManager();
                    Site site = server.Sites.FirstOrDefault(s => s.Name == settings.WebsiteName);
                    if (site != null)
                    {
                        context.Logger.LogInformation("Stopping IIS for site {a}", site.Name);
                        site.Stop();
                        if (site.State == ObjectState.Stopped)
                        {
                            IEnumerable<Process> dotnetProcesses = Process.GetProcesses().Where(pr => pr.ProcessName == "dotnet");
                            if (dotnetProcesses.Count() > 1)
                            {
                                //This app also uses dotnet, the website one should be the longest running
                                //if the site was down for any other reason, the dotnet running the site 
                                //might have been previously killed do not need to kill it again, also
                                //occasionally taking down the site with site.Stop(), will kill dotnet, 
                                //occasionally it will not 
                                Process theOneToKill = dotnetProcesses.Where(process => process.StartTime == dotnetProcesses.Min(pr => pr.StartTime)).First();
                                theOneToKill.Kill();
                            }
                            context.Logger.LogInformation("Transferring files from folder {a} to {b}", path, settings.SiteFolder);
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
                        context.Logger.LogInformation("Restarting site: {a}", site.Name);
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
            context.Logger.LogInformation("App completed");
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
