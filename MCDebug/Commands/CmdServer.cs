using System;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MCMultiServer.Net;
using MCMultiServer.Srv;


namespace MCDebug.Commands {
    public class CmdServer : Command {
        public override string Name { get { return "server"; } }
        public override string Help { get { return "[name] <start|stop|restart|update|set [property] [value]|info>"; } }
        public override string Description { get { return "manages an online server"; } }
        public override void Use(string args) {
            if (args == string.Empty || args == null) {
                base.ReturnHelp(this.Help);
                return;
            }

            string[] a = args.Split(' ');

            //check the length of the arguements.
            if (a.Length < 2) {
                base.ReturnHelp(this.Help);
                return;
            } else if (a[1] == String.Empty) {
                base.ReturnHelp(this.Help);
                return;
            }

            Server srv = null;
            foreach (Server s in Manager.AllServers) {
                if (s.DisplayName.ToLower() == a[0].ToLower()) {
                    srv = s;
                    break;
                }
            }

            if (srv == null) {
                Console.WriteLine("Server does not exist or is loaded.");
                return;
            }

            switch (a[1]) {
                case "start":
                    if (srv.IsRunning) {
                        Console.WriteLine("{0} is already running", srv.DisplayName);
                        break;
                    }

                    if (!File.Exists(Paths.ServerDirectory + "/" + srv.ID.ToString() + "/" + srv.Properties.JarFile)) {
                        if (srv.Properties.Type == ServerType.Minecraft) {
                            Console.WriteLine("Jar File does not exist, replacing...");
                            if (!File.Exists(Paths.JarDirectory + "/minecraft_server." + srv.Properties.MCVersion + ".jar")) {
                                MCMultiServer.Util.JarManager.DownloadFile(srv.Properties.MCVersion);
                            }
                            File.Copy(Paths.JarDirectory + "/minecraft_server." + srv.Properties.MCVersion + ".jar", srv.RootDirectory + "/" + srv.Properties.JarFile);
                        } else {
                            Console.WriteLine("{0} does not have a java jar file placed, please do so before starting.");
                            return;
                        }
                    }

                    if (Manager.StartServer(srv.ID))
                        Console.WriteLine("'{0}' has been started", srv.DisplayName);
                    else
                        Console.WriteLine("'{0}' was unable to start, please try again", srv.DisplayName);
                    break;
                case "stop":
                    if (!srv.IsRunning) {
                        Console.WriteLine("{0} is not running", srv.DisplayName);
                        break;
                    }
                    Manager.StopServer(srv.ID);
                    Console.WriteLine("{0} has been stopped", srv.DisplayName);
                    break;
                case "restart":
                    if (!srv.IsRunning) {
                        Console.WriteLine("{0} is not running", srv.DisplayName);
                        break;
                    }
                    srv.Restart();
                    Console.WriteLine("{0} was restarted", srv.DisplayName);
                    break;
                case "update":
                    if (srv.Properties.Type == ServerType.Minecraft) {
                        Console.WriteLine("Updating jar file for {0}", srv.DisplayName);
                        if (!File.Exists(Paths.JarDirectory + "/minecraft_server." + srv.Properties.MCVersion + ".jar")) {
                            MCMultiServer.Util.JarManager.DownloadFile(srv.Properties.MCVersion);
                        }

                        if (File.Exists(srv.RootDirectory + "/" + srv.Properties.JarFile)) {
                            File.Delete(srv.RootDirectory + "/" + srv.Properties.JarFile);
                        }
                        File.Copy(Paths.JarDirectory + "/minecraft_server." + srv.Properties.MCVersion + ".jar", srv.RootDirectory + "/" + srv.Properties.JarFile);

                        Console.WriteLine("{0} updated", srv.DisplayName);
                    } else {
                        Console.WriteLine("This Minecraft server does not support updating");
                    }
                    break;
                case "set":
                    ServerProperties prop = srv.Properties;
                    JObject jprop = JObject.Parse(JsonConvert.SerializeObject(prop));

                    if (jprop[a[2]] != null) {
                        jprop[a[2]] = a[3];
                    } else {
                        Console.WriteLine("unknown setting '{0}'", a[2]);
                        return;
                    }

                    string name = prop.DisplayName;
                    Guid id = prop.ServerID;
                    prop = JsonConvert.DeserializeObject<ServerProperties>(jprop.ToString());
                    prop.ServerID = id;
                    prop.DisplayName = name;

                    srv.Properties = prop;

                    Database.ChangeServerSettings(id, srv.Properties);

                    Console.WriteLine("{0} changed", a[2]);
                    break;
                case "info":
                    Console.WriteLine();
                    Console.WriteLine("---Server Information [{0}]---", srv.DisplayName);
                    Console.WriteLine("ID: {0}", srv.ID);
                    Console.WriteLine("Root Directory: {0}", srv.RootDirectory);
                    Console.WriteLine("Running: {0}", srv.IsRunning.ToString());
                    Console.WriteLine("Settings: {0}", Newtonsoft.Json.JsonConvert.SerializeObject(srv.Properties));
                    break;
                default:
                    Console.WriteLine("Unknown switch '{0}'", a[1]);
                    break;
            }
        }
    }
}
