using System;
using System.Collections.Generic;

using MCMultiServer.Net;
using MCMultiServer.Srv;
namespace MCMultiServer.Util {
    public static class Func {

        [Obsolete("Do not attempt to use this in any production environments")]
        public static Server GetServerbyName(string name) {
            foreach (Server s in Manager.AllServers) {
                if (s.DisplayName.ToLower() == name.ToLower()) {
                    return s;
                }
            }
            return null;
        }


    }
}
