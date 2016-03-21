using System;
using System.IO;

using MCMultiServer.Srv;
using MCMultiServer.Util;

using System.Diagnostics;
using System.Reflection;
using System.Resources;

namespace MCMultiServer.Net {
    public static class Manager {
        static Boolean _isRunning = false;
        static Boolean _isWin32 = true;


        public static ServerList AllServers = new ServerList();

        //init system
        public static void Init() {
            if (_isRunning) throw new ArgumentException("manager is already running");

            Logger.Write(LogType.Info, "starting mcmultiserver manager");

            //check environment
            if (System.Environment.OSVersion.Platform.ToString().ToLower() == "unix") {
                Logger.Write(LogType.Info, "unix operating system detected");
                _isWin32 = false;
            } else { Logger.Write(LogType.Info, ".NET Framework detected"); }

            //Load Settings & Check Directories
            Settings.Load();
            Paths.CheckDirectories();

            JarManager.Init();

            //test code for plugin system
            //Assembly assem = Assembly.LoadFile(Paths.ConfigDirectory + "/MCMultiServer.Web.dll");
            //ResourceManager rm = new ResourceManager("MCMultiServer.Web.Properties.Resources", assem);
            //Logger.Write(LogType.Info, rm.GetString("plugin_data"));

            Database.SetDatabase(Settings.Database);

            //throw an error when the database hits one, regardless if it can be fixed or not.
            try {
                if (Database.Init()) { Logger.Write(LogType.Info, "{0} database has started", Database.DBType); }
            } catch { throw; }

            if (_isWin32) {
                if (Util.Win32.HasInstalledJava()) {
                    if (Util.Win32.CheckVersion() != null) {
                        Logger.Write(LogType.Info, "this windows server is using an installed Java {0} instance", Util.Win32.CheckVersion());
                        if (!Util.Win32.IsValidJRE(Util.Win32.CheckVersion())) {
                            Logger.Write(LogType.Warning, "the detected version of java is unsupported. refer to documentation for information");
                        }
                        Logger.Write(LogType.Info, "installed java Location: {0}", Util.Win32.JREPath());
                        Paths.JVMInstance = new Util.JavaInstance("DEFAULT_JRE", Util.Win32.JREPath());
                    }
                } else {
                    Logger.Write(LogType.Warning, "java cannot be found! if you have java, please set it in mcms.conf");
                }
            } else {
                //basic linux support
                Logger.Write(LogType.Warning, "checking for Java is currently not supported in Unix like systems, setting defaults");
                Paths.JVMInstance = new JavaInstance("DEFAULT_JRE", "/usr/bin/");
            }

			//We are up and running, start all servers.
            _isRunning = true;


            //Tell the memory we are using.
            Logger.Write(LogType.Info, "{0} MB of the {1} MB max allocated memory has been used", GetAllocatedMemory(), Settings.MaxMemoryAllocation);
            if (GetAllocatedMemory() > Settings.MaxMemoryAllocation) {
                Logger.Write(LogType.Warning, "loaded allocated memory from the servers is over the max memory set, please remove a server or two before starting.");
            }

            //load all node data.
            ServerProperties[] srvproplist = Database.GetAllServers();
            if (srvproplist.Length != 0) {
                foreach (ServerProperties prop in srvproplist) {
                    //make sure that if an unclean shutdown occured that all processes are killed. will change if possible to a normal shutdown.
                    if (File.Exists(Paths.ServerDirectory + "/" + prop.ServerID.ToString() + "/minecraft.pid")) {
                        Logger.Write(LogType.Warning, "unclean shutdown detected for server '{0}', checking and shutting down process", prop.ServerID);
                        try {
                            int procid = Convert.ToInt32(File.ReadAllText(Paths.ServerDirectory + "/" + prop.ServerID.ToString() + "/minecraft.pid"));
                            System.Diagnostics.Process proc = System.Diagnostics.Process.GetProcessById(procid);
                            proc.Kill();
                        } catch {}
                    }
                    AddServer(prop.ServerID);
                    if (Settings.AutoStart) {
                        if (!AllServers.GetServer(prop.ServerID).IsRunning) {
                            StartServer(prop.ServerID);
                        }
                    }
                }
            }

            //We are done here.
            Logger.Write(LogType.Info, "manager has finished setting up");
        }

        public static bool CheckPort(string ip, int port) {
            foreach (ServerProperties p in Database.GetAllServers()) {
                if (p.Port == port && p.IPAddress == ip) {
                    return true;
                }
            }
            return false;
        }

        //get current memory stats from all running servers.
        public static Int32 GetAllocatedMemory() {
            int cur = 0;
            foreach (Server srv in AllServers) {
                cur += System.Convert.ToInt32(srv.Properties.Memory);
            }
            return cur;
        }

        //creates the server, but does not load it.
        public static void CreateServer(string name, Guid id) {
            if (!_isRunning) throw new ArgumentException("server manager is not running");
            if (name == null || name == string.Empty) { throw new ArgumentNullException("server name cannot be null or empty"); }

            ServerProperties prop = CreateProperties(name);
            Directory.CreateDirectory(Paths.ServerDirectory + "/" + id);

            Database.AddServer(id, name, prop);

            Logger.Write(LogType.Info, "server '{0}' was created with id '{1}'", name, id.ToString());
        }

