using System;

namespace Uplift.Common
{
    public class Version : IComparable, ICloneable
    {
        public int Major;
        public int? Minor, Patch, Optional;

        // This is to conform struct->class transition
        public Version() {}

        public Version(int Major, int? Minor, int? Patch, int? Optional) {
            this.Major = Major;
            this.Minor = Minor;
            this.Patch = Patch;
            this.Optional = Optional;
        }

        public object Clone() {
            return new Version(this.Major, this.Minor, this.Patch, this.Optional);
        }

        public int CompareTo(object other) {
            if(other == null) {
                return 1;
            }

            Version otherVersion = other as Version;

            if(otherVersion == null) {
                // Not a Version object
                return 1;
            }

            if(otherVersion > this) {
                return -1;
            } else {
                return 1;
            }
        }

        public Version Next
        {
            get
            {
                Version result = this.Clone() as Version;

                if (Minor != null)
                {
                    if (Patch != null)
                    {
                        if (Optional != null) { result.Optional += 1; }
                        else { result.Patch += 1; }
                    }
                    else
                    {
                        result.Minor += 1;
                    }
                }
                else
                {
                    result.Major += 1;
                }
                return result;
            }
        }

        public static bool operator <(Version a, Version b)
        {
            if (a.Major != b.Major) return a.Major < b.Major;
            bool result = false;
            if (TryCompareInt(a.Minor, b.Minor, ref result)) return result;
            if (TryCompareInt(a.Patch, b.Patch, ref result)) return result;
            if (TryCompareInt(a.Optional, b.Optional, ref result)) return result;
            return false;
        }
        public static bool operator >(Version a, Version b) { return b < a; }
        public static bool operator <=(Version a, Version b) { return !(a > b); }
        public static bool operator >=(Version a, Version b) { return !(a < b); }

        public static bool operator ==(Version a, Version b) {

            // Null checking...
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            return (
                a.Major == b.Major &&
                a.Minor == b.Minor &&
                a.Patch == b.Patch &&
                a.Optional == b.Optional
                );
        }
        public static bool operator !=(Version a, Version b) { return !(a == b); }
        public override int GetHashCode()
        {
            int result = Major;
            if (Minor != null) result = result & (int)Minor;
            if (Patch != null) result = result & (int)Patch;
            if (Optional != null) result = result & (int)Optional;
            return result;
        }
        public override bool Equals(object o)
        {
            return this == (Version)o;
        }

        private static bool TryCompareInt(int? a, int? b, ref bool result)
        {
            if (a != null)
            {
                if (b == null)
                {
                    result = false;
                    return true;
                }
                else if (a != b)
                {
                    result = a < b;
                    return true;
                }
            }
            else
            {
                // If a is null, versionA is X...Y
                // versionA is strictly lower than versionB if and only if versionB is X...Y.Z ie b is not null
                result = b != null;
                return true;
            }
            // Could not distinct versions
            return false;
        }

        public override string ToString()
        {
            string result = Major.ToString();
            if (Minor != null)
            {
                result += "." + Minor.ToString();
                if (Patch != null)
                {
                    result += "." + Patch.ToString();
                    if (Optional != null) { result += "." + Optional.ToString(); }
                }
            }
            return result;
        }
    }
}