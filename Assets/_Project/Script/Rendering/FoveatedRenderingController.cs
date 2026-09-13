using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace MeowStudio.Rendering
{
    public class FoveatedRenderingController: MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float foveationLevel = 1f;

        private XRDisplaySubsystem display;

        private void OnEnable()
        {
            StartCoroutine(ApplyWhenDisplayReady());
        }

        private IEnumerator ApplyWhenDisplayReady()
        {
            var displays = new List<XRDisplaySubsystem>();
            while (display == null)
            {
                SubsystemManager.GetSubsystems(displays);
                foreach (var candidate in displays)
                {
                    if (candidate.running)
                    {
                        display = candidate;
                        break;
                    }
                }
                if (display == null)
                    yield return null;
            }
            Apply();
        }

        public void SetFoveationLevel(float level)
        {
            foveationLevel = Mathf.Clamp01(level);
            Apply();
        }

        private void Apply()
        {
            if (display == null || !display.running)
                return;
            display.foveatedRenderingLevel = foveationLevel;
        }
    }
}
