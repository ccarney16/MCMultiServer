using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMultiServer.Util {
    /// <summary>
    /// unix version of utility functions
    /// </summary>
    class Unix {
        public static String[] ValidJREVer = { "1.7", "1.8" };

        //assume true for now. 
        public static Boolean HasInstalledJava() {
            return true;
        }

        //not too sure about mono now.
        public static String CheckVersion() {
            return null;
        }

        //lazy way out. might support ubuntu
        public static String JREPath() {
            return "/usr/bin/java";
        }
    }
}
