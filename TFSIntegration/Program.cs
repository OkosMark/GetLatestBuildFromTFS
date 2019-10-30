using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Newtonsoft.Json;

namespace TFSIntegration
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            TFSIntegration tfsIntegrationService = new TFSIntegration();
            
            if (Environment.UserInteractive)
            {
                tfsIntegrationService.RunAsConsole(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { tfsIntegrationService };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
