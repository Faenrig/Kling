using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

// An extract of code I made for my other project ADK https://androdevkit.github.io
namespace Adb_gui_Apkbox_plugin
{
    public class IniFile
    {
        private string _Path;
        private string _Exe = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniFile(string IniPath = null)
        {
            _Path = new FileInfo(IniPath ?? _Exe + ".ini").FullName.ToString();
        }

        public string Read(string Key, string Section = null)
        {
            var returnValue = new StringBuilder(255);
            GetPrivateProfileString(Section ?? _Exe, Key, "", returnValue, 255, _Path);
            return returnValue.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? _Exe, Key, Value, _Path);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? _Exe);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? _Exe);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}