using System;
using System.IO;

using MCMultiServer.Net;
using MCMultiServer.Srv;

using Newtonsoft.Json;
namespace MCDebug.Commands {
    //Main Command for the debug tool
    class CmdOldServer : Command {
        public override string Name { get { return "old-server"; } }
        public override string Help { get { return "<start|stop|create|load|delete|list> [NAME]"; } }
        public override string Description { get { return "manages a server, DO NOT USE!"; } }

        //Currently does not use a database
        public override void Use(string args) {
            if (args == string.Empty || args == null) {
                base.ReturnHelp(this.Help);
                return; }
            string[] a = args.Split(' ');
            switch (a[0]) {
                case "start":
                    foreach (MCMultiServer.Srv.ServerProperties prop in Database.GetAllServers()) {
                        if (prop.DisplayName == a[1]) {
                            if (Manager.AllServers.Exists(prop.ServerID)) {
                                Server serv = Manager.AllServers.GetServer(prop.ServerID);
                                if (serv.IsRunning == false) {
                                    //try to add the file in.
                                    if (!File.Exists(serv.RootDirectory + @"\" + serv.Properties.JarFile)) {
                                        if (serv.Properties.Type == ServerType.Minecraft) {
                                            try {
                                                File.Copy(Paths.DataDirectory + @"\jar\" + serv.Properties.JarFile,
                                                    serv.RootDirectory + @"\" + serv.Properties.JarFile);
                                            } catch {
                                                Console.WriteLine("unable to detect version of this server, resorting to latest");
                                                serv.Properties.Type = ServerType.Minecraft;
                                                serv.Properties.JarFile = "minecraft_server." + MCMultiServer.Util.JarManager._releases.LatestReleases.release + ".jar";
                                                serv.Properties.MCVersion = MCMultiServer.Util.JarManager._releases.LatestReleases.release;

                                                string jsonconv = Newtonsoft.Json.JsonConvert.SerializeObject(serv.Properties);
                                                Database.ChangeServerSettings(serv.ID, serv.Properties);
                                                File.Copy(Paths.DataDirectory + @"\jar\" + serv.Properties.JarFile,
                                                     serv.RootDirectory + @"\" + serv.Properties.JarFile);
                                            }
                                        }
                                    }

                                    if (Manager.StartServer(prop.ServerID))
                                        Console.WriteLine("server started");
                                    else
                                        Console.WriteLine("Manager has encounted an error");
                                } else {
                                    Console.WriteLine("server is already running!");
                                }
                            }
                            return;
                        }
                    }
                    Console.WriteLine("server could not be found");
                    break;
                case "stop":
                    foreach (MCMultiServer.Srv.ServerProperties prop in Database.GetAllServers()) {
                        if (prop.DisplayName == a[1]) {
                            if (Manager.StopServer(prop.ServerID)) {
                                Logger.Write(LogType.Info, "server stopped");
                            }
                            return;
                        }
                    }
                    break;
                case "load":
                    foreach (MCMultiServer.Srv.ServerProperties prop in Database.GetAllServers()) {
                        if (prop.DisplayName == a[1]) {
                            if (Manager.AllServers.Exists(prop.ServerID)) {
                                Logger.Write(LogType.Error, "server is already loaded");
                                return;
                            }
                            if (Manager.AddServer(prop.ServerID)) { Logger.Write(LogType.Info, "Server {0} with an id of '{1}' was loaded", a[1].ToString(), prop.ServerID); }
                            return;
                        }
                    }
                    break;
                case "create":
                    foreach (ServerProperties prop in Database.GetAllServers()) {
                        if (prop.DisplayName.ToLower() == a[1].ToLower()) {
                            Console.WriteLine("You cannot use this name in debug mode");
                            return;
                        }
                    }
                    Manager.CreateServer(a[1], Manager.CreateNewServerID());
                    break;
                case "delete":
                    foreach (ServerProperties prop in Database.GetAllServers()) {
                        if (a[1] == prop.DisplayName) {
                            if (Manager.AllServers.Exists(prop.ServerID)) {
                                Server serv = Manager.AllServers.GetServer(prop.ServerID);
                                serv.Shutdown();
                                Manager.AllServers.Remove(serv);
                            }
                            Database.DropServer(prop.ServerID);
                            break;
                        }
                    }
                    break;
                case "list":
                    if (Manager.AllServers.Count == 0) { Console.WriteLine("No Servers Loaded or Running"); return; }
                    foreach (Server server in Manager.AllServers) {
                        Console.Write("Name:" + server.Properties.DisplayName + ";");
                        Console.Write("ID:" + server.ID + ";");
                        Console.Write("MC Version:{0};", server.Properties.MCVersion);
                        Console.Write("Jar File:{0};", server.Properties.JarFile);
                        Console.Write("Root Directory:{0};", server.RootDirectory);
                        Console.Write("Running:" + server.IsRunning.ToString().ToUpper());
                        Console.WriteLine();

                    }
                    break;
                case "setversion":
                    if (!MCMultiServer.Util.JarManager.VersionExists(a[2])) {
                        Console.WriteLine("version {0} does not exist", a[2]);
                        return;
                    }

                    foreach (Server server in Manager.AllServers) {
                        if (server.DisplayName.ToLower() == a[1].ToLower()) {
                            server.Properties.MCVersion = a[2];
                            Database.ChangeServerSettings(server.ID, server.Properties);
                        }
                    }
                    break;
                case "restart":
                    bool found = false;
                    foreach (Server server in Manager.AllServers) {
                        if (server.DisplayName.ToLower() == a[1].ToLower()) {
                            server.Restart();
                            found = true;
                            break;
                        }
                    }
                    if (found) {
                        Console.WriteLine("Restarted Server");
                    } else {
                        Console.WriteLine("Server not found");
                    }
                    break;
                default:
                    base.ReturnHelp(this.Help);
                    break;
            }
        }
    }
}
