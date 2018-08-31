// --- BEGIN LICENSE BLOCK ---
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

using UnityEditor;
using UnityEngine;
using Uplift.Common;
using Uplift.Schemas;

namespace Uplift.Windows
{
	internal class UpdateUtility : EditorWindow
	{
		public enum VersionState
		{
			NOT_INSTALLED,
			OUTDATED,
			INCOMPATIBLE,
			NEW_VERSION_INCOMPATIBLE,
			UP_TO_DATE
		}

		private float indentSize = 20f;
		private float verticalSpace = 5f;
		private Vector2 scrollPosition;
		private UpliftManager.DependencyState[] states = new UpliftManager.DependencyState[0];
		private bool[] showDependency = new bool[0];
		private bool[] showSubdepedency = new bool[0];

		public void Init()
		{
			UpliftManager.ResetInstances();
			states = UpliftManager.Instance().GetDependenciesState();
			showDependency = new bool[states.Length];

			bool emptyParent = false;
			int withSubdependencyCount = 0;

			foreach (UpliftManager.DependencyState state in states)
			{
				if (state.transitive)
				{
					if (emptyParent)
					{
						withSubdependencyCount++;
						emptyParent = false;
					}
				}
				else
				{
					emptyParent = true;
				}
			}

			showSubdepedency = new bool[withSubdependencyCount];

			Repaint();
		}

		protected void OnGUI()
		{
#if UNITY_5_1_OR_NEWER
            titleContent.text = "Update Utility";
#endif
			EditorGUILayout.HelpBox("Please note that this window is not currently supported, and still experimental. Using it may cause unexpected issues. Use with care.", MessageType.Warning, true);

			float indent = 0;
			bool parentShown = false;
			bool emptyParent = false;
			int withSubDependencyCount = 0;

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			for (int i = 0; i < states.Length; i++)
			{
				UpliftManager.DependencyState state = states[i];

				if (state.transitive)
				{
					bool shown = parentShown;
					if (emptyParent)
					{
						emptyParent = false;
						withSubDependencyCount++;

						if (shown)
						{
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(indentSize);
							shown = showSubdepedency[withSubDependencyCount - 1];
							GUIContent tex = shown ? EditorGUIUtility.IconContent("IN foldout act on") : EditorGUIUtility.IconContent("IN foldout act");
							GUILayout.Label(tex, GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight));
							GUILayout.Label("Subdependency list");
							if (IsClicked())
							{
								shown = !shown;
								Event.current.Use();
							}

							GUILayout.FlexibleSpace();
							EditorGUILayout.EndHorizontal();
							showSubdepedency[withSubDependencyCount - 1] = shown;
							indent = 2 * indentSize;
						}
					}
					else if (shown)
					{
						shown = showSubdepedency[withSubDependencyCount - 1];
						GUILayout.Space(5);
					}

					if (!shown)
						continue;
				}
				else
				{
					emptyParent = true;

					indent = 0;
				}

				bool showState = showDependency[i];

				if (!state.transitive)
				{
					parentShown = showState;
				}

				DependencyStateBlock(
					indent,
					state.definition,
					state.bestMatch,
					state.latest,
					state.installed,
					ref showState
				);

				showDependency[i] = showState;
			}

			GUILayout.EndScrollView();
		}

		private void DependencyStateBlock(
			float indent,
			DependencyDefinition definition,
			PackageRepo bestMatch,
			PackageRepo latest,
			InstalledPackage installed,
			ref bool show
		)
		{
			EditorGUILayout.BeginHorizontal();

			if (indent != 0)
				GUILayout.Space(indent);

			GUIContent tex = show ? EditorGUIUtility.IconContent("IN foldout act on") : EditorGUIUtility.IconContent("IN foldout act");
			GUILayout.Label(tex, GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight));

			DrawDependencyTitle(definition.Name);

			GUILayout.Label("Requirement: " + definition.Requirement.ToString());

			VersionState state = VersionState.NOT_INSTALLED;

			if (installed != null)
			{
				if (VersionParser.GreaterThan(bestMatch.Package.PackageVersion, installed.Version))
				{
					state = VersionState.OUTDATED;
				}
				else
					state = VersionState.UP_TO_DATE;

				if (!definition.Requirement.IsMetBy(installed.Version))
					state = VersionState.INCOMPATIBLE;

				if (latest.Package.PackageVersion != bestMatch.Package.PackageVersion)
					state = VersionState.NEW_VERSION_INCOMPATIBLE;
			}

