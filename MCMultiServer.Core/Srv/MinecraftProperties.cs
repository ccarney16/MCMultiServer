using System;
using System.IO;
using System.Collections.Generic;

namespace MCMultiServer.Srv {
    //Should work with most Minecraft Versions, unless Mojang decides to break things...
    public class MinecraftProperties {
        //All Properties
        public Dictionary<String, String> Properties { get; private set; } = new Dictionary<String, String>();
        
        /// <summary>
        /// A Settings file for Vanilla Minecraft
        /// </summary>
        public string PropertyFile { get; set; }
        
        public MinecraftProperties() { }
        public MinecraftProperties(String file) {
            Load(file);
        }

       //Load via file. Use Add() if not adding via file.
        public void Load(String file) {
            if (!File.Exists(file)) { throw new IOException("server.properties cannot be found"); }
            //read each line and split.
            PropertyFile = file;
            foreach (string line in File.ReadAllLines(file)) {
                //there will be some idiots writing giberish to break this.
                try {
                    if (!line.StartsWith("#")) {
                        string[] s = line.Split('=');
                        Properties.Add(s[0], s[1].ToString());
                    }
                } 
               catch {

               }
            }
        }

        public void Save() {
            StreamWriter writer = new StreamWriter(PropertyFile);
            writer.WriteLine("#Minecraft server properties");
            writer.WriteLine("#Edited by MCMultiServer");
            foreach (KeyValuePair<String, String> value in Properties) {
                writer.WriteLine(value.Key + "=" + value.Value.ToString());
            }
            writer.Flush();
        }

        public Boolean Exists(String option) {
            foreach (KeyValuePair<String, String> pair in Properties) {
                if (pair.Key == option) { return true; }
            }
            return false;
        }
        //Add options to the property file.
        public void Add(String option, String value) {
            if (this.Exists(option)) return;

            Properties.Add(option, value);
        }
        public void Set(String key, String value) {
            foreach (KeyValuePair<String, String> pair in Properties) {
                if (pair.Key == key) {
                    Properties[pair.Key] = value;
                    return;
                }
            }
        }
        public void Remove(String option) {
            if (!this.Exists(option)) return;

            Properties.Remove(option);
        }
    }
}
