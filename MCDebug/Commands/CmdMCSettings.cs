using System;
using System.Collections.Generic;
using System.Linq;

namespace MCDebug.Commands {
    class CmdMCSettings : Command {
        static MCMultiServer.Srv.MinecraftProperties properties = null;

        public override string Name { get { return "mcproperties"; } }
        public override string Description { get { return ""; } }

        public override string Help { get { return "<load (file location)|view|set>"; } }

        public override void Use(string args) {
            if (args == null || args == String.Empty) {
                base.ReturnHelp(this.Help);
                return;
            }

            string[] Split;
            if (args.Contains(' ')) Split = args.Split(' ');
            else Split = new string[] { args.ToString() };

            switch (Split[0].ToLower()) {
                case "load":
                    properties = new MCMultiServer.Srv.MinecraftProperties();
                    properties.Load(Split[1]);
                    Console.WriteLine("Loaded Settings");
                    break;
                case "view":
                    if (properties == null) {
                        base.ReturnHelp(Help);
                        return;
                    }

                    foreach (KeyValuePair<String, String> value in properties.Properties) {
                        Console.WriteLine(value.Key + " = " + value.Value);
                    }
                    break;
                case "set":
                    properties.Set(Split[1], Split[2]);
                    break;
                default:
                    base.ReturnHelp(this.Help);
                    break;
            }
        }
    }
}
