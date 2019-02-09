namespace UFBLauncher
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Properties;

    internal class Program
    {
        private const string ExeExt = ".exe";
        private const string DefaultName = "UltimateFishBot.exe";

        private static void Main(string[] args)
        {
            var lastName = Settings.Default.UFBName;

            if (RunBotSecretly(lastName) == false)
            {
                RunBotSecretly(DefaultName);
            }
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowText(IntPtr ptr, string text);

        private static bool RunBotSecretly(string name)
        {
            var assembly = Assembly.GetEntryAssembly();
            var assemblyPath = Path.GetDirectoryName(assembly.Location);

            Debug.Assert(string.IsNullOrWhiteSpace(assemblyPath) == false, "Requires assembly path");

            var currentPath = Path.Combine(assemblyPath, name);
            var ufbExeFile = new FileInfo(currentPath);

            if (ufbExeFile.Exists)
            {
                var path = Path.GetDirectoryName(currentPath);

                Debug.Assert(string.IsNullOrWhiteSpace(path) == false, "Path required");

                var newName = StringUtils.RandomString(12) + ExeExt;
                var newPath = Path.Combine(path, newName);
                ufbExeFile.MoveTo(newPath);

                Settings.Default.UFBName = newName;
                Settings.Default.Save();

                var process = Process.Start(ufbExeFile.FullName);

                Debug.Assert(process != null, "Failed to start process");

                Thread.Sleep(1500);
                SetWindowText(process.MainWindowHandle, newName);
                return true;
            }

            return false;
        }
    }
}
