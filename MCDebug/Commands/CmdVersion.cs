using System;
using System.Reflection;


namespace MCDebug.Commands {
    class CmdVersion : Command {
        public override string Name { get { return "version"; } }
        public override string Help { get { return null; } }
        public override string Description {
            get { return "Gets the Version of MCDebug and MCMultiServer"; }
        }
        public override void Use(string args) {
            ConsoleColor col = Console.ForegroundColor;
            Console.Write("MCDebug Version: ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Assembly.GetEntryAssembly().GetName().Version);
            Console.ForegroundColor = col;
            Console.WriteLine("MCMultiServer Version: ");
            Console.ForegroundColor = col;
        }
    }
}