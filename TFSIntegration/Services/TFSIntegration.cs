using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TFSIntegration
{    
    partial class TFSIntegration : ServiceBase
    {
        private System.Timers.Timer aTimer;
        private TFSIntegrationImplementation integrationImplementation;
        private TFSConfiguration tfsConfiguration;

        public TFSIntegration()
        {
            InitializeComponent();
        }

        [STAThread]
        protected override void OnStart(string[] args)
        {
            Logger.Log.Info("Running in service mode!");
            Logger.Log.Info("Start");
          
            aTimer = new System.Timers.Timer(10000);

            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += this.OnTimedEvent;

           

            Logger.Log.Info("Thread start");
            
            integrationImplementation = new TFSIntegrationImplementation();
            Logger.Log.Info($"Read configuration from {TFSIntegrationImplementation.CONFIG_FILE}");
            TFSConfiguration tfsConfiguration = integrationImplementation.ReadTFSConfiguration(TFSIntegrationImplementation.CONFIG_FILE);
            
            // Set the Interval to 5 minutes
            if (tfsConfiguration != null && tfsConfiguration.RepetTaskEveryXSeconds != 0)
            {
                aTimer.Interval = TimeSpan.FromSeconds(tfsConfiguration.RepetTaskEveryXSeconds).TotalMilliseconds;
            }
            else
            {                
                aTimer.Interval = tfsConfiguration?.RepetTaskEveryXSeconds ?? 300000;
            }

            aTimer.Enabled = true;
            Logger.Log.Info("End onstart");
        }

        public void RunAsConsole(string[] args)
        {
            Logger.Log.Info("Running in console mode!");
            TFSIntegrationImplementation integrationImplementation = new TFSIntegrationImplementation();
            integrationImplementation.Run();
        }

        // Specify what you want to happen when the Elapsed event is 
        // raised.
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Logger.Log.Info($"The Elapsed event was raised at {e.SignalTime}. Start branch syncronization!");
            integrationImplementation?.Run(tfsConfiguration);
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
}
