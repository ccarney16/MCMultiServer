using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Timers;
using System.IO;

namespace MCMultiServer.Srv {
    public class Server : IDisposable {
        //Events to hook into the server. This is useful to keep hooks onto.
        public delegate void OnOutput(String line, Guid serverid);
        public static event OnOutput LogOutput;
        public static event OnOutput ErrorOutput;

        //Designed to handle Exceptions
        public delegate void OnError(Exception ex, Guid serverid);
        public static event OnError ReportError;

        //cleans up all resources needed for the manager.
        private Timer _checkupTimer = new Timer();
        //Restart Timer, of course we need more timers...
        private Timer _restartTimer = new Timer();

        //Name for server
        public String DisplayName { get { return Properties.DisplayName; } }

        //ID for server
        public Guid ID { get { return Properties.ServerID; } }

        //Checks if the server is loaded.
        public Boolean Loaded { get; private set; } = false;

        //Root Directory of the Server
        public String RootDirectory { get; private set; } = String.Empty;
        private String _jrePath;

        //Server Properties
        public ServerProperties Properties { get; set; } = null;

        //Instance for the Minecraft Server
        private Process instance = null;

        //This needs to be fleshed out.
        private void ServerOutput(object sender, DataReceivedEventArgs e) {
            DateTime time = DateTime.Now;

            if (e.Data == null || e.Data == String.Empty) return;

            if (ErrorOutput != null) {
                string str = e.Data;
                LogOutput(str, this.ID);
            }
        }

        private void ServerError(object sender, DataReceivedEventArgs e) {
            DateTime time = DateTime.Now;

            //got to prevent the ship going down somehow.
            if (e.Data == null || e.Data == String.Empty) return;

            if (ErrorOutput != null) { ErrorOutput(e.Data.ToString(), this.ID); }
        }


        // Gets the value of the running server.
        public Boolean IsRunning {
            get {
                try { return !instance.HasExited; } catch { return false; }
            }
        }

        //Grabs the ID of the Process.
        public Int32 getRunningProcessID {
            get {
                try { return instance.Id; } catch { return -1; }
            }
        }

        //Cleanup the server object once the system is shutdown.
        private void _cleanupTimer_Elapsed(object sender, ElapsedEventArgs e) {
            if (!IsRunning) {
                //delete the minecraft pid and property file
                if (File.Exists(RootDirectory + "/minecraft.pid")) {
                    File.Delete(RootDirectory + "/minecraft.pid");
                }
            }
        }

        //Does the Server instance need to be reloaded?
        public Boolean RestartNeeded { get; internal set; } = false;

        public Int32 GetRuntimeProcessID() { return instance.Id; }

        public void Load(String rootdir, ServerProperties prop) {
            if (Loaded) { LogOutput("Server is already loaded!", this.ID); }
            try {
                Properties = prop;
                RootDirectory += rootdir;
                Loaded = true;

                //Restart timer will only react in 5 seconds.
                _restartTimer.Interval = 5000;
                _restartTimer.Elapsed += _restartTimer_Elapsed;
            } catch (Exception ex) { throw ex; }
        }

        int _restartcount = 0;
        private void _restartTimer_Elapsed(object sender, ElapsedEventArgs e) {
            if (RestartNeeded & !IsRunning) {
                this.Start(_jrePath);
                _restartTimer.Stop();
                RestartNeeded = false;
                return;
                //this is where we can now claim the program unresponsive and kill the process.
            } else if (_restartcount == 5 & IsRunning) {
                Shutdown(true, false);
                _restartcount = 0;
                return;
            }
            _restartcount++;
        }

        public void Restart() {
            if (!Loaded) { ErrorOutput("Server is not loaded!", this.ID); return; }
            if (!IsRunning) { LogOutput("Server is not running!", this.ID); return; }
            if (RestartNeeded) { LogOutput("Server is already in the process of restarting", this.ID); return; }

            //Dont do a force restart and dont do a cleanup.
            Shutdown(false, false);
            RestartNeeded = true;

            _restartTimer.Start();
            LogOutput("Restarting Server", this.ID);
        }

        public void Unload(Boolean force = false) {
            if (!Loaded) { ErrorOutput("Server is not loaded!", this.ID); }

            //If Running, check if you want to force the server to close, THIS WILL ALWAYS KILL THE SERVER!
            if (IsRunning) { if (!force) { return; } else { Shutdown(true); } }
            //Lets start fresh
            Loaded = false;
            Properties = new ServerProperties();
        }

        private string createArguements() {
            string args = "";
            if (this.Properties.Type != ServerType.Custom) {
                //Optimize Settings
                if (Properties.Optimize)
                    args += "-server -XX:+UseConcMarkSweepGC -XX:+UseParNewGC -XX:+CMSIncrementalPacing -XX:ParallelGCThreads=2 -XX:+AggressiveOpts";
                //Bukkit Properties.
                if (this.Properties.Type == ServerType.CraftBukkit)
                    args += " -Djline.terminal=jline.UnsupportedTerminal";
                //Non Optmized arguements
                args += String.Format(" -Xmx{0}M -jar {1} nogui", Properties.Memory, Properties.JarFile);
            }
            return args;
        }

