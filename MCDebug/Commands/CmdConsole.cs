using System;
using System.Collections.Generic;

using MCMultiServer.Net;
using MCMultiServer.Srv;
using MCMultiServer.Util;
namespace MCDebug.Commands {
    public class CmdConsole : Command {
        public override string Name { get { return "console"; } }
        public override string Description { get { return "Set Active Server console"; } }
        public override string Help { get { return "[server]"; } }

        public override void Use(string args) {
            if (args == null || args == String.Empty) {
                base.ReturnHelp(Help);
            }

            
            Server srv = Func.GetServerbyName(args.TrimEnd(' '));

            if (srv == null) {
                Console.WriteLine("Server does not exists");
                return;
            }

            CommandMode.ActiveServer = srv.ID;

            Console.WriteLine();
            Console.WriteLine("Entering Console Mode for {0}", srv.DisplayName);
            Console.WriteLine("To exit, type '!exit'");
            Console.WriteLine();

            CommandMode.ConsoleInput();

            CommandMode.ActiveServer = Guid.Empty;
            Console.WriteLine();
            Console.WriteLine("Console Closed");
        }
    }
}
