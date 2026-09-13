using UnityEngine;

namespace MeowStudio.Submarine
{
    public class MoveBackwardButton : MonoBehaviour
    {
        [SerializeField] private SubmarineMove submarineMove;
        public void OnButtonPressed()
        {
            if (submarineMove == null) return;
            submarineMove.SetTrottle(-1f);
        }
        public void OnButtonReleased()
        {
            if (submarineMove == null) return;
            submarineMove.SetTrottle(0f);
        }
    }
}
