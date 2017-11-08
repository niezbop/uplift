using Uplift.Common;

namespace Uplift.Schemas
{
    public interface IURIHandler
    {
        TemporaryDirectory OpenURI(string uri);
    }
}