using System;
using System.Linq;
using System.Reflection;
//Core of the Command System. Fully modular for expansion without needing to modify
//the whole core of the system.
namespace MCDebug.Commands {
    public abstract class Command {
        
        public abstract string Name { get; }
        public virtual string Help { get; }
        public virtual string Description { get; }

        public virtual void Use(string args) {
            Console.WriteLine("Something went wrong with this command, please try again");
        }

        //List of all Commands
        public static CommandList cmd = new CommandList();

        //Inits all Commands
        public static void Init() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] typelist = assembly.GetTypes().Where(t => String.Equals(t.Namespace, "MCDebug.Commands", StringComparison.Ordinal)).ToArray();
            foreach (Type t in typelist) {
                if (t.Name.ToLower().StartsWith("cmd")) {
                    Object inst = System.Activator.CreateInstance(t);
                    Command newcmd = (Command)inst;
                    //Console.WriteLine("MCDBG> loading spell '" + newcmd.Name + "'");
                    cmd.Add(newcmd);
                }
            }
        }

        public void ReturnHelp(String HelpStr) {
            Console.WriteLine("Usage: " + Help);
        }
    }
}
