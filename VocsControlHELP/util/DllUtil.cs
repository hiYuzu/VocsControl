using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VocsControlHELP.util
{
    public class DllUtil
    {
        #region 操作配置文件
        //声明方法
        public static int WriteConfigFile(string section, string key, string val, string filePath)
        {
            return NativeMethods.WritePrivateProfileString(section, key, val, filePath);
        }
        public static int ReadConfigFile(string section, string key, string def, StringBuilder retVal, int size, string filePath)
        {
            return NativeMethods.GetPrivateProfileString(section, key, def, retVal, size, filePath);
        }

        #endregion

        #region 加载dll
        internal static class NativeMethods
        {
            [DllImport("Kernel32", CharSet = CharSet.Unicode)]
            public static extern int LoadLibrary(String funcname);
            [DllImport("kernel32")]
            public static extern int WritePrivateProfileString(string section, string key, string val, string filePath);
            [DllImport("kernel32")]
            public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        }
        #endregion
    }
}
