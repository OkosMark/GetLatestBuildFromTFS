﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
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
        private static readonly object padlock = new object();

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
            Logger.Log.Info($"Application runs from this location: {System.Reflection.Assembly.GetExecutingAssembly().Location}");
            Logger.Log.Info("Read configuration");
            tfsConfiguration = integrationImplementation.ReadTFSConfiguration(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), TFSIntegrationImplementation.CONFIG_FILE));
            
            // Set the Interval
            if (tfsConfiguration != null && tfsConfiguration.RepetTaskEveryXSeconds != 0)
            {
                aTimer.Interval = TimeSpan.FromSeconds(tfsConfiguration.RepetTaskEveryXSeconds).TotalMilliseconds;
            }
            else
            {                
                aTimer.Interval = 300000;//5 minutes
            }

            Logger.Log.Info($"Downloading will start in {aTimer.Interval} seconds!");
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
            lock (padlock)
            {
                integrationImplementation?.Run(tfsConfiguration);
            }
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
}
