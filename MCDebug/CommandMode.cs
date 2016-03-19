using System;

using MCMultiServer.Net;
namespace MCDebug {
    class CommandMode {
        public static Guid ActiveServer;

        //Handle all commands.
        public static bool HandleCommand() {
            Console.Write("#> ");
            string con = Console.ReadLine();
            if (con != "exit") return RunCommand(con);
            //Program specific code goes here...
            Manager.StopAllServers();
            return false;
        }

        //Sure, lets go with another Bool. 
        static bool RunCommand(string cmd) {
            if (cmd == null || cmd == String.Empty) { return true; }
            string[] split = cmd.Split(' ');
            string args = null;
            if (split.Length >= 2) {
                for (int i = 1; i < split.Length; i++) {
                    args += split[i] + " ";
                }
            }
            Commands.Command.cmd.ExecuteCommand(split[0], args);
            return true;
        }

        public static void ConsoleInput() {
            while (true) {
                string cmd = Console.ReadLine();
                if (cmd == "!exit") { break; }
                Manager.AllServers.GetServer(ActiveServer).SendInput(cmd);
            }
        }
    }
}
