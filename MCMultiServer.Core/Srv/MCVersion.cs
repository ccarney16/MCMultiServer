using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMultiServer.Srv {
    public class MCVersion {
        public ReleaseType Type;
        public string version;
    }

    public enum ReleaseType {
        alpha,
        beta,
        release,
        snapshot
    }
}
