using MeowStudio.UI;
using UnityEngine;
using Zenject;

namespace MeowStudio.Bootstrap
{
    public class GameplayBootstrap: MonoBehaviour
    {
        [Inject] private PopupService popupService;

        private void Awake()
        {
            popupService.Initialize();
        }
    }
}
