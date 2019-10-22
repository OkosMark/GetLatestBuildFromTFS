using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Newtonsoft.Json;

namespace TFSIntegration
{
    class Program
    {
        /// <summary>
        /// The configuration file
        /// </summary>
        private const string CONFIG_FILE = "TFSIntegrationSettings.json";

        static void Main(string[] args)
        {  
            Logger.Log.Info("START");
            Logger.Log.Info($"Application runs from this location: {System.Reflection.Assembly.GetExecutingAssembly().Location}");

            Logger.Log.Info("Read configuration!");
            Settings tfsIntegrationSettings = ReadConfiguration(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), CONFIG_FILE));

            Logger.Log.Info($"Authenticate to server {tfsIntegrationSettings.TfsUrl}");
            

            NetworkCredential credentials = CredentialUtil.GetCredential(
                tfsIntegrationSettings.TfsUrl.AbsoluteUri.Substring(0, tfsIntegrationSettings.TfsUrl.AbsoluteUri.Length - tfsIntegrationSettings.TfsUrl.AbsolutePath.Length + 1));
            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(tfsIntegrationSettings.TfsUrl, credentials);
            tfs.Authenticate();
            

            Logger.Log.Info("Get build server!");
            IBuildServer buildServer = tfs.GetService<IBuildServer>();
            Logger.Log.Info($"Build server version: {buildServer.BuildServerVersion}!");

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
        }

        private static IBuildDetail GetLatestBuildDetails(IBuildServer buildServer, IBuildDefinition def, string teamProjectName)
        {
            IBuildDetailSpec spec = buildServer.CreateBuildDetailSpec(teamProjectName, def.Name);
            spec.MaxBuildsPerDefinition = 1;
            spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
            spec.Status = BuildStatus.Succeeded;

            var builds = buildServer.QueryBuilds(spec);
            return builds.Builds.FirstOrDefault();
        }

        private static void DownloadVantageInstaller(string source, string destination)
        {
            if (!Directory.Exists(Path.GetDirectoryName(destination)))
            {
                Logger.Log.Info($"Create directory {Path.GetDirectoryName(destination)}");
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
            }

            if (File.Exists(destination))
            {
                Logger.Log.Info($"File {source} already exists! Exiting!");
                return;
            }

            using (WebClient webClient = new WebClient())
            {
                Logger.Log.Info($"Download file from {source} to {destination}");
                webClient.DownloadFile(source, destination);
            }
        }
       
        private static Settings ReadConfiguration(string configPath)
        {
            Settings config = null;
         
            using (StreamReader file = File.OpenText(configPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                config = (Settings)serializer.Deserialize(file, typeof(Settings));
            }

            return config;
        }

        private static void CleanUp(string pathToClean, int maxItemsToKeep)
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
