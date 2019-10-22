using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CredentialManagement;

namespace TFSIntegration
{
    public static class CredentialUtil
    {
        public static NetworkCredential GetCredential(string target)
        {
            var cm = new Credential { Target = target };
            if (!cm.Load())
            {
                return null;
            }
            
            if (cm.Username.Contains("\\"))
            {
                var usernameSplit = cm.Username.Split('\\');
                var domain = usernameSplit[0];
                var username = usernameSplit[1];

                return new NetworkCredential(username, cm.Password, domain);
            }

            // UserPass is just a class with two string properties for user and pass
            return new NetworkCredential(cm.Username, cm.Password);
        }
    }
}
