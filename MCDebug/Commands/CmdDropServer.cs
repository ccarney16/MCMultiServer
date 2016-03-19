using System;

using MCMultiServer.Net;
using MCMultiServer.Srv;
namespace MCDebug.Commands {
    class CmdDropServer : Command {
        public override string Description {
            get {
                return "deletes a server from the manager and database";
            }
        }

        public override string Help {
            get {
                return "[name] <rmdir>";
            }
        }

        public override string Name {
            get {
                return "drop-server";
            }
        }

        public override void Use(string args) {
            if (args == null || args == string.Empty) {
                Console.WriteLine("not enough args");
                return;
            }

            string[] a = args.Split(' ');
            bool inDB = false;

            Server srv = null;
            foreach (Server s in Manager.AllServers) {
                if (s.DisplayName.ToLower() == a[0].ToLower()) {
                    srv = s;
                    break;
                }
            }

            if (srv == null) {
                Console.WriteLine("Server is not loaded, checking database...");
                ServerProperties[] proplist = Database.GetAllServers();

                foreach (ServerProperties prop in proplist) {
                    if (prop.DisplayName.ToLower() == a[0]) {
                        srv = new Server();
                        srv.Load(Paths.ServerDirectory + "/" + prop.ServerID, prop);
                        inDB = true;
                        break;
                    }
                }
            } else {
                ServerProperties prop = Database.GetServerProperties(srv.ID);
                if (prop != null) {
                    inDB = true;
                } else {
                    Console.WriteLine("Server does not exist in the database! (was this loaded correctly?)");
                }
            }

            //attempt two, this time kill the command if server is STILL not there
            if (srv == null) {
                Console.WriteLine("Server does not exist");
                return;
            }

            Console.Write("Are you sure you want to delete '{0}'? [y/N] ", srv.DisplayName);
            string prompt = Console.ReadLine();

            if (srv.IsRunning) {
                Console.WriteLine("Error, server is still running, please stop this server to continue");
                return;
            }

            if (prompt.ToLower().StartsWith("y")) {
                if (a[1].ToLower() == "rmdir") {
                    try {
                        System.IO.Directory.Delete(srv.RootDirectory, true);
                    } catch (System.IO.IOException e) {
                        Console.WriteLine(e.HResult);
                        Console.WriteLine(e.Message);
                        return;
                    }
                } else {
                    System.IO.Directory.Move(srv.RootDirectory, srv.RootDirectory + "." + srv.DisplayName);
                }

                if (inDB != false) {
                    Database.DropServer(srv.ID);
                }

                Manager.RemoveServer(srv.ID);
                srv.Dispose();
            } else {
                Console.WriteLine("Aborted");
            }
        }
    }
}
