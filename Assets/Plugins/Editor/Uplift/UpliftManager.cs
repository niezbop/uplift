using System;
using System.Collections.Generic;

using Uplift.Common;
using Uplift.Requirement;
using Uplift.Schemas;

namespace Uplift
{
    public class UpliftManager
    {
        private List<IRepositoryHandler> repositories;
        private List<IRequirement> requirements;

        private void LoadUpfile() { LoadUpfile(Upfile.Instance()); }
        private void LoadUpfile(Upfile upfile)
        {

        }

        private void LoadUpbring() { LoadUpbring(Upbring.Instance()); }
        private void LoadUpbring(Upbring upbring)
        {

        }

        internal void Register(object obj)
        {
            if(obj is IRequirement) requirements.Add(obj as IRequirement);
        }
    }
}