using System;
using System.IO;
using System.Text;

namespace MCMultiServer.Net {
    public static class Settings {
        //Default url's
        public const String DEFAULT_URL = "http://EXAMPLE.COM";
        public const String UPDATE_URL = "http://UPDATE.EXAMPLE.COM";

        //ID for a node server
        public static String NodeID { get; private set; }

        //Database Information
        public static String Database { get; private set; }
        public static String DatabaseConfig { get; private set; }

        //General Configurations
        public static Boolean AcceptMojangEULA { get; private set; }

        //For Windows, MCMS will check for an installed Java instance and attempt to use it
        public static Boolean CheckInstalledJava { get; private set; }

        //only set to false during debug use. otherwise, it still stay as it is.
        public static Boolean LoadAllServers { get; private set; }

        //set in MB, not GB
        public static Int32 MaxMemoryAllocation { get; private set; }

        //Autostart all Servers
        public static Boolean AutoStart { get; private set; }


        //Loads the database config
        private static Boolean loaded = false;
        //might change to json
        public static void Load() {
            if (loaded) { return; }

            //Throw Error Exception on missing the config file
            if (!File.Exists(Paths.ConfigurationFile)) {
                throw new IOException("config file does not exist");
            }

            //ok, lets check each line.
            foreach (String lin in File.ReadAllLines(Paths.ConfigurationFile)) {
                if (!lin.StartsWith("#") & !lin.StartsWith(" ") & lin != String.Empty) {
                    string[] split = lin.Split('=');
                    split[0] = split[0].TrimEnd();
                    split[1] = split[1].TrimStart();
                    switch (split[0].ToLower()) {
                        case "database":
                            Database = split[1];
                            break;
                        case "database-config":
                            DatabaseConfig = split[1];
                            break;
                        case "allow-autostart":
                            AutoStart = getBool(split[1]);
                            break;
                        case "check-java":
                            CheckInstalledJava = getBool(split[1]);
                            break;
                        case "accept-eula":
                            AcceptMojangEULA = getBool(split[1]);
                            break;
                        case "mb-memory-allocation":
                            MaxMemoryAllocation = Convert.ToInt32(split[1]);
                            break;
                        default:
                            Logger.Write(LogType.Warning, "unknown setting {0}", split[1]);
                            break;
                    }
                }
            }
            loaded = true;
        }

        //just converts a string to a boolean value.
        public static Boolean getBool(string value) {
            if (value.ToLower() == "true") { return true; }
            return false;
        }
    }
}
