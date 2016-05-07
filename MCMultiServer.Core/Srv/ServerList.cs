using System;
using System.Collections.Generic;

namespace MCMultiServer.Srv {
    public class ServerList : List<Server> {
        /// <summary>
        /// Gets a server object from the list.
        /// </summary>
        /// <param name="id">ID of the server</param>
        public Server GetServer(Guid id) {
            foreach (Server srv in this) {
                if (srv.ID == id) { return srv; }
            }
            return null;
        }

        //Takes over the List's normal Add system.
        public new void Add(Server s) {
            if (s.Properties.ServerID == null || s.Properties.ServerID == Guid.Empty) {
                throw new ArgumentException("server id cannot be null!");
            }
            if (this.Exists(s.ID)) {
                throw new ArgumentException("server with id of " + s.ID.ToString() + " already exists.");
            }
            //you can call base now.
            base.Add(s);
        }

        /// <summary>
        /// Checks to see if a server exists or not by the ID
        /// </summary>
        public Boolean Exists(Guid id) {
            if (GetServer(id) != null) { return true; }
            return false;
        }
    }
}
