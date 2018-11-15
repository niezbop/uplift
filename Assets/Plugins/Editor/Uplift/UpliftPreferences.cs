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
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Uplift.Schemas;

namespace Uplift
{
    public class UpliftPreferences : MonoBehaviour
    {
        private const string useExperimentalFeaturesKey = "UpliftExperimentalFeatures";
        private const string trustUnknownCertificatesKey = "UpliftUnknownCertificates";
        private const string githubProxyUseKey = "UpliftGithubProxyUseKey";
        private const string githubProxyUrlKey = "UpliftGithubProxyUrlKey";

        private static UpliftSettings settingsCached = null;
        private static UpliftSettings GetSettings(bool refresh = false)
        {
            if (refresh || settingsCached == null)
                settingsCached = UpliftSettings.FromDefaultFile();

            bool changed = false;

            changed &= LoadLegacy(settingsCached.UseExperimentalFeatures, useExperimentalFeaturesKey);
            changed &= LoadLegacy(settingsCached.TrustUnknowCertificates, trustUnknownCertificatesKey);
            changed &= LoadLegacy(settingsCached.UseGithubProxy, githubProxyUseKey);
            changed &= LoadLegacy(settingsCached.GithubProxyUrl, githubProxyUrlKey);

            if (changed)
                settingsCached.SaveToDefaultFile();

            return settingsCached;
        }

        public static bool UseExperimental()
        {
            return GetSettings().UseExperimentalFeatures;
        }

        public static bool TrustUnknownCertificates()
        {
            return GetSettings().TrustUnknowCertificates;
        }

        public static string UseGithubProxy(string url)
        {
            var settings = GetSettings();

            if (!settings.UseGithubProxy || string.IsNullOrEmpty(settings.GithubProxyUrl))
                return url;

            Debug.Log("Proxying github api with " + settings.GithubProxyUrl);
            return url.Replace("https://api.github.com", settings.GithubProxyUrl);
        }

        private static bool LoadLegacy(bool variable, string legacyKey)
        {
            if (EditorPrefs.HasKey(legacyKey))
            {
                var legacyValue = EditorPrefs.GetBool(legacyKey);
                if(legacyValue != variable)
                {
                    variable = legacyValue;
                    return true;
                }
            }

            return false;
        }

        private static bool LoadLegacy(string variable, string legacyKey)
        {
            if(EditorPrefs.HasKey(legacyKey))
            {
                var legacyValue = EditorPrefs.GetString(legacyKey);
                if (!string.IsNullOrEmpty(legacyValue) && legacyValue != variable)
                {
                    variable = legacyValue;
                    return true;
                }
            }

            return false;
        }
    }
}
