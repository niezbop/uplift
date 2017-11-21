using Uplift.Schemas;

namespace Uplift.Requirement
{
    // When no requirement is specified
    public class NoRequirement : IRequirement
    {
        public bool IsMetBy(Upset package) { return true; }
        public bool IsMetBy(InstalledPackage package) { return true; }
        public IRequirement RestrictTo(IRequirement other) { return other; }
        public override string ToString() { return ""; }
    }
}