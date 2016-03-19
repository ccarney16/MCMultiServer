using System;
using System.Diagnostics;

namespace MCMultiServer.Databases {
    //Basic SQLite Information
    public class SQLite_info : Net.dbinfo {
        public override String Name {
            get {
                return "MCMultiServer Internal SQLite";
            }
        }

        //Always go with the Assembly Version
        public override String Version {
            get {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.FileVersion;
            }
        }
    }
}