        private string getJREPath() {
            return Net.Paths.JVMInstance.JREPath;
        }

        //starts server
        public void Start(string jrePath) {
            //Block of Exception code.
            if (!Loaded) { LogOutput("Server is not loaded!", this.ID); return; }
            if (IsRunning) { LogOutput("Server is already running!", this.ID); return; }
            if (RootDirectory == null || RootDirectory == String.Empty) throw new ApplicationException("root directory was not set");

            //configure path and arguements. 
            instance = new Process();
            instance.StartInfo.FileName = String.Format(jrePath);
            _jrePath = jrePath;
            instance.StartInfo.Arguments = this.createArguements();
            
            instance.StartInfo.UseShellExecute = false;
            instance.StartInfo.CreateNoWindow = true;
            instance.StartInfo.WorkingDirectory = RootDirectory;

            instance.StartInfo.RedirectStandardInput = true;
            instance.StartInfo.RedirectStandardOutput = true;
            instance.StartInfo.RedirectStandardError = true;
            instance.EnableRaisingEvents = true;

            instance.OutputDataReceived += new DataReceivedEventHandler(ServerOutput);
            instance.ErrorDataReceived += new DataReceivedEventHandler(ServerError);
            instance.EnableRaisingEvents = true;
            
            instance.Start();

            instance.BeginOutputReadLine();
            instance.BeginErrorReadLine();

            if (_checkupTimer.Enabled) {
                _checkupTimer.Interval = 5000;
                _checkupTimer.Elapsed += _cleanupTimer_Elapsed;
                _checkupTimer.Start();
            }

            using (StreamWriter writer = new StreamWriter(this.RootDirectory + "/minecraft.pid")) {
                writer.Write(this.instance.Id);
                writer.Flush();
            }
        }

        //Shuts down the server. Keep cleanup to true
        public void Shutdown(Boolean force = false, Boolean cleanup = true) {
            if (!Loaded) {
                ErrorOutput("Server is not loaded!", this.ID);
                return;
            }

            if (!IsRunning) {
                ErrorOutput("Server is not running!", this.ID);
                return;
            }

            //sends the stop command if not forced to kill.
            if (!force) {
                SaveAll();
                instance.StandardInput.WriteLine("stop");
                LogOutput("Server was requested to stopped", this.ID);
            } else {
                instance.Kill();
                LogOutput("Server was forced to stop", this.ID);
            }
        }

        public void SaveAll() { SendInput("save-all"); }

        public void SendInput(string msg) { instance.StandardInput.WriteLine(msg); }

        //When the object is going to be disposed.
        public void Dispose() {
            if (IsRunning) {
                instance.Kill();
            }
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }

    public class ServerProperties {
        //Used for Databases
        [JsonIgnore]
        public String DisplayName = String.Empty;
        [JsonIgnore]
        public Guid ServerID = Guid.Empty;
        [JsonIgnore]
        public DateTime DateCreated;

        /// <summary>
        /// Software version of the server, vanilla (and modded), CraftBukkit, or custom.
        /// </summary>
        [JsonProperty(PropertyName = "server-type")]
        public ServerType Type;

        [JsonProperty(PropertyName = "jar-file")]
        public String JarFile;

        //like nogui, justs adds arguements into the execution. will have an option to disable mc-arguements
        [JsonProperty(PropertyName = "mc-args")]
        public String MinecraftArguements;

        /// <summary>
        /// Version of Minecraft to look for.
        /// </summary>
        [JsonProperty(PropertyName = "mc-version")]
        public String MCVersion;

        /// <summary>
        /// The amount of memory java can use
        /// </summary>
        [JsonProperty(PropertyName = "memory")]
        public String Memory;

        /// <summary>
        /// The amount of threads a java application can use.
        /// </summary>
        [JsonProperty(PropertyName = "threads")]
        public String Threads;

        /// <summary>
        /// Enables java optimized arguements, typically for bukkit and spigot
        /// </summary>
        [JsonProperty(PropertyName = "optimize")]
        public Boolean Optimize;

        /// <summary>
        /// specify an IP address for one server
        /// </summary>
        [JsonProperty(PropertyName = "ip-addr")]
        public String IPAddress;

        /// <summary>
        /// Lock the server port down.
        /// </summary>
        [JsonProperty(PropertyName = "server-port")]
        public Int32 Port;
    }
    /// <summary>
    /// Different server types for Minecraft, most common are vanilla and CraftBukkit
    /// </summary>
    public enum ServerType : Byte {
        /// <summary>
        /// Vanilla/Modded Minecraft Server
        /// </summary>
        Minecraft = 0,

        /// <summary>
        /// CraftBukkit and Spigot
        /// </summary>
        CraftBukkit = 1,

        /// <summary>
        /// Bungeecord servers.
        /// </summary>
        BungeeCord = 2,

        /// <summary>
        /// Custom Jar files.
        /// </summary>
        Custom = 3
    }
}