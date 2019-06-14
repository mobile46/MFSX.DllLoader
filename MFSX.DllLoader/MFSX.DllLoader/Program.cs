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

        private const int SW_HIDE = 0;
        private static readonly char RandomChar = GetRandomCharacter();

        private static void Main(string[] args)
        {
            Console.Title = "MFSX Dll Loader v1.0 by Mobile46";
            bool hideWindow = false;

            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = $"{currentPath}{Process.GetCurrentProcess().ProcessName}.mfsx";

                IniFile myIni = new IniFile(filePath);

                if (!myIni.KeyExists("ProcessName"))
                {
                    myIni.Write("ProcessName", "fileName.exe");
                }

                if (!myIni.KeyExists("DllName"))
                {
                    myIni.Write("DllName", "version.dll");
                }

                if (!myIni.KeyExists("InjectionMethod"))
                {
                    myIni.Write("InjectionMethod", "1");
                }

                if (!myIni.KeyExists("RunProcessAsAdmin"))
                {
                    myIni.Write("RunProcessAsAdmin", "0");
                }

                if (!myIni.KeyExists("HideWindow"))
                {
                    myIni.Write("HideWindow", "0");
                }

                if (!File.Exists(filePath))
                {
                    Log($"{Path.GetFileName(filePath)} file is not found!");
                    goto exit;
                }

                string hideWindowText = myIni.Read("HideWindow");

                StringToBool(hideWindowText, ref hideWindow);

                if (hideWindow)
                {
                    var handle = GetConsoleWindow();
                    ShowWindow(handle, SW_HIDE);
                }

                string runProcessAsAdminText = myIni.Read("ProcessName");

                bool runProcessAsAdmin = false;

                StringToBool(runProcessAsAdminText, ref runProcessAsAdmin);

                string processName = currentPath + myIni.Read("ProcessName");

                if (!processName.EndsWith(".exe"))
                {
                    processName += ".exe";
                }

                string dllName = currentPath + myIni.Read("DllName");

                if (!dllName.EndsWith(".dll"))
                {
                    dllName += ".dll";
                }

                if (File.Exists(dllName))
                {
                    LogFileFound(dllName);

                    string injectionMethod = myIni.Read("InjectionMethod");

                    string injectionMethodText;
                    switch (injectionMethod)
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
                            Process proc = new Process
                            {
                                StartInfo = { FileName = filePath, UseShellExecute = true, Verb = "runas" }
                            };
                            proc.Start();

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
                    Log("Press any key to continue...");
                    Console.ReadKey();
                }
            }

        exit:
            Environment.Exit(0);
        }

        private static void StringToBool(string text, ref bool value)
        {
            switch (text.ToLower())
            {
                case "1":
                case "true":
                    value = true;
                    break;

                default:
                    value = false;
                    break;
            }
        }

        public static char GetRandomCharacter()
        {
            string text = "*-X+#?";
            return text[new Random().Next(text.Length)];
        }

        private static void Log(string text)
        {
            Console.WriteLine($"[{RandomChar}] {text}");
        }

        private static void LogFileNotFound(string fileName)
        {
            Log($"{Path.GetFileName(fileName)} file is not found!");
        }

        private static void LogFileFound(string fileName)
        {
            Log($"{Path.GetFileName(fileName)} file is found!");
        }
    }
}