using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MeowStudio.Utils
{
    public class PoolObject: MonoBehaviour
    {
        [SerializeField] private float autoDisableTime;
        [SerializeField] private bool randomizeSize;
        [SerializeField, ShowIf("randomizeSize")] private Vector2 sizeDisp;
        [SerializeField] private bool randomizeRotation;
        [SerializeField] private bool disappearFade;
        [SerializeField, ShowIf("disappearFade")] private SpriteRenderer rend;
        [SerializeField, ShowIf("disappearFade")] private float disappearTime = 0.5f;

        private Pool pool;
        private Color startColor;

        public void Initialize(Pool pool)=> this.pool = pool;

        private void OnEnable()
        {
            if (disappearFade)
                startColor = rend.color;
            if (randomizeSize)
                transform.localScale = Vector3.one * Random.Range(sizeDisp.x, sizeDisp.y);
            if (randomizeRotation)
                transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

            if (autoDisableTime > 0)
                StartCoroutine(LifetimeCor());
        }
        private IEnumerator LifetimeCor()
        {
            yield return new WaitForSeconds(autoDisableTime);
            if(disappearFade)
            {
                yield return rend.DOFade(0, disappearTime).WaitForCompletion();
                rend.color = startColor;
            }
            ReturnToPool();
        }
        public void ReturnToPool()
        {
            pool.ReturnToPool(this);
        }
    }
}
