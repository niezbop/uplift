// --- BEGIN LICENSE BLOCK ---
/*
 * Copyright (c) 2017-present WeWantToKnow AS
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
// --- END LICENSE BLOCK ---

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;
using Uplift.Extensions;
using UnityEngine;
using UnityEditor;
using Uplift.Common;
using Version = Uplift.Common.Version;
using System.Text.RegularExpressions;

namespace Uplift.Schemas {

    public partial class FileRepository
    {
        public override string ToString()
        {
            return "FileRepository " + this.Path;
        }

        public override TemporaryDirectory DownloadPackage(Upset package) {
            IURIHandler uriHandler;

            string uri = package.PackageURI;

            if(uri.StartsWith("file://"))
                uriHandler = new FileHandler();
            else if(uri.StartsWith("http://"))
                uriHandler = new HttpHandler();
            else if(uri.StartsWith("https://"))
                uriHandler = new HttpHandler();
            else
                throw new NotSupportedException("Uplift only supports \"file://something\" for URI in Upsets for now");

            TemporaryDirectory td = uriHandler.OpenURI(uri);
            
            string[] entries = Directory.GetFileSystemEntries(td.Path);
            if(entries == null || entries.Length == 0)
                throw new ApplicationException(string.Format(
                    "opening URI at {0} (in package {1}) did not retrieve anything",
                    uri,
                    package.PackageName
                ));

            string sourcePath = Directory.GetFileSystemEntries(td.Path)[0];

            if (IsUnityPackage(sourcePath))
            {
                var unityPackage = new UnityPackage();
                unityPackage.Extract(sourcePath, td.Path);
            }
            else if (!Directory.Exists(sourcePath))
            {
                Debug.LogError(string.Format("Package {0} version {1} found at {2} has an unexpected format and cannot be downloaded ", package.PackageName, package.PackageVersion, sourcePath));
            }

            return td;
        }

        private static bool IsUnityPackage(string Path)
        {
            return File.Exists(Path) && ".unitypackage".Equals(System.IO.Path.GetExtension(Path), StringComparison.CurrentCultureIgnoreCase);
        }

        public override Upset[] ListPackages() {
            List<Upset> upsetList = new List<Upset>();

            string[] files = Directory.GetFiles(Path);
            StrictXmlDeserializer<Upset> deserializer = new StrictXmlDeserializer<Upset>();

            foreach(string file in files)
            {
                if(!file.EndsWith(".Upset.xml")) continue;
                
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    Upset upset = deserializer.Deserialize(fs);
                    if (upset.Configuration != null && upset.Configuration.Length != 0)
                    {
                        foreach (InstallSpecPath spec in upset.Configuration)
                        {
                            spec.Path = Uplift.Common.FileSystemUtil.MakePathOSFriendly(spec.Path);
                        }
                    }
                    upset.MetaInformation.dirName = Regex.Replace(file, ".Upset.xml$", ".unitypackage", RegexOptions.IgnoreCase);
                    upsetList.Add(upset);
                }
            }

            return upsetList.ToArray();
        }
    }
}