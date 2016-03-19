using System;
using System.IO;
using System.Net;

using MCMultiServer.Net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MCMultiServer.Util {
    public static class JarManager {
        public static MojangReleases _releases;

        public static void Init() {
            //moving to jar manager when ready
            Logger.Write(LogType.Info, "loading jar manager");
            using (WebClient webClient = new WebClient()) {
                try {
                    webClient.DownloadFile(new Uri("https://launchermeta.mojang.com/mc/game/version_manifest.json"), Paths.DataDirectory + @"\new.versions.json");

                    //delete the old file
                    if (File.Exists(Paths.DataDirectory + @"/versions.json")) {
                        File.Delete(Paths.DataDirectory + @"/versions.json");
                    }

                    File.Copy(Paths.DataDirectory + @"/new.versions.json", Paths.DataDirectory + @"/versions.json");
                    File.Delete(Paths.DataDirectory + @"/new.versions.json");
                } catch {
                    Logger.Write(LogType.Warning, "Unable to download latest versions, using an old list.");
                }
            }

            MojangReleases versions = JsonConvert.DeserializeObject<MojangReleases>(File.ReadAllText(Paths.DataDirectory + "/versions.json"));
            _releases = versions;

            int count = 0;
            string msg = null;
            msg += "Minecraft Versions: ";
            foreach (MojangReleases._versions ver in _releases.AllVersions) {
                if (count != 6) {
                    msg = msg + ver.ID + ", ";
                    count++;
                } else {
                    Logger.Write(LogType.Info, msg + "" + ver.ID);
                    count = 0;
                    msg = null;
                }
            } 
        }

        public static bool VersionExists(string version) {
            foreach (MojangReleases._versions ver in _releases.AllVersions) {
                if (ver.ID == version) {
                    return true;
                }
            }
            return false;
        }


        //allows downloading snapshots now.
        public static void DownloadFile(string version) {
            MojangReleases._versions ver = new MojangReleases._versions();
            foreach (MojangReleases._versions v in _releases.AllVersions) {
                if (v.ID == version) {
                    ver = v;
                } 
            }

            using (WebClient client = new WebClient()) {
                try {
                    string json = client.DownloadString(ver.Url);
                    JObject obj = JObject.Parse(json);

                    string serverurl = obj["downloads"]["server"]["url"].ToString();
                    Logger.Write(LogType.Info, "attempting to download version {0}", ver.ID);
                    client.DownloadFile(serverurl, Paths.JarDirectory + "/minecraft_server." + ver.ID + ".jar");
                } catch (Exception e) {
                    throw e;
                }
            }
        }
    }

    //information for Minecraft releases
    public class MojangReleases {

        [JsonProperty(PropertyName = "latest")]
        public _latest LatestReleases;
        public struct _latest {
            public string snapshot;
            public string release;
        }

        [JsonProperty(PropertyName = "versions")]
        public _versions[] AllVersions;
        public struct _versions {
            [JsonProperty(PropertyName = "id")]
            public string ID;

            [JsonProperty(PropertyName = "time")]
            public string Date;

            [JsonProperty(PropertyName = "releaseTime")]
            public string ReleaseTime;

            [JsonProperty(PropertyName = "type")]
            public string Type;

            [JsonProperty(PropertyName = "url")]
            public string Url;
        }
    }
}
