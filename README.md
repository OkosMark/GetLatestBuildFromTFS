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

-you can specify multiple TFSCopySettings.

Before running this application you could access the "TfsUrl" and login, by logging into TFS your credentials will be saved in the "Credentials Manager". By doing this you can avoid saving credentials in code or configuration files.

You can use this application in three modes:
-as a windows service by running InstallService.bat file that comes with the solution.
-as a standalone application which you can add to the Windows TaskSecheduler to run every x minutes, by doing this you will have the latest build downloaded and ready to be deployed.
-as a run on demand by executing the TFSIntegration.exe file.