        //Creates the base Server Properties, this wont save the settings.
        public static ServerProperties CreateProperties(string name) {
            ServerProperties prop = new ServerProperties();

            //set name
            prop.DisplayName = name;

            //server type
            prop.Type = ServerType.Minecraft;

            //have to change to jarmanager later
            prop.JarFile = "minecraft_server.jar";

            //Do everything in MB
            prop.Memory = "1024";

            //supported threads
            prop.Threads = "2";

            //grabs the latest release
            prop.MCVersion = JarManager._releases.LatestReleases.release;

            prop.Optimize = true;

            prop.IPAddress = "0.0.0.0";

            prop.Port = 25565;

            return prop;
        }

        public static Boolean AddServer(Guid serverID) {
            if (!_isRunning) throw new ArgumentException("manager is not running");
            if (serverID == null || serverID == Guid.Empty) throw new ArgumentNullException("server id cannot be null or empty");

            if (Database.GetServerProperties(serverID) != null) {
                if (AllServers.Exists(serverID)) {
                    return false;
                } else {
                    
                    Server s = new Server();
                    s.Load(Paths.ServerDirectory + "/" + serverID.ToString(), Database.GetServerProperties(serverID));

                    AllServers.Add(s);
                    return true;
                }
            }
            return false;
        }

        public static Boolean RemoveServer(Guid serverID) {
            if (!_isRunning) throw new ArgumentException("manager is not running");
            if (serverID == null || serverID == Guid.Empty) throw new ArgumentNullException("server id cannot be null or empty");

            Server serv = AllServers.GetServer(serverID);

            if (serv != null) {
                if (serv.IsRunning) { serv.Shutdown(); }
                AllServers.Remove(serv);
                Logger.Write(LogType.Info, "server '{0}' was removed from manager", serv.ID);
                return true;
            }
            return false;
        }

        //Deletes all data responding to the id
        public static Boolean DropServer(Guid serverID) {
            if (!_isRunning) throw new ArgumentException("manager is not running");
            if (serverID == null || serverID == Guid.Empty) throw new ArgumentNullException("server id cannot be null or empty");

            Server serv = AllServers.GetServer(serverID);

            if (serv != null) { if (serv.IsRunning) { return false; } }

            if (Database.GetServerProperties(serverID) != null) {
                Database.DropServer(serverID);
                Logger.Write(LogType.Info, "server '{0}' was removed from database", serverID);                
            } else { Logger.Write(LogType.Error, "Database does not contain information for '{0}'!", serverID.ToString()); }
            return false;
        }

        public static Boolean StartServer(Guid serverID) {
            if (!_isRunning) throw new ArgumentException("manager is not running");
            if (serverID == null || serverID == Guid.Empty) throw new ArgumentNullException("server id cannot be null or empty");

            if (AllServers.Exists(serverID)) {
                Server serv = AllServers.GetServer(serverID);
                if (!(serv.IsRunning || serv.Loaded)) return false;

                AgreeMojangEULA(serverID);

                //If normal Minecraft, allow this.
                if (serv.Properties.Type == ServerType.Minecraft || serv.Properties.Type == ServerType.CraftBukkit) {
                    if (!File.Exists(serv.RootDirectory + "/server.properties")) {
                        MinecraftProperties prop = new MinecraftProperties(Paths.ConfigDirectory + "/default.properties");
                        prop.PropertyFile = serv.RootDirectory + "/server.properties";
                        prop.Add("server-port", serv.Properties.Port.ToString());
                        prop.Add("server-ip", serv.Properties.IPAddress);
                        prop.Save();
                    } else {
                        //just in case someone got smart, lets write over the file.
                        MinecraftProperties prop = new MinecraftProperties();
                        prop.Load(serv.RootDirectory + "/server.properties");
                        prop.Set("server-port", serv.Properties.Port.ToString());
                        prop.Set("server-ip", serv.Properties.IPAddress);
                        prop.Save();
                    }
                }

                System.Threading.Thread.Sleep(1000);

                
                if (_isWin32) {
                    serv.Start(Paths.JVMInstance.JREPath + @"/" + Paths.JVMInstance.javaExecutable);
                } else {
                    serv.Start(Paths.JVMInstance.JREPath + @"/" + Paths.JVMInstance.javaExecutableUNIX);
                }
                Logger.Write(LogType.Info, "server with an id of '{0}' was started", serv.ID);
                return true;
            }
            return false;
        }

        public static Boolean StopServer(Guid serverID) {
            if (!_isRunning) throw new ArgumentException("manager is not running");
            if (serverID == null || serverID == Guid.Empty) throw new ArgumentNullException("server id cannot be null or empty");
            if (AllServers.Exists(serverID)) {
                Server serv = AllServers.GetServer(serverID);
                if (serv.IsRunning) {
                    serv.Shutdown();
                    Logger.Write(LogType.Info, "server '{0}' was requested to stop");
                    return true;
                }
            }
            return false;
        }

        //Creates a new Server ID. Might be replaced when in a node system.
        public static Guid CreateNewServerID() {
            Guid id = Guid.NewGuid();
            //Checks to make sure if a directory exists.
            bool uniqueid = false;
            while (!uniqueid) {
                if (Database.GetServerProperties(id) == null) {
                    uniqueid = true;
                } else { uniqueid = false; }
            }
            return id;
        }

        //Simple write to text file.
        public static void AgreeMojangEULA(Guid serverID) {
            if (Settings.AcceptMojangEULA) {
                File.WriteAllText(Paths.ServerDirectory + "/" + serverID + "/eula.txt", "eula=true");
            }
        }

        //Halt all running servers.
        public static void StopAllServers() {
            Logger.Write(LogType.Warning, "Halting All Servers!");
            foreach (Server srv in AllServers) {
                srv.Shutdown();
            }
        }
    }
}