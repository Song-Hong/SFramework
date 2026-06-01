using System.IO;
using SFramework.SAI.Module.Data;
using UnityEditor;
using UnityEngine;

namespace SFramework.SAI.Editor.Support
{
  public static class SfAiEditorSettingsUtility
  {
    public static SfAiSettings GetOrCreateSettings()
    {
      var settings = AssetDatabase.LoadAssetAtPath<SfAiSettings>(SfAiEditorPaths.SettingsAssetPath);
      if (settings != null)
      {
        settings.EnsureDefaults();
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        return settings;
      }

      if (!Directory.Exists(SfAiEditorPaths.SettingsFolder))
        Directory.CreateDirectory(SfAiEditorPaths.SettingsFolder);

      settings = ScriptableObject.CreateInstance<SfAiSettings>();
      settings.EnsureDefaults();
      AssetDatabase.CreateAsset(settings, SfAiEditorPaths.SettingsAssetPath);
      AssetDatabase.SaveAssets();
      return settings;
    }
  }
}