			switch (state)
			{
				case VersionState.NOT_INSTALLED:
					{
						DrawVersion("Installed version: " + installed.Version, Color.red);
						if (IsClicked())
						{
							show = !show;
							Event.current.Use();
						}
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();
						break;
					}
				case VersionState.OUTDATED:
					{
						DrawVersion("Installed version: " + installed.Version, Color.red);
						if (IsClicked())
						{
							show = !show;
							Event.current.Use();
						}
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						if (show)
						{
							EditorGUILayout.BeginHorizontal();
							if (indent != 0)
								GUILayout.Space(indent);

							EditorGUILayout.HelpBox(string.Format(
							"Package is outdated. You can update it to {0} (from {1})",
							bestMatch.Package.PackageVersion,
							bestMatch.Repository.ToString()), MessageType.Info);
							EditorGUILayout.BeginHorizontal();

							EditorGUILayout.BeginHorizontal();
							if (indent != 0)
								GUILayout.Space(indent);

							if (GUILayout.Button("Update to version " + bestMatch.Package.PackageVersion))
							{
								UpliftManager.Instance().UpdatePackage(bestMatch);
								Init();
								Repaint();
							}
							EditorGUILayout.EndHorizontal();
						}

						break;
					}
				case VersionState.INCOMPATIBLE:
					{
						MessageType messageType = installed.Version != bestMatch.Package.PackageVersion ? MessageType.Warning : MessageType.Error;
						DrawVersion("Installed version: " + installed.Version, messageType == MessageType.Warning ? Color.yellow : Color.red);
						if (IsClicked())
						{
							show = !show;
							Event.current.Use();
						}
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						if (show)
						{
							EditorGUILayout.BeginHorizontal();
							if (indent != 0)
								GUILayout.Space(indent);
							EditorGUILayout.HelpBox("The version of the package currently installed does not match the requirements of your project!",
							messageType
						);
							EditorGUILayout.EndHorizontal();
						}

						break;
					}
				case VersionState.NEW_VERSION_INCOMPATIBLE:
					{
						DrawVersion("Installed version: " + installed.Version, Color.yellow);
						if (IsClicked())
						{
							show = !show;
							Event.current.Use();
						}
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						if (show)
						{
							EditorGUILayout.BeginHorizontal();
							if (indent != 0)
								GUILayout.Space(indent);
							EditorGUILayout.HelpBox(
							string.Format(
								"Note: there is a more recent version of the package ({0} from {1}), but it doesn't match your requirement",
								latest.Package.PackageVersion,
								bestMatch.Repository.ToString()
							),
							MessageType.Info
						);
							EditorGUILayout.EndHorizontal();
						}
						break;
					}
				case VersionState.UP_TO_DATE:
					{
						DrawVersion("Installed version: " + installed.Version, Color.green);
						if (IsClicked())
						{
							show = !show;
							Event.current.Use();
						}
						GUILayout.FlexibleSpace();
						EditorGUILayout.EndHorizontal();

						if (show)
						{
							EditorGUILayout.BeginHorizontal();
							if (indent != 0)
								GUILayout.Space(indent); EditorGUILayout.HelpBox("Package is up to date!", MessageType.Info);
							EditorGUILayout.EndHorizontal();
						}

						break;
					}
			}
		}

		public void OnInspectorUpdate()
		{
			this.Repaint();
		}

		private void DrawDependencyTitle(string info)
		{
			GUIStyle boldStyle = EditorStyles.label;
			boldStyle.fontStyle = FontStyle.Bold;
			GUILayout.Label(info, boldStyle);
		}

		private void DrawVersion(string info, Color infoColor)
		{
			GUIStyle style = EditorStyles.label;
			Color previousColor = style.normal.textColor;
			style.normal.textColor = infoColor;
			GUILayout.Label(info, style);
			style.normal.textColor = previousColor;
		}

		private bool IsClicked()
		{
			if (Event.current.type == EventType.mouseDown)
			{
				Rect rect = GUILayoutUtility.GetLastRect();
				float y = Event.current.mousePosition.y;

				if (rect.yMin <= y && rect.yMax >= y)
				{
					return true;
				}
			}

			return false;
		}
	}
}
