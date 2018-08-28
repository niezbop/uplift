using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Uplift.Export
{
    [CustomEditor(typeof(PackageExportData))]
    public class PackageExportDataInspector : Editor
    {
        private PackageExportData packageExportData;
        private bool showPathspathsToExport = true;
        private bool showDependencyList = true;
        private bool showSpecPathList = true;
        private string templateUpsetFile = "";

        public void OnEnable()
        {
            packageExportData = (PackageExportData)target;
            if(packageExportData.pathsToExport == null)
                packageExportData.pathsToExport = new string[0];
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Basic Package Information", EditorStyles.boldLabel);
            packageExportData.packageName  = EditorGUILayout.TextField("Package name", packageExportData.packageName);
            packageExportData.packageVersion  = EditorGUILayout.TextField("Package version", packageExportData.packageVersion);
            packageExportData.license  = EditorGUILayout.TextField("License", packageExportData.license);

#region UpsetTemplateReplacement
    #region ImportFromUpset
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Additional Package Information", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("The template upset file is used to get the dependencies and the configuration of the package", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Import from template"))
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Uplift.Schemas.Upset));

                if (string.IsNullOrEmpty(templateUpsetFile))
                {
                    Debug.LogWarning("No template Upset specified, dependencies and configuration will not follow through");
                }
                else
                {
                    using (System.IO.FileStream fs = new System.IO.FileStream(templateUpsetFile, System.IO.FileMode.Open))
                    {
                        Uplift.Schemas.Upset template = serializer.Deserialize(fs) as Uplift.Schemas.Upset;
                        packageExportData.dependencyList = template.Dependencies;
                        packageExportData.specPathList = template.Configuration;
                    }
                }
            }

            EditorGUILayout.LabelField("File name", GUILayout.Width(60f));
            templateUpsetFile = AssetDatabase.GetAssetPath(
                EditorGUILayout.ObjectField(
                    AssetDatabase.LoadMainAssetAtPath(templateUpsetFile),
                    typeof(UnityEngine.Object),
                    false
                    )
                );

            EditorGUILayout.EndHorizontal();
            EditorUtility.SetDirty(packageExportData);
    #endregion ImportFromUpset
    #region DependencyList
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Dependency Settings", EditorStyles.boldLabel);
            showDependencyList = EditorGUILayout.Foldout(showDependencyList, "Dependency list");

            if(showDependencyList)
            {
                EditorGUI.indentLevel += 1;

                for(int i = 0; i < packageExportData.dependencyList.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    Schemas.DependencyDefinition dependency = packageExportData.dependencyList[i];

                    EditorGUILayout.LabelField("Name", GUILayout.Width(55f));
                    dependency.Name = EditorGUILayout.TextField(dependency.Name);
                    EditorGUILayout.LabelField("Version", GUILayout.Width(65f));
                    dependency.Version = EditorGUILayout.TextField(dependency.Version);

                    packageExportData.dependencyList[i] = dependency;

                    if(GUILayout.Button("X", GUILayout.Width(20.0f)))
                    {
                        var tempDependencyList = new List<Schemas.DependencyDefinition>(packageExportData.dependencyList);
                        tempDependencyList.RemoveAt(i);
                        packageExportData.dependencyList = tempDependencyList.ToArray();
                        EditorUtility.SetDirty(packageExportData);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel -= 1;

                if(GUILayout.Button("+"))
                {
                    Array.Resize<Schemas.DependencyDefinition>(ref packageExportData.dependencyList, packageExportData.dependencyList.Length + 1);
                    packageExportData.dependencyList[packageExportData.dependencyList.Length - 1] = new Schemas.DependencyDefinition();
                    EditorUtility.SetDirty(packageExportData);
                }
            }

            EditorUtility.SetDirty(packageExportData);
    #endregion DependencyList
    #region SpecPathList
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("SpecPath Settings", EditorStyles.boldLabel);
            showSpecPathList = EditorGUILayout.Foldout(showSpecPathList, "SpecPath list");

            if (showSpecPathList)
            {
                EditorGUI.indentLevel += 1;

                for (int i = 0; i < packageExportData.specPathList.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    Schemas.InstallSpecPath spec = packageExportData.specPathList[i];

                    EditorGUILayout.LabelField("Type", GUILayout.Width(55f));
                    spec.Type = (Schemas.InstallSpecType)EditorGUILayout.EnumPopup(spec.Type);
                    EditorGUILayout.LabelField("Path", GUILayout.Width(55f));

                    spec.Path = AssetDatabase.GetAssetPath(
                        EditorGUILayout.ObjectField(
                            AssetDatabase.LoadMainAssetAtPath(spec.Path),
                            typeof(UnityEngine.Object),
                            false
                        )
                    );

                    packageExportData.specPathList[i] = spec;

                    if (GUILayout.Button("X", GUILayout.Width(20.0f)))
                    {
                        var tempspecPathList = new List<Schemas.InstallSpecPath>(packageExportData.specPathList);
                        tempspecPathList.RemoveAt(i);
                        packageExportData.specPathList = tempspecPathList.ToArray();
                        EditorUtility.SetDirty(packageExportData);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel -= 1;

                if (GUILayout.Button("+"))
                {
                    Array.Resize<Schemas.InstallSpecPath>(ref packageExportData.specPathList, packageExportData.specPathList.Length + 1);
                    packageExportData.specPathList[packageExportData.specPathList.Length - 1] = new Schemas.InstallSpecPath();
                    EditorUtility.SetDirty(packageExportData);
                }
            }

            EditorUtility.SetDirty(packageExportData);
    #endregion SpecPathList
#endregion UpsetTemplateReplacement

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Export Settings", EditorStyles.boldLabel);
            packageExportData.targetDir = EditorGUILayout.TextField("Build destination directory", packageExportData.targetDir);
            showPathspathsToExport = EditorGUILayout.Foldout(showPathspathsToExport, "Paths to export");
            if(showPathspathsToExport)
            {
                EditorGUI.indentLevel += 1;
                for(int i = 0; i < packageExportData.pathsToExport.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    packageExportData.pathsToExport[i] = AssetDatabase.GetAssetPath(
                        EditorGUILayout.ObjectField(
                            "Item to export",
                            AssetDatabase.LoadMainAssetAtPath(packageExportData.pathsToExport[i]),
                            typeof(UnityEngine.Object),
                            false
                        )
                    );
                    if(GUILayout.Button("X", GUILayout.Width(20.0f)))
                    {
                        var tempPathspathsToExport = new List<string>(packageExportData.pathsToExport);
                        tempPathspathsToExport.RemoveAt(i);
                        packageExportData.pathsToExport = tempPathspathsToExport.ToArray();
                        EditorUtility.SetDirty(packageExportData);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel -= 1;
                if(GUILayout.Button("+"))
                {
                    Array.Resize<string>(ref packageExportData.pathsToExport, packageExportData.pathsToExport.Length + 1);
                    EditorUtility.SetDirty(packageExportData);
                }
            }

            EditorUtility.SetDirty(packageExportData);
        }
    }
}