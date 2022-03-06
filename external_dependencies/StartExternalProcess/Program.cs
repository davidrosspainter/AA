using System;
using System.Diagnostics;
using System.IO;

namespace StartExternalProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string entryAssembleyLocation = System.Reflection.Assembly.GetEntryAssembly().Location.Replace(@"\StartExternalProcess.exe", "");

            Console.WriteLine("currentDirectory:{0}", currentDirectory);
            Console.WriteLine("entryAssembleyLocation:{0}", entryAssembleyLocation);

            if (currentDirectory != entryAssembleyLocation)
            {
                Console.WriteLine("set current directory to entryAssembleyLocation");
                Directory.SetCurrentDirectory(entryAssembleyLocation);
            }
            else
            {
                Console.WriteLine("current path is already at the entryAssembleyLocation");
            }

            string[] lines = File.ReadAllLines(@"command.txt");

            string command = lines[0];

            if(command == "restart") // needs work?!
            {
                Console.WriteLine("restart");
                SendCommandPromptHidden(@"taskkill /F /IM buffer.exe /T");
                SendCommandPromptHidden(@"taskkill /IM cmd.exe");
            }
            else
            {
                Console.WriteLine(command);
                Process.Start("CMD.exe", command);
            }
            //Console.ReadLine();
        }

        public static void SendCommandPromptHidden(string argument)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = argument;
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}
