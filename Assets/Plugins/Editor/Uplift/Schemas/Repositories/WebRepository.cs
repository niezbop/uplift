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
using System.Net;
using Uplift.Common;

namespace Uplift.Schemas
{
    public partial class WebRepository : Repository
    {
        public override Upset[] ListPackages()
        {
            UpsetManifest manifest;
            try
            {
                StrictXmlDeserializer<UpsetManifest> deserializer = new StrictXmlDeserializer<UpsetManifest>();

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                using (var response = WebRequest.Create(this.Url).GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    manifest = deserializer.Deserialize(stream);
                }
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.LogErrorFormat(
                    "There was an error loading the manifest at {0}:\n{1}",
                    this.Url,
                    ex
                );
                return new Upset[0];
            }
            
            return manifest.UpsetList;
        }
    }
}
