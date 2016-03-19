using System;
using System.Threading;
using MCMultiServer.Net;

namespace MCDebug {
    class Program {
        static bool ServiceMode = false;
        static string TITLE = "MCMS Debug Tool";
        static string MOTD = "CSpells & MCMultiServer is licensed under the BSD License. Check the provided 'License.txt' to know more.";

        //static Thread svr = new Thread(new ThreadStart(Session.Start));

        static void Main(string[] args) {

            //Debug Tool Lines
            Console.Title = TITLE;
            ConsoleColor cur = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(MOTD);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("This is a Debug Console, Do not attempt to use this in production!");
            Console.WriteLine();
            Console.ForegroundColor = cur;

            OnLoad();

            ParseArguements(args);
            if (ServiceMode) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("You are now in debug service mode, All commands are disabled to replicate a similar production environment.");
                Console.ForegroundColor = cur;
                Console.WriteLine();
                Console.WriteLine("Feel free to exit anytime by pressing any key");
                Console.ReadLine();
            } else {
                Console.WriteLine("Using complex debug mode, type 'help' to get a list of commands. Type 'exit' to close MCDebug.");
                Console.WriteLine();

                OnCommand();
                //svr.Start();
                

                while (CommandMode.HandleCommand());

                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static void Server_ErrorOutput(string line, Guid serverid) {
            MCMultiServer.Srv.Server srv = Manager.AllServers.GetServer(serverid);
            if (!ServiceMode) {
                if (CommandMode.ActiveServer == srv.ID) {
                    Console.WriteLine(srv.Properties.DisplayName + ": " + line);
                }
            }
        }

        private static void Server_LogOutput(string line, Guid serverid) {
            MCMultiServer.Srv.Server srv = Manager.AllServers.GetServer(serverid);
            if (!ServiceMode) {
                if (CommandMode.ActiveServer == srv.ID) {
                    Console.WriteLine(srv.Properties.DisplayName + ": " + line);
                }
            }
        }

        static void ParseArguements(string[] args) {
            foreach (string a in args) {
                switch (a.ToLower()) {
                    case "-service-mode":
                        ServiceMode = true;
                        break;
                    default:
                        break;
                }
            }
        }

        //Anything that the Program depends on before loading the service or command prompt
        static bool OnLoad() {

            return true;
        }

        //Starts the Command Line
        static bool OnCommand() {
            Commands.Command.Init();
            Manager.Init();
            MCMultiServer.Srv.Server.LogOutput += Server_LogOutput;
            MCMultiServer.Srv.Server.ErrorOutput += Server_ErrorOutput;
            return true;
        }

        static void OnQuit() {

        }
    }
}
