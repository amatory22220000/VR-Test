using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MeowStudio.Utils
{
    public class UI_Utils
    {
        public static void RefreshLayoutGroupsImmediateAndRecursive(GameObject root)
        {
            var componentsInChildren = root.GetComponentsInChildren<LayoutGroup>(true);
            foreach (var layoutGroup in componentsInChildren)
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
            var parent = root.GetComponent<LayoutGroup>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());

            root.SetActive(false);
            root.SetActive(true);
        }
        public static bool IsHitUI(Vector2 screenCoord)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current){pointerId = -1,};
            pointerData.position = screenCoord;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            return results.Count > 0;
        }
        public static Vector2 RectToWorldPosition(RectTransform rect)
        {
            Vector3[] v = new Vector3[4];
            rect.GetWorldCorners(v);
            return new Vector2(v[2].x - v[0].x, v[1].y - v[0].y);
        }
    }
}
