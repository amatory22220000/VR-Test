using UnityEngine;
namespace MeowStudio.Gameplay
{
    using DG.Tweening;
    using MeowStudio.Utils;

    using TMPro;
    public class HintView: MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private CanvasGroup group;

        private float upDistance = 2f;
        private float speed = 2f;

        private Sequence sequence;

        public void PlayEffect(string text)
        {
            Reset();

            this.text.text = text;
            Vector3 targetPoint = transform.position + Vector3.up * upDistance;
            float time = Vector3.Distance(transform.position, targetPoint) / speed;

            sequence = DOTween.Sequence();
            sequence.AppendCallback(()=> transform.DOMove(targetPoint, time));
            sequence.AppendInterval(time / 2);
            sequence.Append(group.DOFade(0, time / 2));
            sequence.OnComplete(()=> GetComponent<PoolObject>().ReturnToPool());
        }
        private void Reset()
        {
            group.alpha = 1f;
            if (sequence.IsActive())
                sequence.Kill();
        }
    }
}
