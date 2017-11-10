using System;
using Uplift.Common;

namespace Uplift.Requirement
{
    public class RequirementParser
    {
        public static IRequirement ParseRequirement(string requirement)
        {
            if (string.IsNullOrEmpty(requirement))
            {
                return new NoRequirement();
            }
            else if (requirement.EndsWith("!"))
            {
                return new ExactVersionRequirement(requirement.TrimEnd('!'));
            }
            else if (requirement.EndsWith("+"))
            {
                return new MinimalVersionRequirement(requirement.TrimEnd('+'));
            }
            else if (requirement.EndsWith(".*"))
            {
                return new BoundedVersionRequirement(requirement.TrimEnd('*').TrimEnd('.'));
            }
            else
            {
                if(VersionParser.ParseVersion(requirement) != new Uplift.Common.Version { Major = 0 })
                    return new LoseVersionRequirement(requirement);
            }
            throw new ArgumentException("Cannot parse requirement from " + requirement);
        }
    }
}