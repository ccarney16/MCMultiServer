using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;

//Basic Logging Class, this needs to be rewritten.
namespace MCMultiServer.Net {

    //Basic Log Types, will extend later.
    public enum LogType : byte {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    public static class Logger {
        private static string formatTime = "MM/dd/yyyy H:mm:ss zzz";
        private static string dateTime { get { return (DateTime.Now.ToString(formatTime)); } }
        private static List<string> RecentLog = new List<string>();

        private static Timer flushTimer = new Timer(10000);

        public static void Init() {
            flushTimer.Elapsed += FlushTimer_Elapsed;
        }

        private static void FlushTimer_Elapsed(object sender, ElapsedEventArgs e) {
            FlushToTextFile(RecentLog);
        }

        public static void Write(LogType type, string message, params object[] values) { Write(type, String.Format(message, values)); }
        public static void Write(LogType type, string message) {
            WriteToConsole(type, message);
            string LogMessage = ConstructLogMessage(type, message);
            UpdateLog(LogMessage);
        }

        //Used for debugging
        private static void WriteToConsole(LogType type, string message) {
            Console.WriteLine("[" + DateTime.Now.ToString(formatTime) + "] " + message);
        }

        private static string ConstructLogMessage(LogType type, string message) {
            string LogMessage = "[" + DateTime.Now.ToString(formatTime) + "]"
                               + "ID=" + type.ToString() +
                               " : " + message;

            return LogMessage;
        }

        /* This method updates the Log List based on the Write() method */
        private static void UpdateLog(string message) {
            RecentLog.Add(message);
            //WriteToTextFile(RecentLog);
        }

        /* File writing */

        // takes a single message and writes to a text file
        private static void FlushToTextFile(string msg) {
            string path = @"log\TempName.txt"; // Temporary

            using (StreamWriter sw = new StreamWriter(path)) {
                sw.WriteLine(msg);
            }
        }

        //overload to allow a list to be input instead of a single message
        private static void FlushToTextFile(List<string> log) {
            string path = @"log\TempName.txt"; // Temporary
            using (StreamWriter sw = new StreamWriter(path)) {
                foreach (string msg in log) {
                    sw.WriteLine(msg);
                }
            }
        }
    }

}