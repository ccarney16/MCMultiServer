using System;
using System.IO;

namespace MCMultiServer.Net {
    public static class Paths {

        public const String ConfigurationFile = "mcms.conf";

        //Main Config Directories
        public static String ConfigDirectory = Environment.CurrentDirectory;
        public static String DataDirectory = Environment.CurrentDirectory;

        //Ever changing Data
        public static String ServerDirectory { get { return DataDirectory + "/servers"; } }
        public static String BackupDirectory { get { return DataDirectory + "/backup"; } }
        public static String DatabaseDirectory { get { return DataDirectory + "/db"; } }
        public static String JarDirectory { get { return DataDirectory + "/jar"; } }

        //java instance for a node.
        public static Util.JavaInstance JVMInstance;

        public static void CheckDirectories() {
            //Main Directories, This is useful for new servers.
            if (!Directory.Exists(DataDirectory)) Directory.CreateDirectory(DataDirectory);

            //Other Directories
            if (!Directory.Exists(ServerDirectory)) Directory.CreateDirectory(ServerDirectory);
            if (!Directory.Exists(JarDirectory)) Directory.CreateDirectory(JarDirectory);
            if (!Directory.Exists(DatabaseDirectory)) Directory.CreateDirectory(DatabaseDirectory);
        }
    }
}
