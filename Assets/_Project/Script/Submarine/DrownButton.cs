using UnityEngine;

namespace MeowStudio.Submarine
{
    public class DrownButton : MonoBehaviour
    {
        [SerializeField] private SubmarineMove submarineMove;
        public void OnButtonPressed()
        {
            if (submarineMove == null) return;
            submarineMove.SetDepthControl(-1f);
        }
        public void OnButtonReleased()
        {
            if (submarineMove == null) return;
            submarineMove.SetDepthControl(0f);
        }
    }
}
