using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Uplift.Common;

namespace Uplift.Schemas
{
    public class HttpHandler : IURIHandler
    {
        public TemporaryDirectory OpenURI(string uri)
        {
            TemporaryDirectory td = new TemporaryDirectory();
            
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            using (var client = new WebClient())
            {
                client.DownloadFile(uri, Path.Combine(td.Path, uri.Split('/').Last()));
            }

            return td;
        }
    }
}