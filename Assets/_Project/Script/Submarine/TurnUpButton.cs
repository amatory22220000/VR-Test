using UnityEngine;

namespace MeowStudio.Submarine
{
    public class TurnUpButton : MonoBehaviour
    {
        [SerializeField] private SubmarineMove submarineMove;
        public void OnButtonPressed()
        {
            if (submarineMove == null) return;
            submarineMove.SetDepthSteering(1f);
        }
        public void OnButtonReleased()
        {
            if (submarineMove == null) return;
            submarineMove.SetDepthSteering(0f);
        }
    }
}
