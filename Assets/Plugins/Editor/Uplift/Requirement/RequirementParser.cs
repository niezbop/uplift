using System;
using Uplift.Common;
using Version = Uplift.Common.Version;

namespace Uplift.Requirement
{
    public class RequirementParser
    {
        public static IRequirement ParseRequirement(string requirement)
        {
            if(VersionParser.ParseVersion(requirement, false) != new Version(-1, null, null, null))
            {
                return ParseVersionRequirement(requirement);
            }
            else if(requirement.StartsWith("git"))
            {
                return new GitRequirement(requirement);
            }
            else if(string.IsNullOrEmpty(requirement))
            {
                return new NoRequirement();
            }
            throw new ArgumentException("Cannot parse requirement from " + requirement);
        }

        private static VersionRequirement ParseVersionRequirement(string requirement)
        {
            if (requirement.EndsWith("!"))
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
            return new LoseVersionRequirement(requirement);
        }
    }

    public class IncompatibleRequirementException : Exception
    {
        public IncompatibleRequirementException() : base("Incompatible requirements were identified") { }
        public IncompatibleRequirementException(string message) : base(message) { }
        public IncompatibleRequirementException(IRequirement a, IRequirement b)
            : this(string.Format("Requirements {0} and {1} are not compatible", a.ToString(), b.ToString())) { }
        public IncompatibleRequirementException(string format, params object[] args) : base(string.Format(format, args)) { }
        public IncompatibleRequirementException(string message, Exception innerException) : base(message, innerException) { }
        public IncompatibleRequirementException(string format, Exception innerException, params object[] args) : base(string.Format(format, args), innerException) { }
    }
}