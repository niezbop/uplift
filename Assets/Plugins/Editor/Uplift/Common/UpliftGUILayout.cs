using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using Uplift.Schemas;

namespace Uplift.Common.Editor
{
    public class UpliftGUILayout
    {
        public static DependencyDefinition DependencyField(string label, DependencyDefinition def)
        {
            DependencyDefinition result = new DependencyDefinition()
            {
                Name = def.Name,
                Version = def.Version
            };

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            result.Name = EditorGUILayout.TextField(result.Name);
            result.Version = EditorGUILayout.TextField(result.Version);
            EditorGUILayout.EndHorizontal();

            return result;
        }

        public static InstallSpecPath InstallSpecPathField(string label, InstallSpecPath spec)
        {
            InstallSpecPath result = new InstallSpecPath
            {
                Path = spec.Path,
                Type = spec.Type
            };

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            result.Type = (InstallSpecType)EditorGUILayout.EnumPopup(result.Type);
            result.Path = EditorGUILayout.TextField(result.Path);
            EditorGUILayout.EndHorizontal();

            return result;
        }
    }
}
