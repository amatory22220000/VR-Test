using UnityEngine;

namespace MeowStudio.Submarine
{
    public class TurnLeftButton : MonoBehaviour
    {
        [SerializeField] private SubmarineMove submarineMove;
        public void OnButtonPressed()
        {
            if (submarineMove == null) return;
            submarineMove.SetSteering(-60f);
        }
        public void OnButtonReleased()
        {
            if (submarineMove == null) return;
            submarineMove.SetSteering(0f);
        }
    }
}
