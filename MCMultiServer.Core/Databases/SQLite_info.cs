using System;
using System.Diagnostics;
using MCMultiServer.Net;

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
        public override DatabaseFlags[] DBFlags { get { return new DatabaseFlags[] { DatabaseFlags.Query_Support, DatabaseFlags.Json_Support }; } }
    }
}
