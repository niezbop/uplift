using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Uplift.Common;
using Uplift.Schemas;

namespace Uplift.Requirement
{
    public class GitRequirement : IRequirement
    {
        public ExactVersionRequirement innerRequirement;
        public string url;

        public GitRequirement(string _url)
        {
            url = _url;
            string fetchedPackageVersion = null;
            string urlPattern = @"git:(.+)#?(.*)?";
            Regex reg = new Regex(urlPattern);
            Match match = reg.Match(url);
            string address = match.Groups[1].Value;
            string branch = "master";
            if(match.Groups.Count >= 3 && !string.IsNullOrEmpty(match.Groups[2].Value))
                branch = match.Groups[2].Value;

            using(TemporaryGitClone clone = new TemporaryGitClone(address, branch, true))
            {
                string[] candidateUpsets = System.IO.Directory.GetFiles(clone.RepositoryPath, "*Upset.xml", SearchOption.TopDirectoryOnly);
                if(candidateUpsets.Length < 1)
                    throw new System.ApplicationException("No Upset file in repository at " + url);

                if(candidateUpsets.Length > 1)
                    Debug.LogErrorFormat("There are too many Upset files in the git repository at {0}. Uplift may not behave as expected.");
                    
                StrictXmlDeserializer<Upset> deserializer = new StrictXmlDeserializer<Upset>();
                using (FileStream file = new FileStream(candidateUpsets[0], FileMode.Open))
                {
                    fetchedPackageVersion = deserializer.Deserialize(file).PackageVersion;
                }
            }
            
            innerRequirement = new ExactVersionRequirement(fetchedPackageVersion);
        }
        
        public bool IsMetBy(Upset package)
        {
            return innerRequirement.IsMetBy(package);
        }

        public bool IsMetBy(InstalledPackage package)
        {
            return innerRequirement.IsMetBy(package);
        }

        public IRequirement RestrictTo(IRequirement other)
        {
            throw new IncompatibleRequirementException("Git requirements are never compatible with any other requirement");
        }

        public override string ToString()
        {
            return url;
        }
    }
}