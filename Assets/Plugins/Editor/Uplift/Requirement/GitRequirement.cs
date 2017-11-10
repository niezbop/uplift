using Uplift.Common;
using Uplift.Schemas;

namespace Uplift.Requirement
{
    public class GitRequirement : IRequirement
    {
        public VersionRequirement innerRequirement;
        public string uri;

        public GitRequirement(string uri)
        {
            // TODO: Get git package and load its upset to get meaningful information
            string fetchedPackageVersion = "0.0.0";
            IRequirement requirement = RequirementParser.ParseRequirement(fetchedPackageVersion);
            if(!(requirement is VersionRequirement))
                throw new System.ApplicationException("Pouet");

            innerRequirement = (VersionRequirement)requirement;
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
    }
}