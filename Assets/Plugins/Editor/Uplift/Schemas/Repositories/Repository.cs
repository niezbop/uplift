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
using System.IO;
using Uplift.Common;
using UnityEngine;

namespace Uplift.Schemas {
    public abstract partial class Repository : IRepositoryHandler
    {
        public const string UpsetFile = "Upset.xml";
        public virtual Upset[] ListPackages()
        {
            throw new NotImplementedException();
        }

        public virtual TemporaryDirectory DownloadPackage(Upset package)
        {
            IURIHandler uriHandler;

            string uri = package.PackageURI;

            if(uri.StartsWith("file://"))
                uriHandler = new FileHandler();
            else if(uri.StartsWith("http://") || uri.StartsWith("https://"))
                uriHandler = new HttpHandler();
            else
                throw new NotSupportedException("Uplift only supports \"file://\", \"http://\" and \"https://\" for URI in Upsets for now");

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
        
        protected virtual bool IsUnityPackage(string Path)
        {
            return File.Exists(Path) && ".unitypackage".Equals(System.IO.Path.GetExtension(Path), StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
