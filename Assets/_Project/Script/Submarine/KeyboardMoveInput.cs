using UnityEngine;
using UnityEngine.InputSystem;

namespace MeowStudio.Submarine
{
    public class KeyboardMoveInput : MonoBehaviour
    {
        [SerializeField] private SubmarineMove submarineMove;

        private void Update()
        {
            if (submarineMove == null) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                submarineMove.SetTrottle(0f);
                submarineMove.SetSteering(0f);
                submarineMove.SetDepthControl(0f);
                return;
            }

            float trottle = 0f;
            float steering = 0f;
            float depthControl = 0f;
            float deptSteering = 0f;

            if (keyboard.wKey.isPressed) trottle = 1f;
            if (keyboard.sKey.isPressed) trottle = -1f;
            if (keyboard.aKey.isPressed) steering = -90f;
            if (keyboard.dKey.isPressed) steering = 90f;
            if (keyboard.qKey.isPressed) depthControl = 1f;
            if (keyboard.eKey.isPressed) depthControl = -1f;
            if (keyboard.upArrowKey.isPressed) deptSteering = -1f;
            if (keyboard.downArrowKey.isPressed) deptSteering = 1f;

            submarineMove.SetTrottle(trottle);
            submarineMove.SetSteering(steering);
            submarineMove.SetDepthControl(depthControl);
            submarineMove.SetDepthSteering(deptSteering);
        }
    }
}
