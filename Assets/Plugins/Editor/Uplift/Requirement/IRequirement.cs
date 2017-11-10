using Uplift.Schemas;

namespace Uplift.Requirement
{
    public interface IRequirement
    {
        bool IsMetBy(Upset package);
        bool IsMetBy(InstalledPackage package);
        IRequirement RestrictTo(IRequirement other);
    }
}