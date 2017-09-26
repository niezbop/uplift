using System.Xml;

namespace Uplift.Schemas.Migration
{
    interface IFileUpdater
    {
        XmlDocument Update(XmlDocument doc);
    }
}
