using MeowStudio.UI;
using UnityEngine;
using Zenject;

namespace MeowStudio.Bootstrap
{
    public class MenuContextInstaller: MonoInstaller
    {
        [SerializeField] private PopupService popupService;

        public override void InstallBindings()
        {
            Container.Bind<PopupService>().FromComponentInNewPrefab(popupService).AsSingle().NonLazy();
        }
    }
}
