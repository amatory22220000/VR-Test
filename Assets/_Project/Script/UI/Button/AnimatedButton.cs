using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MeowStudio.UI
{
    [RequireComponent(typeof(Button))]
    public class AnimatedButton : MonoBehaviour
    {
        private Sequence sequence;
        private float animTime = 0.2f;

        private void Start()
        {
            Button button = GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(PlayAnimation);
        }
        private void OnDisable()
        {
            if (sequence.IsActive())
                sequence.Kill();
        }

        private void PlayAnimation()
        {
            if (sequence.IsActive())
                sequence.Kill();

            sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(0.9f, animTime / 2));
            sequence.Append(transform.DOScale(1.0f, animTime / 2));
        }
    }
}