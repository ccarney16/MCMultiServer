using System;

namespace MCMultiServer.Permissions {
    public class User {

        public String Name;

        public Guid ID;

        public Boolean isOp = false;

        //MD5 Hash
        public String PasswordHash;
    }
}
