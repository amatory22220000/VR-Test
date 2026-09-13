using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MeowStudio.Utils
{
    public class EditorAssetsUtils
    {
        public static List<T> LoadAssetsOfType<T>(string path) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            return AssetDatabase
                .FindAssets($"t:{typeof(T).Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .ToList();
#else
            return null;
#endif
        }
        public static void SaveScriptable(ScriptableObject asset)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
    }
}
