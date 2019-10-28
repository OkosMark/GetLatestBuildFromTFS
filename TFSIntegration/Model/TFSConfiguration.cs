using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSIntegration
{
    public class TFSConfiguration
    {
        public Uri TfsUrl { get; set; }

        public List<TFSCopySettings> TFSSettings { get; set; }
    }
}
