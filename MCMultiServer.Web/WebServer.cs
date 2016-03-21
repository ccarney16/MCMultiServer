using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MCMultiServer.Net;
namespace MCMultiServer.Web {
    public class WebServer : Net.Plugins.Plugin {

        public override void OnEnable() {
            Logger.Write(LogType.Info, "Hello World from WebServer!");

            base.OnEnable();
        }
    }
}
