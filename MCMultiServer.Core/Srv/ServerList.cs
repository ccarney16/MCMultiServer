using System;
using System.Collections.Generic;

namespace MCMultiServer.Srv {
    public class ServerList : List<Server> {
        //Gets the Server instance from the list. really dangerous if many servers are on a machine.
        public Server GetServer(Guid id) {
            foreach (Server srv in this) {
                if (srv.ID == id) { return srv; }
            }
            return null;
        }

        //Takes over the List's normal Add system.
        public new void Add(Server s) {
            if (this.Exists(s.ID)) {
                throw new ArgumentException("server with id of " + s.ID.ToString() + " already exists.");
            }
            //we can call base now.
            base.Add(s);
        }

        public Boolean Exists(Guid id) {
            if (GetServer(id) != null) { return true; }
            return false;
        }
    }
}
