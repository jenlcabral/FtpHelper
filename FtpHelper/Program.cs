using System;
using System.IO;
using System.Threading;

namespace FtpHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(@"C:\Temp\ftpfolder");
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.Created += new FileSystemEventHandler(OnReceiveFiles);
            fileSystemWatcher.EnableRaisingEvents = true;
            Console.WriteLine("Restarting IIS");
            Console.ReadLine();


        }

        public static void OnReceiveFiles(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Found file " + e.FullPath);
            string destinationPath = @"C:\Temp\movedToFolder";
            FileInfo fileToMove = new FileInfo(e.FullPath);
            string fullDestinationPath = destinationPath + "\\"+ fileToMove.Name;
            WaitForFinishWrite(e.FullPath, fullDestinationPath);

            Console.WriteLine("File done writing " + e.FullPath + " move to " + fullDestinationPath);
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }
            if (File.Exists(fullDestinationPath))
                File.Delete(fullDestinationPath);
            File.Move(e.FullPath, fullDestinationPath);
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
