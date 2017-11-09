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

        public override Upset[] ListPackages()
        {
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