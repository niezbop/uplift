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
using Uplift.Common;

namespace Uplift.Schemas {
    public partial class GitRepository : Repository
    {
        [System.Xml.Serialization.XmlIgnore]
        public Upset package;
        public override TemporaryDirectory DownloadPackage(Upset _package)
        {
            if(package != _package)
            {
                throw new ArgumentException(string.Format(
                    "Package {0} ({1}) can't be downloaded from this git repository!",
                    _package.PackageName,
                    _package.PackageVersion
                ));
            }

            return new TemporaryGitClone(urlField);
        }

        public override Upset[] ListPackages()
        {
            return new Upset[] { package };
        }
        
        public override string ToString()
        {
            return "Git repository at " + urlField;
        }
    }
}
