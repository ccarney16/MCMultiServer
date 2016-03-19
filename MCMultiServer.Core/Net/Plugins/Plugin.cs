using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMultiServer.Net.Plugins {
    public abstract class Plugin {
        public string Name;
        public string configFile;
        public string dataFolder;

        public virtual void OnLoad() { }
        public virtual void OnEnable() { }

        public virtual void OnDisable() { }
    }
}
