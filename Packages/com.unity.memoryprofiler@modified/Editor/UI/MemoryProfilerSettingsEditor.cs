using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.MemoryProfilerExtension.Editor.UIContentData;

#if UNITY_2021_2_OR_NEWER
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Unity.MemoryProfilerExtension.Editor.MemoryProfilerModule")]
#endif

namespace Unity.MemoryProfilerExtension.Editor
{
    public class MemoryProfilerSettingsEditor
    {
        public const string SettingsPath = "Preferences/Analysis/Memory Profiler";
        class Content
        {
            public static readonly GUIContent SnapshotPathLabel = EditorGUIUtility.TrTextContent("Memory Snapshot Storage Path");
            public static readonly string OnlyRelativePaths = L10n.Tr("Only relative paths are allowed");
            public static readonly string OKButton = L10n.Tr("OK");
            public static readonly string InvalidPathWindow = L10n.Tr("Invalid Path");
            public static readonly string MemoryProfilerPackageOverridesMemoryModuleUI = L10n.Tr("Replace Memory UI in Profiler Window", "If set to true, the Memory Profiler Module UI in the Profiler Window will be replaced with UI from the Memory Profiler package.");
            public static readonly string ShowReservedMemoryBreakdown = L10n.Tr("Show reserved memory breakdown", "If set to true, the Memory Profiler will show reserved memory breakdown to individual allocator in `All Of Memory` view.");
            public static readonly string ShowAllSystemMemoryView = L10n.Tr("Show Memory Map view", "If set to true, the Memory Profiler will show additional `Memory Map` view with low-level OS system memory map with breakdown.");
            public static readonly string AssetSearchSettings = L10n.Tr("Asset Search Provider Settings", "The Memory Profiler uses search features to search for assets listed in the capture within the currently open project. When it finds them, it allows selecting the asset and, if one exists, shows a preview of the asset in the Memory Profiler UI. There have been isolated reports of issues with opening the Memory Profiler based on the indexing of the used search providers. If you are experiencing problems with opening the Memory Profiler Window or extreme delays when selecting assets in it, you could try changing this setting. This setting also affects which providers will be used by the Open in Search button.");

            public static readonly GUIContent TitleSettingsIcon = EditorGUIUtility.TrIconContent("_Popup", "Settings");
            public static readonly GUIContent HelpIcon = EditorGUIUtility.TrIconContent("_Help", "Open Documentation");
            public static readonly GUIContent ResetSettings = EditorGUIUtility.TrTextContent("Revert to default settings");

            public static readonly GUIContent ResetOptOutDialogsButton = EditorGUIUtility.TrTextContent("Reset Opt-Out settings for dialog prompts", "All dialogs that you have previously opted out of will show up again when they get triggered.");
        }
        static class Style
        {
            public static readonly GUIStyle IconButton = new GUIStyle("IconButton");
        }
        const string k_RootPathSignifier = "./";
        const string k_PathOneUpSignifier = "../";

        [SettingsProvider()]
        static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider(SettingsPath, SettingsScope.User)
            {
                guiHandler = searchConext =>
                {
                    PreferencesGUI();
                },

                titleBarGuiHandler = TitleBarGUI,
            };
            provider.PopulateSearchKeywordsFromGUIContentProperties<Content>();
            return provider;
        }

        static void TitleBarGUI()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Content.HelpIcon, Style.IconButton))
                Application.OpenURL(MemoryProfilerExtension.Editor.UIContentData.DocumentationUrls.LatestPackageVersionUrl);
            GUILayout.Space(2);
            if (GUILayout.Button(Content.TitleSettingsIcon, Style.IconButton))
            {
                var menu = new GenericMenu();
                menu.AddItem(Content.ResetSettings, false, () =>
                {
                    MemoryProfilerSettings.ResetMemorySnapshotStoragePathToDefault();
                });
                var position = new Rect(Event.current.mousePosition, Vector2.zero);
                menu.DropDown(position);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        static void PreferencesGUI()
        {
            float layoutMaxWidth = 500;
            float s_DefaultLabelWidth = 250;
            float m_LabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = s_DefaultLabelWidth;
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(layoutMaxWidth));
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            {
                EditorGUI.BeginChangeCheck();
                var prevControl = GUI.GetNameOfFocusedControl();
                var val = EditorGUILayout.DelayedTextField(Content.SnapshotPathLabel, MemoryProfilerSettings.MemorySnapshotStoragePath);

                if (EditorGUI.EndChangeCheck())
                {
                    if (!(val.StartsWith(k_RootPathSignifier) || val.StartsWith(k_PathOneUpSignifier)))
                    {
                        if (EditorUtilityCompatibilityHelper.DisplayDecisionDialog(Content.InvalidPathWindow, Content.OnlyRelativePaths, Content.OKButton))
                        {
                            GUI.FocusControl(prevControl);
                            var currentlySavedPath = MemoryProfilerSettings.MemorySnapshotStoragePath;
                            // in case this faulty path has actually been saved, fix it back to default
                            if (!(currentlySavedPath.StartsWith(k_RootPathSignifier) || currentlySavedPath.StartsWith(k_PathOneUpSignifier)))
                                MemoryProfilerSettings.ResetMemorySnapshotStoragePathToDefault();
                        }
                    }
                    else
                    {
                        MemoryProfilerSettings.MemorySnapshotStoragePath = val;
                        var collectionPath = MemoryProfilerSettings.AbsoluteMemorySnapshotStoragePath;
                        var info = new DirectoryInfo(collectionPath);
                        if (!info.Exists)
                        {
                            info = Directory.CreateDirectory(collectionPath);
                            if (!info.Exists)
                                throw new UnityException("Failed to create directory, with provided preferences path: " + collectionPath);
                        }
                    }
                }
#if UNITY_2021_2_OR_NEWER
                MemoryProfilerSettings.MemoryProfilerPackageOverridesMemoryModuleUI = EditorGUILayout.Toggle(Content.MemoryProfilerPackageOverridesMemoryModuleUI, MemoryProfilerSettings.MemoryProfilerPackageOverridesMemoryModuleUI);
#endif
                if (GUILayout.Button(Content.ResetOptOutDialogsButton))
                {
                    MemoryProfilerSettings.ResetAllOptOutModalDialogSettings();
                }
                var newTruncateValue = EditorGUILayout.Toggle(new GUIContent(TextContent.TruncateTypeName), MemoryProfilerSettings.MemorySnapshotTruncateTypes);
                if (newTruncateValue != MemoryProfilerSettings.MemorySnapshotTruncateTypes)
                {
                    MemoryProfilerSettings.ToggleTruncateTypes();
                }
                MemoryProfilerSettings.ShowReservedMemoryBreakdown = EditorGUILayout.Toggle(Content.ShowReservedMemoryBreakdown, MemoryProfilerSettings.ShowReservedMemoryBreakdown);
                MemoryProfilerSettings.ShowMemoryMapView = EditorGUILayout.Toggle(Content.ShowAllSystemMemoryView, MemoryProfilerSettings.ShowMemoryMapView);

                if (MemoryProfilerSettings.InternalMode)
                    MemoryProfilerSettings.AssetSearchSetting = (QuickSearchUtility.AssetSearchSetting)EditorGUILayout.EnumPopup(Content.AssetSearchSettings, MemoryProfilerSettings.AssetSearchSetting);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = m_LabelWidth;
        }
    }
}
