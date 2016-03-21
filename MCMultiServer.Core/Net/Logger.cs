using System;
using System.IO;

//Provided from MCSharp, modified for use in MCMultiServer, Licensed under the MIT License.
namespace MCMultiServer.Net {
    public static class Logger {
        const string LogDirectory = "logs/";
        const string LogFileName = "mcms";

        const string TimeFormat = "hh:mm:ss tt";

        static string shortDateTime = "dd/MM/yyyy hh:mm:ss tt";
        static string longDateTime = "dddd, MMMM dd, yyyy - hh:mm tt (K)";

        static string SessionStart = DateTime.Now.ToString(longDateTime);
        static LogType errorTypes;

        public delegate void LogHandler(string message, LogType type);
        public static event LogHandler OnLog;

        static Logger() {
            if (!Directory.Exists(LogDirectory)) {
                Directory.CreateDirectory(LogDirectory);
            }

            // Get our log files ready
            PrepareLogFile(LogFileName);

            errorTypes |= LogType.Error;

            // Add our file writer to the log
            OnLog += HandleLog;
        }

        private static void HandleLog(string errorMessage, LogType type) {
            // Format our output string
            string message = "[" + DateTime.Now.ToString(shortDateTime) + "]"
                           + "[" + Enum.GetName(typeof(LogType), type) + "]"
                           + " - " + errorMessage;

            WriteLog(message, LogFileName);
        }

        private static void WriteLog(string message, string filename) {
            bool appendHeader = false;
            string today = DateTime.Now.ToString("-MM-dd-yyyy");
            TextWriter logStream = null;

            try {
                // If we're on a new day, append a header to the file
                if (!File.Exists(LogDirectory + filename + today + ".log")) {
                    appendHeader = true;
                }

                // Open the log file
                logStream = new StreamWriter(LogDirectory + filename + today + ".log", true);

                // Append the header if its a new day
                if (appendHeader) {
                    // Add session header
                    logStream.WriteLine("==================== Session Continued ==================");
                    logStream.WriteLine(SessionStart);
                    logStream.WriteLine("=======================================================");
                }


                // Write contents
                logStream.WriteLine(message);
            } catch { } finally {
                if (logStream != null) {
                    logStream.Close();
                }
            }
        }

        internal static void PrepareLogFile(string filename) {
            string today = DateTime.Now.ToString("-MM-dd-yyyy");
            TextWriter logStream = null;

            try {
                // Open the log file
                logStream = new StreamWriter(LogDirectory + filename + today + ".log", true);

                // Add session header
                logStream.WriteLine("==================== Session Start ====================");
                logStream.WriteLine(SessionStart);
                logStream.WriteLine("=======================================================");
            } finally {
                // Close the log file no matter what happens
                if (logStream != null) {
                    logStream.Close();
                }
            }
        }


        public static void Write(string message) {
            Write(LogType.Info, message);
        }

        
        public static void Write(LogType type, string message) {
            if (OnLog != null) {
                OnLog(message, type);
            }
        }

        public static void Write(LogType type, string message, params object[] values) {
            Write(type, String.Format(message, values));
        }
    }
}