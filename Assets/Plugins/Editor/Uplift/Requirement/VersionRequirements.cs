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

using Uplift.Requirement;
using Uplift.Schemas;
using Uplift.Common;

namespace Uplift.Requirement
{
    public abstract class VersionRequirement : IRequirement
    {
        public virtual bool IsMetBy(Upset package)
        {
            return IsMetBy(package.PackageVersion);
        }
        public virtual bool IsMetBy(InstalledPackage package)
        {
            return IsMetBy(package.Version);
        }
        public virtual bool IsMetBy(string versionString)
        {
            return IsMetBy(VersionParser.ParseVersion(versionString));
        }
        public abstract bool IsMetBy(Version version);
        public abstract IRequirement RestrictTo(IRequirement other);
    }

    // When minimal+ is specified
    public class MinimalVersionRequirement : VersionRequirement
    {
        public Version minimal;

        public MinimalVersionRequirement(string minimal) : this(VersionParser.ParseIncompleteVersion(minimal)) { }
        public MinimalVersionRequirement(Version minimal)
        {
            this.minimal = minimal;
        }

        public override bool IsMetBy(Version version)
        {
            return version >= minimal;
        }

        public override IRequirement RestrictTo(IRequirement other)
        {
            if(other is NoRequirement)
            {
                return other.RestrictTo(this);
            }
            else if (other is MinimalVersionRequirement)
            {
                return minimal >= (other as MinimalVersionRequirement).minimal ? this : other;
            }
            else if (other is LoseVersionRequirement)
            {
                if (minimal <= (other as LoseVersionRequirement).stub) return other;
            }
            else if (other is BoundedVersionRequirement)
            {
                if (minimal <= (other as BoundedVersionRequirement).lowerBound) return other;
            }
            else if (other is ExactVersionRequirement)
            {
                if (minimal <= (other as ExactVersionRequirement).expected) return other;
            }
            throw new IncompatibleRequirementException(this, other);
        }

        public override string ToString()
        {
            return minimal.ToString() + "+";
        }
    }

    // When stub is specified
    public class LoseVersionRequirement : VersionRequirement
    {
        public Version stub;
        private Version limit;

        public LoseVersionRequirement(string stub) : this(VersionParser.ParseIncompleteVersion(stub)) { }
        public LoseVersionRequirement(Version stub)
        {
            this.stub = stub;
            limit = stub.Next;
        }

        public override bool IsMetBy(Version version)
        {
            return version >= stub && version < limit;
        }

        public override IRequirement RestrictTo(IRequirement other)
        {
            if (other is NoRequirement || other is MinimalVersionRequirement)
            {
                return other.RestrictTo(this);
            }
            else if (other is LoseVersionRequirement)
            {
                if (IsMetBy((other as LoseVersionRequirement).stub)) return other;
                if ((other as LoseVersionRequirement).IsMetBy(stub)) return this;
            }
            else if(other is BoundedVersionRequirement)
            {
                if (IsMetBy((other as BoundedVersionRequirement).lowerBound)) return other;
                if ((other as BoundedVersionRequirement).IsMetBy(stub)) return this;
            }
            else if(other is ExactVersionRequirement)
            {
                if (IsMetBy((other as ExactVersionRequirement).expected)) return other;
            }
            throw new IncompatibleRequirementException(this, other);
        }

        public override string ToString()
        {
            return stub.ToString();
        }
    }

    public class BoundedVersionRequirement : VersionRequirement
    {
        public Version lowerBound;
        private Version upperBound;

        public BoundedVersionRequirement(string lowerBound) : this(VersionParser.ParseIncompleteVersion(lowerBound)) { }
        public BoundedVersionRequirement(Version lowerBound)
        {
            this.lowerBound = lowerBound;
            upperBound = lowerBound.Next;
        }

        public override bool IsMetBy(Version version)
        {
            return version > lowerBound && version < upperBound;
        }

        public override IRequirement RestrictTo(IRequirement other)
        {
            if (other is NoRequirement || other is MinimalVersionRequirement || other is LoseVersionRequirement)
            {
                return other.RestrictTo(this);
            }   
            else if(other is BoundedVersionRequirement)
            {
                if (IsMetBy((other as BoundedVersionRequirement).lowerBound)) return other;
                if ((other as BoundedVersionRequirement).IsMetBy(lowerBound)) return this;
            }
            else if(other is ExactVersionRequirement)
            {
                if (IsMetBy((other as ExactVersionRequirement).expected)) return other;
            }
            throw new IncompatibleRequirementException(this, other);
        }

        public override string ToString()
        {
            return lowerBound.ToString() + ".*";
        }
    }

    public class ExactVersionRequirement : VersionRequirement
    {
        public Version expected;

        public ExactVersionRequirement(string expected) : this(VersionParser.ParseIncompleteVersion(expected)) { }
        public ExactVersionRequirement(Version expected)
        {
            this.expected = expected;
        }

        public override bool IsMetBy(Version version)
        {
            return expected.Equals(version);
        }

        public override IRequirement RestrictTo(IRequirement other)
        {
            if (other is ExactVersionRequirement)
            {
                if (IsMetBy((other as ExactVersionRequirement).expected)) return this;
            }
            else
            {
                return other.RestrictTo(this);
            }
            throw new IncompatibleRequirementException(this, other);
        }

        public override string ToString()
        {
            return expected.ToString() + "!";
        }
    }
}
