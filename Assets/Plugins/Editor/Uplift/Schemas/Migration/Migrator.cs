using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Uplift.Schemas.Migration
{
    class Migrator<T>
    {
        public bool NeedMigration(string path)
        {
            int version;
            if(TryGetFileVersion(path, out version))
            {
                return version == LatestVersion();
            }
            return true;
        }

        public void MigrateToLatest(string path)
        {
            XmlDocument document = new XmlDocument();
            document.Load(path);
            int latest = LatestVersion();
            int current;
            if (!TryGetFileVersion(document, out current))
                Debug.Log("Could not load the file version of your Upbring. Assuming it is 1");
            while(current < latest)
            {
                document = Update(document, current);
                if(!TryGetFileVersion(document, out current))
                {
                    throw new FormatException("Cannot read the file at " + path);
                }
            }
            document.Save(path);
        }

        private XmlDocument Update(XmlDocument document, int fileVersion)
        {
            MethodInfo mInfo;
            object updater;
            try
            {
                Type updaterType = Type.GetType(string.Format("{0}UpdaterFrom{1}", typeof(T).ToString(), fileVersion));
                mInfo = updaterType.GetMethod("Update");
                ConstructorInfo updaterCtor = updaterType.GetConstructor(Type.EmptyTypes);
                updater = updaterCtor.Invoke(new object[] { });
            }
            catch(Exception e)
            {
                Debug.LogErrorFormat("Could not update the {0} from version {1}:\n{2}", typeof(T).ToString(), fileVersion, e);
                throw;
            }
            
            return (XmlDocument)mInfo.Invoke(updater, new object[] { document });
        }

        private bool TryGetFileVersion(string path, out int version)
        {
            XmlDocument document = new XmlDocument();
            document.Load(path);
            return TryGetFileVersion(document, out version);
        }

        private bool TryGetFileVersion(XmlDocument document, out int version)
        {
            version = 0;
            XmlElement root = document.DocumentElement;
            try
            {
                version = int.Parse(root.Attributes["FileVersion"].Value);
                return true;
            }
            catch(NullReferenceException)
            {
                root.SetAttribute("FileVersion", "0");
            }
            catch (FormatException)
            {
                root.Attributes["FileVersion"].Value = "0";
            }
            return false;
        }

        private int LatestVersion()
        {
            Type currentType = typeof(T);
            int version;

            version = (int)currentType.GetField("fileVersion").GetValue(null);

            return version;
        }
    }
}
