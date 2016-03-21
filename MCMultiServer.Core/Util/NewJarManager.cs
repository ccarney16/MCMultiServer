using System;
using System.Collections.Generic;
using System.IO;
using System.Net;


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using MCMultiServer.Srv;
using MCMultiServer.Net;

namespace MCMultiServer.Util {
    //rewritten jar manager
    public static class NewJarManager {
        public struct Jar {
            //safe name
            public string Name;

            //file name in jar directory
            public string FileName;

            //mc version this matches
            public string Version;

            //url to download? maybe
            public string Url;

            public ServerType type;
        }

        //list of all versions, useful for custom jar files.
        private static List<String> _validVersions;
        public static List<Jar> JarFiles = new List<Jar>();

        public static string LatestRelease;

        private static string _MCURL = "https://launchermeta.mojang.com/mc/game/version_manifest.json";

        public static void Init() {
            Logger.Write("starting jar manager");
            using (WebClient webClient = new WebClient()) {
                try {
                    webClient.DownloadFile(new Uri(_MCURL), Paths.DataDirectory + @"\new.versions.json");

                    //delete the old file
                    if (File.Exists(Paths.DataDirectory + @"/versions.json")) {
                        File.Delete(Paths.DataDirectory + @"/versions.json");
                    }

                    File.Copy(Paths.DataDirectory + @"/new.versions.json", Paths.DataDirectory + @"/versions.json");
                    File.Delete(Paths.DataDirectory + @"/new.versions.json");
                } catch {
                    if (File.Exists(Paths.DataDirectory + "/versions.json")) {
                        Logger.Write("unable to download a new list, continuing with old list");
                    } else {
                        //I got nothing else if I dont have a file.
                        throw;
                    }
                }
            }

            //
            //Database code here sometime.
            //

            //lets load up everything up.


            JObject jlist = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("versions.json"));
            foreach (JToken obj in jlist["versions"]) {
                if (obj["type"].ToString().ToLower() == "release") {
                    Jar j = new Jar();
                    j.type = ServerType.Minecraft;
                    j.Version = obj["id"].ToString();
                    j.Name = obj["id"].ToString() + "-Mojang";
                    j.Url = obj["url"].ToString();
                    j.FileName = "minecraft_server." + obj["id"].ToString() + ".jar";
                    JarFiles.Add(j);
                }
            }
            LatestRelease = jlist["latest"]["release"].ToString();


            Logger.Write("jar manager finished");
        }
    }
}
