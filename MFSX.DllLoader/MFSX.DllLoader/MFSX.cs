using Simple_Injection;
using System.IO;

namespace MFSX.DllLoader
{
    public static class MFSX
    {
        private static readonly Injector Injector = new Injector();

        public static bool Inject(string processName, string dllName, string injectionMethod)
        {
            processName = Path.GetFileNameWithoutExtension(processName);

            bool status = false;

            switch (injectionMethod.ToLower())
            {
                case "1":
                case "createremotethread":

                    if (Injector.CreateRemoteThread(dllName, processName))
                    {
                        status = true;
                    }

                    break;

                case "2":
                case "rtlcreateuserthread":

                    if (Injector.RtlCreateUserThread(dllName, processName))
                    {
                        status = true;
                    }

                    break;

                case "3":
                case "setthreadcontext":

                    if (Injector.SetThreadContext(dllName, processName))
                    {
                        status = true;
                    }

                    break;

                default:
                    return false;
            }

            return status;
        }
    }
}