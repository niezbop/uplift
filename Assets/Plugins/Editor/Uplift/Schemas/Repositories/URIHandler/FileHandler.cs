using System;
using System.IO;
using System.Text.RegularExpressions;
using Uplift.Common;

namespace Uplift.Schemas
{
    public class FileHandler : IURIHandler
    {
        public TemporaryDirectory OpenURI(string uri)
        {
            TemporaryDirectory td = new TemporaryDirectory();

            string path = Regex.Replace(uri, "^file://", string.Empty);

            if(Directory.Exists(path))
            {
                Uplift.Common.FileSystemUtil.CopyDirectoryWithMeta(path, Path.Combine(td.Path, Path.GetFileName(path)));
            }
            else if(File.Exists(path))
            {
                File.Copy(path, Path.Combine(td.Path, Path.GetFileName(path)));
            }
            else
                throw new ApplicationException("Could not resolve what is at " + path);

            return td;
        }
    }
}