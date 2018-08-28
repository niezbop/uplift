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

using UnityEngine;
using UnityEditor;
using System;

namespace Uplift.Export
{
#if UNITY_5_1_OR_NEWER
    [CreateAssetMenuAttribute(fileName = "PackageExport.asset", menuName = "Uplift/Package Export Definition", order = 250)]
#endif
    class PackageExportData : ScriptableObject, ICloneable
    {
        public  string    packageName     =  "";
        public  string    packageVersion  =  "";
        public  string    license         =  "";
        public  string    targetDir       =  "target";
        public string[]   pathsToExport   = new string[0];

        public Schemas.DependencyDefinition[] dependencyList = new Schemas.DependencyDefinition[0];
        public Schemas.InstallSpecPath[] specPathList = new Schemas.InstallSpecPath[0];

        public object Clone()
        {
            return UnityEngine.Object.Instantiate(this) as PackageExportData;
        }

        public void SetOrCheckOverridenDefaults(PackageExportData defaults)
        {
            string[] overridableDefaults = {"packageName", "packageVersion", "license", "targetDir"};

            foreach(var fieldName in overridableDefaults)
            {
                System.Reflection.FieldInfo field = this.GetType().GetField(fieldName);

                // I know those are strings, so...
                var defaultValue = field.GetValue(defaults) as string;
                var currentValue = field.GetValue(this) as string;

                if(string.IsNullOrEmpty(currentValue) && currentValue != defaultValue)
                {
                    Debug.LogFormat("NOTE: using default for Package Export Specification {0}: {1}", fieldName, defaultValue);
                    field.SetValue(this, defaultValue);
                }
            }
        }
    }
}
