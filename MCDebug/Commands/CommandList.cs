using System;
using System.Collections.Generic;

namespace MCDebug.Commands {
    public class CommandList : List<Command> {

        //Finds the Command in the list
        public void ExecuteCommand(string name, string args) {
            int fndcnt = 0;
            Command cmd = null;
            foreach (Command cmdfind in this) {
                if (cmdfind.Name.ToLower().StartsWith(name.ToLower())) {
                    fndcnt++;
                    if (fndcnt == 1) {
                        cmd = cmdfind;
                    }
                }
            }

            if (fndcnt > 1) {
                Console.WriteLine("MCDBG> unambigous command");
                return;
            }
            if (cmd == null) {
                Console.WriteLine("MCDBG> Unknown Command");
                return; }

            cmd.Use(args);
        }
    }
}
