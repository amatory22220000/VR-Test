using MeowStudio.Utils;
using UnityEngine;

namespace MeowStudio.Gameplay
{
    public class HintService: MonoBehaviour
    {
        [SerializeField] private GameObject hintPrefab;
        private Pool pool;

        public void ShowHint(string text, Vector2 position)
        {
            pool.GetObject(hintPrefab, position).GetComponent<HintView>().PlayEffect(text);
        }
    }
}
