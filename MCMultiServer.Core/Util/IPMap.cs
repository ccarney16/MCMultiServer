using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MCMultiServer.Util {
    public class IPMap {

        public string IPAddress { get; private set; }


        public List<ushort> Ports = new List<ushort>();


        public IPMap(string map) {
            //separate ip and port numbers
            string[] splitA = map.Split('/');

            //should be an ip address
            IPAddress = splitA[0];

            //split all the ports up now
            string[] splitB = splitA[1].Split(';');
            foreach (string p in splitB) {
                string[] ports = p.Split('-');

                if (ports.Length == 1) {
                    if (!Ports.Contains(Convert.ToUInt16(p))) {
                        Ports.Add(Convert.ToUInt16(p));
                    }
                } else if (ports.Length == 2) {
                    ushort plow = Convert.ToUInt16(ports[0]);
                    ushort phigh = Convert.ToUInt16(ports[1]);
                    //keep less than and everything should be fine.
                    if (plow < phigh) {
                        for (ushort i = plow; i <= phigh; i++) {
                            if (!Ports.Contains(i)) {
                                Ports.Add(i);
                            }
                        }
                    } else {
                        //invalid format
                        throw new Exception();
                    }
                }
            }
        }
    }
}
