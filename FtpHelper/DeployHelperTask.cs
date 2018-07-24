using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Web.Administration;


namespace FtpHelper
{
    public class DeployHelperTask : IMaintenanceTask
    {
        private readonly FolderSettings settings;
        public DeployHelperTask(IOptions<FolderSettings> options)
        {
            settings = options.Value;
        }

        public bool Execute()
        {
            //string path = @"C:\Temp\ftptemp";
            string path = settings.FtpFolder;
            if (Directory.EnumerateFileSystemEntries(path).Any())
            {

                //turn off IIS - transfer files to iis location, turn IIS back on after transfer is done
                IEnumerable<string> files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
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
                            if (dotnetProcesses.Count() > 1)
                            {
                                //This app also uses dotnet, the first one should be the longest running
                                //if the site was down for any other reason, the dotnet running the site 
                                //might have been previously killed do not need to kill it again
                                Process theOneToKill = dotnetProcesses.Where(process => process.StartTime == dotnetProcesses.Min(pr => pr.StartTime)).First();

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

        public void OnReceiveFiles(string filePathMove, string currentPath)
        {
            Console.WriteLine("Found file " + filePathMove);
            //string destinationPath = @"C:\inetpub\wwwroot\TestAspNet";
            string destinationPath = settings.SiteFolder;
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

            if (File.Exists(fullDestinationPath))
            {
                File.Delete(fullDestinationPath);
            }
            File.Move(filePathMove, fullDestinationPath);
        }

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
