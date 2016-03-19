using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MCMultiServer.Net;
using MCMultiServer.Srv;

namespace MCDebug.Commands {
    class CmdNewServer : Command {
        public override string Description {
            get {
                return "Creates a new Server";
            }
        }

        public override string Help {
            get {
                return "[name]";
            }
        }

        public override string Name {
            get {
                return "new-server";
            }
        }

        public override void Use(string args) {
            if (args == null || args == string.Empty) {
                Console.WriteLine("not enough args");
                return;
            }

            string[] split = args.Split(' ');

            if (split.Length < 2) {
                Console.WriteLine("not enough args");
                return;
            }

            //if server exists or not
            foreach (ServerProperties prop in Database.GetAllServers()) {
                if (prop.DisplayName.ToLower() == split[0].ToLower()) {
                    Console.WriteLine("You cannot use this name in debug mode");
                    return;
                }
            }

            Guid id = Manager.CreateNewServerID();
            Manager.CreateServer(split[0], id);
            Manager.AddServer(id);
        }
    }
}
