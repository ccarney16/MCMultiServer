using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;

using MCMultiServer.Net;
using MCMultiServer.Srv;
namespace MCMultiServer.Service {
    public partial class Service1 : ServiceBase {
        public Service1() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            Logger.Write("starting service...");
            try {
                Manager.Init();
            } catch {
                throw;
            }
        }

        protected override void OnStop() {
            //get to later.
        }
    }
}
