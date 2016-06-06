using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using MCMultiServer.Srv;
using MCMultiServer.Net;

namespace MCMultiServer.Util {
    public static class JarManager {
        public static Boolean IsSetup { get; private set; } = false;

        //list of all versions, useful for custom jar files.
        private static List<String> _validVersions = new List<String>();
        //All Entries for minecraft
        public static List<Jar> JarFileEntries = new List<Jar>();

        public static String LatestRelease;

        //Url for mojang's list
        private static String _MCURL = "https://launchermeta.mojang.com/mc/game/version_manifest.json";

        public static void Init() {
            Logger.Write("starting jar manager");
            //import later
            //JarFileEntries = (List<Jar>)JsonConvert.DeserializeObject(File.ReadAllText("./jarentries.json"));

            UpdateMojang();

            string msg = "Jar Entries: ";
            int count = 0;
            foreach (Jar j in JarFileEntries) {
                if (count != 6) {
                    msg = msg + j.Name + "; ";
                    count++;
                } else {
                    Logger.Write(LogType.Info, msg + "" + j.Name + "; ");
                    count = 0;
                    msg = null;
                }
            }
            Logger.Write(msg);
            //lets load up everything up.
            Logger.Write("jar manager finished");
            IsSetup = true;            
        }

        public static void DownloadJarFile(string name) {
            if (name == null) { throw new ArgumentException("name cannot be null"); }
            Jar j = GetJarFileEntry(name);
            if (j == null) {
                return;
            }

            using (WebClient client = new WebClient()) {
                try {
                    string json = client.DownloadString(j.Url);
                    JObject obj = JObject.Parse(json);

                    string serverurl = obj["downloads"]["server"]["url"].ToString();
                    Logger.Write(LogType.Info, "attempting to download version {0}", j.Name);
                    client.DownloadFile(serverurl, Paths.JarDirectory + "/minecraft_server." + j.Name + ".jar");
                } catch {
                    throw;
                }
            }
        }

        //Returns a jar file, decided to reuse this a few times.
        public static Jar GetJarFileEntry(string name) {
            if (name == null) { throw new ArgumentException("name cannot be null"); }
            foreach (Jar j in JarFileEntries) {
                if (j.Name.ToLower() == name.ToLower()) {
                    //we are done here.
                    return j;
                }
            }
            //return an empty jar entry that does not exist.
            return null;
        }

        //empty for now...
        public static void UpdateMojang() {
            Logger.Write("Updating jar manager...");
            using (WebClient webClient = new WebClient()) {
                try {
                    webClient.DownloadFile(new Uri(_MCURL), Paths.DataDirectory + @"/new.versions.json");

                    //we need to make sure that the file im looking for exists.
                    if (File.Exists(Paths.DataDirectory + @"/new.versions.json")) {
                        //delete the old file
                        if (File.Exists(Paths.DataDirectory + @"/versions.json")) {
                            File.Delete(Paths.DataDirectory + @"/versions.json");
                        }

                        File.Copy(Paths.DataDirectory + @"/new.versions.json", Paths.DataDirectory + @"/versions.json");
                        File.Delete(Paths.DataDirectory + @"/new.versions.json");
                    }
                } catch {
                    if (File.Exists(Paths.DataDirectory + "/versions.json")) {
                        Logger.Write(LogType.Error, "unable to download a new version list from mojang, using old list");
                    } else {
                        //I have nothing else to do if the file im looking for does not exist
                        throw;
                    }
                }
            }

            //Ability to use snapshots
            Boolean useBeta = false;
            JObject jlist = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("versions.json"));
            foreach (JToken obj in jlist["versions"]) {
                if (obj["type"].ToString().ToLower() == "release" | (obj["type"].ToString().ToLower() == "snapshot" && useBeta)) {
                    if (GetJarFileEntry(obj["id"].ToString() + "-Mojang") == null) {
                        Jar j = new Jar();
                        j.type = ServerType.Minecraft;
                        //just the version number, will also place in valid versions.
                        j.Version = obj["id"].ToString();
                        //1.5.2-Mojang, sounds about right
                        j.Name = obj["id"].ToString() + "-Mojang";
                        //only for mojang jar files.
                        j.Url = obj["url"].ToString();
                        JarFileEntries.Add(j);
                    }
                    if (_validVersions.Contains(obj["id"].ToString())) {
                        _validVersions.Add(obj["id"].ToString());
                    }
                }
            }
            LatestRelease = jlist["latest"]["release"].ToString();
        }
    }

    public class Jar {
        [JsonProperty("jar-name")]
        public string Name;

        [JsonProperty("version")]
        public string Version;

        //Url for jar file, useless on non minecraft releases
        [JsonProperty("url")]
        public string Url;

        [JsonProperty("type")]
        public ServerType type;
    }
}
