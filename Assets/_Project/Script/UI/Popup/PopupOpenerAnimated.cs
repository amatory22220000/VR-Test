using DG.Tweening;
using UnityEngine;

namespace MeowStudio.UI
{
    public class PopupOpenerAnimated : MonoBehaviour, IPopupOpener
    {
        private CanvasGroup blackGroup;
        private Transform popupTransform;

        private Sequence sequence;
        private float fadeTime = 0.2f;

        private void OnEnable()
        {
            blackGroup ??= transform.GetChild(0).GetComponent<CanvasGroup>();
            popupTransform ??= transform.GetChild(1);
            Open();
        }

        public virtual void Open()
        {
            ResetSequence();

            blackGroup.alpha = 0;
            popupTransform.localScale = Vector3.zero;

            sequence = DOTween.Sequence();
            sequence.AppendCallback(()=> popupTransform.DOScale(Vector3.one, fadeTime));
            sequence.Append(blackGroup.DOFade(1, fadeTime));
        }
        public virtual void Close()
        {
            ResetSequence();

            sequence = DOTween.Sequence();
            sequence.AppendCallback(() => popupTransform.DOScale(Vector3.zero, fadeTime));
            sequence.Append(blackGroup.DOFade(0, fadeTime + 0.01f));
            sequence.OnComplete(()=> Destroy(gameObject));
        }

        private void ResetSequence()
        {
            if (sequence.IsActive())
                sequence.Kill();
        }
    }
}
