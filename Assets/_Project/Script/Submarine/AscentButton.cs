using UnityEngine;

namespace MeowStudio.Submarine
{
    public class AscentButton : MonoBehaviour
    {
        [SerializeField] private SubmarineMove submarineMove;
        public void OnButtonPressed()
        {
            if (submarineMove == null) return;
            submarineMove.SetDepthControl(1f);
        }
        public void OnButtonReleased()
        {
            if (submarineMove == null) return;
            submarineMove.SetDepthControl(0f);
        }
    }
}
