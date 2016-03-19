using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCDebug.Commands {
    class CmdHelp : Command {
        public override string Name { get { return "help"; } }
        public override string Help { get { return null; } }
        public override string Description { get { return "Returns Server Commands"; } }

        public override void Use(string args) {
            Console.WriteLine("Help List");
            foreach (Command cmd in Command.cmd) {
                Console.WriteLine(cmd.Name + " " + cmd.Help + " - " + cmd.Description);
            }
        }
    }
}
