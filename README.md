# GetLatestBuildFromTFS

The scope of this application is to get the drop location from TFS of the latest successful build and download it to a local folder.

To run this application you must first configure the TFSIntegrationSettings.json like this:

{ 
   "TfsUrl":"http://MyTfsServer.MyDomain.com:port/tfs/MyCollection",
   "TFSCopySettings": [
     {
      "TeamProject":"MyTeamProjectName",
      "BuildDefinitionName":"MyBuildDefinitionName",
      "CopyTo":"LocalPathToCopyTheFile",
      "SourcePathFragment":"Some e.g. Release\\FullInstaller",
      "SourceFile":"The installer/filename e.g. intaller.msi",
      "MaxBuildsToKeep":5 --the max number of builds to keep based on date
      }
    ]
}

Before running this application you could first access the "TfsUrl" and login. By logging into TFS your credentials will be saved in the "Credentials Manager". By doing this you can avoid saving passwords in code or configuration files.

You can add this application to Windows TaskSecheduler to run every x minutes, by doing this you will have the latest build downloaded and ready to be deployed.
