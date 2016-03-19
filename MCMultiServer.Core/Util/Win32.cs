using System;
using Microsoft.Win32;

//Wont need to worry much about Windows since things are 99% there by default.
namespace MCMultiServer.Util {
    /// <summary>
    /// Win32 Version of utility functions, using registry. 
    /// </summary>
    public class Win32 {
        #region --JAVA CHECK--
        //I do not recommend using 1.6 and back
        public static String[] ValidJREVer = { "1.7", "1.8" };

        public static Boolean IsValidJRE(String ver) {
            for (int i = 0; i < ValidJREVer.Length; i++) { if (ValidJREVer[i] == ver) { return true; } }
            return false;
        }
        //java path
        private static String _regjre = @"SOFTWARE\JavaSoft\Java Runtime Environment";

        //For x64 systems with 32 bit java installed.
        private static String _regjrex64 = @"SOFTWARE\Wow6432Node\JavaSoft\Java Runtime Environment";

        //Currently does not do anything at the moment.
        public static Boolean HasInstalledJava() {
            RegistryKey reg = Registry.LocalMachine;
            RegistryKey subkey = reg.OpenSubKey(_regjre);
            if (subkey != null) return true; 
            else {
                subkey = reg.OpenSubKey(_regjrex64);
                if (subkey != null) return true;
            }
            return false;
        }

        public static String CheckVersion() {
            RegistryKey reg = Registry.LocalMachine;
            RegistryKey subKey = reg.OpenSubKey(_regjre);
            if (subKey != null) return subKey.GetValue("CurrentVersion").ToString();
            else {
                subKey = reg.OpenSubKey(_regjrex64);
                if (subKey != null) return subKey.GetValue("CurrentVersion").ToString(); 
            }
            return null;
        }

        public static String JREPath() {
            RegistryKey reg = Registry.LocalMachine;
            RegistryKey subKey = reg.OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment\" + CheckVersion());
            if (subKey != null) return subKey.GetValue("JavaHome").ToString();
            return null;
        }
        #endregion 
    }
}
