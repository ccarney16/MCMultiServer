using System;
using System.Data;
using System.IO;
using System.Reflection;

namespace MCMultiServer.Net {
    /// <summary>
    /// Base Database class. All database related stuff routes though here
    /// </summary>
    public abstract class Database {
        private static Database dbInstance;

        private static dbinfo info;

        //Returns the type of database
        public static String DBType { get {
                try { return dbInstance.GetType().ToString();
                } catch { throw new InvalidOperationException("Invalid Database Type or Database is Null"); }
            }
        }

        //Database Information
        public static String GetVersion() { return info.Version; }
        public static String GetName() { return info.Name; }

        //Get Database Instance Flags
        protected static DatabaseFlags[] Flags { get { return info.DBFlags; } }

        //Checks if the Database is Loaded.
        public static Boolean Loaded { get; private set; } = false;

        public static String[] Databases;

        //Loads up a database plugin.
        public static void SetDatabase(String dbtype) {
            if (Loaded) {
                Logger.Write(LogType.Warning, "Database is already Loaded");
                return;
            }

            //dbInstance = GetDBInstance(dbtype);

            //Find all the information first, then we can throw an exception
            Type i = Type.GetType(dbtype + "_info");
            if (i == null || i.BaseType != Type.GetType("MCMultiServer.Net.dbinfo")) throw new ArgumentException("Invalid or Null database");
            info = (dbinfo)System.Activator.CreateInstance(i);

            //Now lets loads up the database
            Type thistype = Type.GetType(dbtype);
            if (thistype == null || thistype.BaseType != Type.GetType("MCMultiServer.Net.Database")) throw new ArgumentException("Invalid or Null database");

            //Lets turn that type into an object
            Object inst = System.Activator.CreateInstance(thistype);

            dbInstance = (Database)inst;

            Loaded = true;

            Logger.Write(LogType.Info, "[{0}] Loaded Database", info.Name);
            if (Flags != null) {
                string flagsstr = "";
                foreach (DatabaseFlags f in Flags) {
                    flagsstr += Enum.GetName(typeof(DatabaseFlags), f) + ",";
                }
                Logger.Write(LogType.Info, "[{0}] Flags: {1}", info.Name, flagsstr);
            } else {
                Logger.Write(LogType.Warning, "[{0}] Database has no Flags (many features will not be used)", info.Name);
            }
        }

        public static Boolean Init() { return dbInstance.init(Settings.DatabaseConfig); }

        //Inside the Database system
        protected void OnLog(String s) {
            Logger.Write(LogType.Info, "[{0}] " + s, info.Name);
        }

        //init method
        public abstract Boolean init(string configFile = null);

        public String version {
            get { return Assembly.GetEntryAssembly().GetName().Version.ToString(); }
        }

        //features supported by the database, not needed for override
        public virtual DatabaseFlags[] supportedFlags { get; }
        
        //This is unsafe, Recommended only to follow other methods.
        public abstract void executeQuery(String query, Boolean verbose = false);
        public abstract DataTable getDataTable(String query);

        //Server Data
        public abstract Boolean addServer(Guid serverID, String serverName, Srv.ServerProperties properties);
        public abstract Boolean changeServerSettings(Guid serverID, Srv.ServerProperties properties);
        public abstract Boolean changeServerName(Guid serverID, String newName);
        public abstract MCMultiServer.Srv.ServerProperties getServerProperties(Guid serverID);
        public abstract MCMultiServer.Srv.ServerProperties[] getAllServers();
        public abstract MCMultiServer.Srv.ServerProperties[] getServerList(Int32 firstRow, Int32 lastRow);
        public abstract Boolean dropServer(Guid serverID);

        //ip mapper functions.
        public abstract Boolean addIPMap(string map);
        public abstract String[] getIPMap();
        public abstract Boolean removeIPMap(string ip); 


        //Public callable methods
        public static void AddServer(Guid serverID, String serverName, Srv.ServerProperties properties) {
            try {
                dbInstance.addServer(serverID, serverName, properties);
            } catch(NullReferenceException e) { throw e; }
        }
        public static Srv.ServerProperties GetServerProperties(Guid serverID) {
            try {
                return dbInstance.getServerProperties(serverID);
            } catch (NullReferenceException e) { throw e; }
        }
        public static Srv.ServerProperties[] GetAllServers() {
            try {
                return dbInstance.getAllServers();
            } catch (NullReferenceException e) { throw e; }
        }
        public static Boolean ChangeServerSettings(Guid serverID, Srv.ServerProperties properties) {
            try {
                return dbInstance.changeServerSettings(serverID, properties);
            } catch (NullReferenceException e) { throw e; }
        }

        public static Boolean DropServer(Guid serverID) {
            try {
                return dbInstance.dropServer(serverID);
            } catch (NullReferenceException e) { throw e; }
        }
    }

    //Supported options
    public enum DatabaseFlags : byte {
        //Override most settings
        Settings_Override,
        //able to support queries.
        Query_Support,
        //Support for Json.NET
        Json_Support,
        //Makes sure that the database can handle multiple nodes.
        Multi_Node
    }

    //information for the database class.
    public abstract class dbinfo {
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract DatabaseFlags[] DBFlags { get; }
    }
}