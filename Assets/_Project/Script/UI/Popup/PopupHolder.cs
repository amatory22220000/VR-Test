using Sirenix.OdinInspector;
using UnityEngine;

namespace MeowStudio.UI
{
    [CreateAssetMenu(menuName = "DATA/Popup/PopupHolder")]
    public class PopupHolder: SerializedScriptableObject
    {
        [field: SerializeField] public Popup[] popupList { get; private set; }
    }
}
