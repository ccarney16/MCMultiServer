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
            bool WaitUntilClosed = true;
            do {
                if (Manager.AllServers.Count == 0) {
                    WaitUntilClosed = false;
                }
                foreach (MCMultiServer.Srv.Server s in Manager.AllServers) {
                    if (s.IsRunning) {
                        WaitUntilClosed = true;
                        break;
                    } else {
                        WaitUntilClosed = false;
                    }
                }
                System.Threading.Thread.Sleep(500);
            } while (WaitUntilClosed);
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
