﻿using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TFSIntegration
{
    public class TFSIntegrationImplementation
    {
        /// <summary>
        /// The configuration file
        /// </summary>
        public const string CONFIG_FILE = "Configuration\\TFSIntegrationSettings.json";       

        public void Run(TFSConfiguration tfsConfiguration = null)
        {
            Logger.Log.Info("Run TFS integration");            

            if (tfsConfiguration == null)
            {
                Logger.Log.Info($"Application runs from this location: {System.Reflection.Assembly.GetExecutingAssembly().Location}");
                Logger.Log.Info("Read configuration");
                tfsConfiguration = ReadTFSConfiguration(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), CONFIG_FILE));
            }

            Logger.Log.Info($"Authenticate to server {tfsConfiguration.TfsUrl}");
            NetworkCredential credentials = CredentialUtil.GetCredential(
                tfsConfiguration.TfsUrl.AbsoluteUri.Substring(0, tfsConfiguration.TfsUrl.AbsoluteUri.Length - tfsConfiguration.TfsUrl.AbsolutePath.Length + 1));
            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(tfsConfiguration.TfsUrl, credentials);
            tfs.Authenticate();


            Logger.Log.Info("Get build server!");
            IBuildServer buildServer = tfs.GetService<IBuildServer>();
            Logger.Log.Info($"Build server version: {buildServer.BuildServerVersion}!");

            Parallel.ForEach(tfsConfiguration.TFSSettings, (tfsIntegrationSettings) =>
            {
                Logger.Log.Info($"Get all build definitions from team project {tfsIntegrationSettings.TeamProject}.");
                IBuildDefinition[] buildDefinitions = buildServer.QueryBuildDefinitions(tfsIntegrationSettings.TeamProject, QueryOptions.Definitions);

                if (buildDefinitions == null || !buildDefinitions.Any())
                {
                    Logger.Log.Info("No build definitions found! Exiting!");
                    return;
                }

                Logger.Log.Info($"Found {buildDefinitions.Length} build definitions.");
                IBuildDefinition buildDefinition =
                    buildDefinitions.Where(build => build.Name == tfsIntegrationSettings.BuildDefinitionName)
                        .Select(b => b)
                        .FirstOrDefault();

                Logger.Log.Info($"Get build details from {buildDefinition.Name}.");
                IBuildDetail buildDetail = GetLatestBuildDetails(buildServer, buildDefinition,
                    tfsIntegrationSettings.TeamProject);

                if (buildDetail != null)
                {
                    DownloadVantageInstaller(
                        Path.Combine(buildDetail.DropLocation, tfsIntegrationSettings.SourcePathFragment,
                            tfsIntegrationSettings.SourceFile),
                        Path.Combine(tfsIntegrationSettings.CopyTo, tfsIntegrationSettings.BuildDefinitionName,
                            buildDetail.BuildNumber, tfsIntegrationSettings.SourceFile));
                }

                Logger.Log.Info("Cleanup old builds");
                CleanUp(Path.Combine(tfsIntegrationSettings.CopyTo, tfsIntegrationSettings.BuildDefinitionName),
                    tfsIntegrationSettings.MaxBuildsToKeep);
                Logger.Log.Info("FINISHED");
            });
        }

        private IBuildDetail GetLatestBuildDetails(IBuildServer buildServer, IBuildDefinition def, string teamProjectName)
        {
            IBuildDetailSpec spec = buildServer.CreateBuildDetailSpec(teamProjectName, def.Name);
            spec.MaxBuildsPerDefinition = 1;
            spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
            spec.Status = BuildStatus.Succeeded;
            spec.Reason = BuildReason.All ^ BuildReason.ValidateShelveset;

            var builds = buildServer.QueryBuilds(spec);
            return builds.Builds.FirstOrDefault();
        }

        private void DownloadVantageInstaller(string source, string destination)
        {
            string pathToDownload = Path.GetDirectoryName(destination) + "_Downloading";
            string pathToFileToDownload = Path.Combine(pathToDownload, Path.GetFileName(destination));

            if (File.Exists(destination))
            {
                Logger.Log.Info($"File {destination} already exists! Exiting!");
                return;
            }

            if (File.Exists(pathToFileToDownload))
            {
                Logger.Log.Info($"File {pathToFileToDownload} is beeing downloaded!");
                return;
            }

            if (!Directory.Exists(pathToDownload))
            {
                Logger.Log.Info($"Create directory temp directory {pathToDownload}");
                Directory.CreateDirectory(pathToDownload);
            }

            using (WebClient webClient = new WebClient())
            {
                Logger.Log.Info($"Download file from {source} to {destination}");
                webClient.DownloadFile(source, pathToFileToDownload);
            }

            if (Directory.Exists(pathToDownload))
            {
                Logger.Log.Info($"Rename directory from {pathToDownload} to {Path.GetDirectoryName(destination)}");
                Directory.Move(pathToDownload, Path.GetDirectoryName(destination));
            }
        }

        public TFSConfiguration ReadTFSConfiguration(string configPath)
        {
            TFSConfiguration config = null;

            using (StreamReader file = File.OpenText(configPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                config = (TFSConfiguration)serializer.Deserialize(file, typeof(TFSConfiguration));
            }

            return config;
        }

        private void CleanUp(string pathToClean, int maxItemsToKeep)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(pathToClean);
            var result = directoryInfo.GetDirectories().OrderByDescending(t => t.CreationTime).ToList();

            if (!result.Any() || result.Count <= maxItemsToKeep)
            {
                Logger.Log.Info($"Nothing to delete! The threshold of {maxItemsToKeep} was not met.");
                return;
            }

            for (int i = maxItemsToKeep; i < result.Count(); i++)
            {
                Logger.Log.Info($"Delete build {result[i].Name}");
                result[i].Delete(true);
            }
        }
    }
}
