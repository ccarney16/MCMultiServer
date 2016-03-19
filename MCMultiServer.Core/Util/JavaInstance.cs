using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MCMultiServer.Util {
    public class JavaInstance {
        //Name of the java instance
        public String Name;

        //Path of JRE
        public String JREPath;

        //excutable java
        public String javaExecutable = @"bin\java.exe";
        //unix version
        public String javaExecutableUNIX = "java";

        public Boolean HasJDK = false;

        public String JDKPath;

        public JavaInstance(String name, String JRE) {
            Name = name;
            JREPath = JRE;
        }

        private static Regex jreVer = new Regex(@"java version " + '\u0022' + "" + '\u0022');
        public static Boolean JREExists(String Path) {
            if (Directory.Exists(Path)) {
            }
            return false;
        }

        public static Boolean JDKExists() {
            return false;
        }
    }
}
