using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeowStudio.Utils.Editor
{
    /// <summary>
    /// Quick toggle between a "Realtime" light preview (instant feedback while placing/tuning
    /// lights in the Scene view, no baking needed) and the project's actual "Baked/Mixed"
    /// lighting setup (ready for Window > Rendering > Lighting > Generate Lighting).
    ///
    /// "Set Realtime" remembers each light's current Lightmapping mode for the rest of the
    /// Editor session and forces every light to Realtime, then clears the baked lightmap data
    /// so it doesn't double up with the realtime preview. "Set Baked" restores every light to
    /// the mode it had before switching, ready to bake again.
    /// </summary>
    public static class LightBakeModeToggle
    {
        private const string SessionKeyPrefix = "LightBakeModeToggle_";
        private const string CachedKeysListKey = SessionKeyPrefix + "CachedKeys";
        private const int NoCachedValue = -1; // SessionState has no HasKey API, so this sentinel marks "not stored yet".

        [MenuItem("Tools/Lighting/Set Realtime", priority = 100)]
        public static void SetRealtime()
        {
            List<Light> lights = FindAllLights();
            if (lights.Count == 0)
            {
                Debug.LogWarning("[LightBakeModeToggle] No Light components found in the open scene(s).");
                return;
            }

            var cachedKeys = new List<string>();
            int switched = 0;

            foreach (Light light in lights)
            {
                string key = GetLightKey(light);
                cachedKeys.Add(key);

                // Only remember the very first mode we see for this light in this session,
                // so pressing "Set Realtime" twice in a row can't overwrite the real original.
                if (SessionState.GetInt(key, NoCachedValue) == NoCachedValue)
                {
                    SessionState.SetInt(key, (int)light.lightmapBakeType);
                }

                if (light.lightmapBakeType != LightmapBakeType.Realtime)
                {
                    Undo.RecordObject(light, "Set Realtime Lighting");
                    light.lightmapBakeType = LightmapBakeType.Realtime;
                    EditorUtility.SetDirty(light);
                    switched++;
                }
            }

            SessionState.SetString(CachedKeysListKey, string.Join("\n", cachedKeys));

            // Drop the current lightmaps so the old bake doesn't mix with the realtime preview.
            Lightmapping.Clear();

            MarkOpenScenesDirty();

            Debug.Log($"[LightBakeModeToggle] Realtime preview ON — {switched} light(s) switched to Realtime, lightmaps cleared.");
        }

        [MenuItem("Tools/Lighting/Set Baked", priority = 101)]
        public static void SetBaked()
        {
            string cachedKeysRaw = SessionState.GetString(CachedKeysListKey, string.Empty);
            if (string.IsNullOrEmpty(cachedKeysRaw))
            {
                Debug.LogWarning("[LightBakeModeToggle] Nothing to restore — run \"Tools/Lighting/Set Realtime\" first in this Editor session.");
                return;
            }

            Dictionary<string, Light> lightByKey = new Dictionary<string, Light>();
            foreach (Light light in FindAllLights())
            {
                lightByKey[GetLightKey(light)] = light;
            }

            int restored = 0;
            foreach (string key in cachedKeysRaw.Split('\n'))
            {
                int cachedValue = string.IsNullOrEmpty(key) ? NoCachedValue : SessionState.GetInt(key, NoCachedValue);
                if (cachedValue == NoCachedValue)
                {
                    continue;
                }

                var originalMode = (LightmapBakeType)cachedValue;
                if (lightByKey.TryGetValue(key, out Light light))
                {
                    Undo.RecordObject(light, "Set Baked Lighting");
                    light.lightmapBakeType = originalMode;
                    EditorUtility.SetDirty(light);
                    restored++;
                }

                SessionState.EraseInt(key);
            }

            SessionState.EraseString(CachedKeysListKey);

            MarkOpenScenesDirty();

            Debug.Log($"[LightBakeModeToggle] Baked setup restored — {restored} light(s) reverted to their original mode. Run Generate Lighting to (re)bake.");
        }

        private static List<Light> FindAllLights()
        {
            var result = new List<Light>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                {
                    continue;
                }

                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    result.AddRange(root.GetComponentsInChildren<Light>(true));
                }
            }

            return result;
        }

        private static void MarkOpenScenesDirty()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }
        }

        // Identifies a light by its scene path + hierarchy path so the cached mode survives
        // reordering/renaming of other objects. Not persisted to disk on purpose: a fresh
        // Editor session starts with a clean slate instead of restoring stale data.
        private static string GetLightKey(Light light)
        {
            var sb = new StringBuilder();
            sb.Append(light.gameObject.scene.path);
            sb.Append('|');
            sb.Append(GetHierarchyPath(light.transform));
            return SessionKeyPrefix + sb.ToString().GetHashCode();
        }

        private static string GetHierarchyPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }

            return path;
        }
    }
}
