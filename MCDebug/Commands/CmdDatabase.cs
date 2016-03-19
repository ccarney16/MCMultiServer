using System;

using MCMultiServer.Net;
using MCMultiServer.Srv;
namespace MCDebug.Commands {
    class CmdDatabase : Command {

        public override string Name { get { return "database"; } }
        public override string Description { get { return "Grabs Database information"; } }

        public override string Help { get { return "[info | view <server|user> index]"; } }

        public override void Use(string args) {
            if (args == null || args == String.Empty) {
                base.ReturnHelp(this.Help);
                return;
            }

            string[] split = args.Split(' ');

            if (split.Length > 0) {
                switch(split[0]) {
                    case "info":
                        Console.WriteLine();
                        Console.WriteLine("-- Database Information [General] --");
                        Console.WriteLine();
                        Console.WriteLine("Plugin Name: " + Database.GetName());
                        Console.WriteLine("Database Namespace: " + MCMultiServer.Net.Database.DBType);
                        Console.WriteLine("Version: {0}", Database.GetVersion());
                        break;
                    case "view":
                        if (split.Length > 1) {
                            switch (split[1]) {
                                case "user":
                                    Console.WriteLine("Not Implemented");
                                    break;
                                case "server":
                                    Console.WriteLine();
                                    Console.WriteLine("-- Database Information [Server] --");
                                    foreach (ServerProperties prop in Database.GetAllServers()) {
                                        Console.WriteLine();
                                        Console.WriteLine("NAME: {0}", prop.DisplayName);
                                        Console.WriteLine("ID: {0}", prop.ServerID);
                                        Console.WriteLine("SETTINGS: {0}", Newtonsoft.Json.JsonConvert.SerializeObject(prop).ToString());
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
        }
    }
}
