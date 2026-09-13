using UnityEngine;

namespace MeowStudio.Submarine
{
    public class MoveForwardButton : MonoBehaviour
    {
        [SerializeField] private SubmarineMove submarineMove;
        public void OnButtonPressed()
        {
            if (submarineMove == null) return;
            Debug.Log("Move Forward Button Pressed");
            submarineMove.SetTrottle(1f);
        }
        public void OnButtonReleased()
        {
            if (submarineMove == null) return;
            Debug.Log("Move Forward Button Released");
            submarineMove.SetTrottle(0f);
        }
    }
}
