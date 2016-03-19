using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MCMultiServer.Net;
using MCMultiServer.Srv;
namespace MCDebug.Commands {
    class CmdListServer : Command {
        public override string Description {
            get {
                return "returns a list of online servers";
            }
        }

        public override string Help {
            get {
                return null;
            }
        }

        public override string Name {
            get {
                return "list-servers";
            }
        }

        public override void Use(string args) {

            if (Manager.AllServers.Count == 0) {
                Console.WriteLine("No Servers loaded. (Please check database to verify that no servers have been created)");
                return;
            }

            foreach (Server srv in Manager.AllServers) {
                Command.cmd.ExecuteCommand("server", srv.DisplayName + " info");
            }
        }
    }
}
