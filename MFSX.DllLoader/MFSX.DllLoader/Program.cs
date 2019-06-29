using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MFSX.DllLoader
{
    internal class Program
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static readonly char RandomChar = GetRandomCharacter();

        private static bool hideWindow = false;

        private static void Main(string[] args)
        {
            Console.Title = "MFSX Dll Loader v1.1 by Mobile46";

            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = $"{currentPath}{Process.GetCurrentProcess().ProcessName}.mfsx";

                IniFile myIni = new IniFile(filePath);

                if (!File.Exists(filePath))
                {
                    Log($"{Path.GetFileName(filePath)} file is not found!");
                    goto exit;
                }

                hideWindow = StringToBool(myIni.Read("HideWindow"));

                if (hideWindow)
                {
                    ShowWindow(GetConsoleWindow(), 0);
                }

                bool runProcessAsAdmin = StringToBool(myIni.Read("RunProcessAsAdmin"));

                string processName = currentPath + myIni.Read("ProcessName");

                AppendExtension(ref processName, ".exe");

                string dllName = currentPath + myIni.Read("DllName");

                AppendExtension(ref dllName, ".dll");

                if (File.Exists(dllName))
                {
                    LogFileFound(dllName);

                    string injectionMethod = myIni.Read("InjectionMethod");

                    string injectionMethodText;
                    switch (injectionMethod.ToLower())
                    {
                        case "1":
                        case "createremotethread":
                            injectionMethodText = "CreateRemoteThread";
                            break;

                        case "2":
                        case "rtlcreateuserthread":
                            injectionMethodText = "RtlCreateUserThread";
                            break;

                        case "3":
                        case "setthreadcontext":
                            injectionMethodText = "SetThreadContext";
                            break;

                        default:
                            Log("Invalid injection method!");
                            goto exit;
                    }

                    Log($"Injection method: {injectionMethodText}");

                    if (File.Exists(processName))
                    {
                        LogFileFound(processName);

                        if (runProcessAsAdmin)
                        {
                            new Process { StartInfo = new ProcessStartInfo(processName) { Verb = "runas" } }.Start();

                            Log($"{Path.GetFileName(processName)} started as administrator!");
                        }
                        else
                        {
                            Process.Start(processName);

                            Log($"{Path.GetFileName(processName)} started!");
                        }
                    }
                    else
                    {
                        LogFileNotFound(processName);
                        goto exit;
                    }

                    int waitBeforeInjection = 0;

                    if (!int.TryParse(myIni.Read("WaitBeforeInjection"), out waitBeforeInjection))
                    {
                        Log("Invalid WaitBeforeInjection value! It must be a integer (1000 ms == 1 sec)");
                    }

                    if (waitBeforeInjection != 0)
                    {
                        Log($"Waiting {waitBeforeInjection * 0.001} second(s) before injection!");
                        System.Threading.Thread.Sleep(waitBeforeInjection);
                    }

                    Log(MFSX.Inject(processName, dllName, injectionMethod) ? "Dll injection process is successful!" : "Dll injection process is failed!");
                }
                else
                {
                    LogFileNotFound(dllName);
                }
            }
            catch (Exception ex)
            {
                Log($"An error occurred: {ex.Message}!");
            }
            finally
            {
                if (!hideWindow)
                {
                    Log("Press any key to exit...");
                    Console.ReadKey();
                }
            }

            exit:
            Environment.Exit(0);
        }

        private static bool StringToBool(string text)
        {
            switch (text.ToLower())
            {
                case "1":
                case "true":
                    return true;

                default:
                    return false;
            }
        }

        public static char GetRandomCharacter()
        {
            string text = "*-X+#?";
            return text[new Random().Next(text.Length)];
        }

        private static void AppendExtension(ref string text, string suffix)
        {
            if (!text.EndsWith(suffix))
            {
                text += suffix;
            }
        }

        private static void Log(string text)
        {
            if (!hideWindow)
            {
                Console.WriteLine($"[{RandomChar}] {text}");
            }
        }

        private static void LogFileFound(string fileName)
        {
            Log($"{Path.GetFileName(fileName)} file is found!");
        }

        private static void LogFileNotFound(string fileName)
        {
            Log($"{Path.GetFileName(fileName)} file is not found!");
        }
    }
}