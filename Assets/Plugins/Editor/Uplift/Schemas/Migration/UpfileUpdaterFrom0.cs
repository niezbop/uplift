using System.Xml;
using Uplift.Schemas.Migration;

namespace Uplift.Schemas
{
    class UpfileUpdaterFrom0 : IFileUpdater
    {
        public XmlDocument Update(XmlDocument doc)
        {
            XmlDocument result = doc;
            result.DocumentElement.Attributes["FileVersion"].Value = "1";
            return result;
        }
    }
}
