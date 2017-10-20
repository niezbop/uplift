using Uplift.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Uplift.Strategies {
    internal class OnlyMatchingUnityVersionStrategy : CandidateSelectionStrategy {

        Version unityVersion;

        public OnlyMatchingUnityVersionStrategy(string unityVersion) {
            this.unityVersion = VersionParser.ParseVersion(unityVersion, false);
        }

        public override PackageRepo[] Filter(PackageRepo[] candidates) {
            var result = new List<PackageRepo>();

            foreach(PackageRepo item in candidates) {
                // if lack of UnityVersion - continue gracefully but notify
                if(item.Package.UnityVersion == null) {
                    Debug.LogWarning("Package " + item.Package.ToString() + " doesn't have minimal UnityVersion specified!");
                    result.Add(item);
                    continue;
                }

                Version packageUnityRequirement = VersionParser.ParseVersion(item.Package.UnityVersion, false);

                if(unityVersion >= packageUnityRequirement) {
                    result.Add(item);
                }
            }

            return result.ToArray();
        }
    }
}