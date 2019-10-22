using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSIntegration
{
    public class Settings
    {
        public Uri TfsUrl { get; set; }
        public string TeamProject { get; set; }
        public string BuildDefinitionName { get; set; }
        public string CopyTo { get; set; }
        public string SourcePathFragment { get; set; }
        public string SourceFile { get; set; }
        public int MaxBuildsToKeep { get; set; }
    }
}
