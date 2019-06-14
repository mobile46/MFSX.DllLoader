using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MFSX.DllLoader
{
    internal class IniFile
    {
        private readonly string _path;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string Default, StringBuilder retVal, int size, string filePath);

        public IniFile(string iniPath = null)
        {
            _path = new FileInfo(iniPath).FullName;
        }

        public string Read(string key)
        {
            var retVal = new StringBuilder(255);
            GetPrivateProfileString("Settings", key, "", retVal, 255, _path);
            return retVal.ToString();
        }

        public void Write(string key, string value)
        {
            WritePrivateProfileString("Settings", key, value, _path);
        }

        public bool KeyExists(string key)
        {
            return Read(key).Length > 0;
        }
    }
}