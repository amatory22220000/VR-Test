using MeowStudio.Ads;
using MeowStudio.Analytics;
using MeowStudio.Audio;
using MeowStudio.Data;
using MeowStudio.DataSave;
using MeowStudio.SceneManagement;
using UnityEngine;
using Zenject;

namespace MeowStudio.Bootstrap
{
    public class ProjectContextInstaller : MonoInstaller
    {
        [SerializeField] private GameObject audioPlayer;
        [SerializeField] private GameObject sceneLoader;
        [SerializeField] private AdsData adsData;
        [SerializeField] private GameSettings settings;

        public override void InstallBindings()
        {
            Container.Bind<AppStatusService>().FromNewComponentOn(gameObject).AsSingle().NonLazy();

            Container.Bind<GameSettingsProvider>().FromNew().WithArguments(settings);
            Container.Bind<GameVariables>().FromNew();

            Container.Bind<SaveDataManager>().FromNewComponentOn(gameObject).AsSingle().NonLazy();
            Container.Bind<SaveDataProvider>().FromNewComponentOn(gameObject).AsSingle().NonLazy();

            Container.Bind<AudioPlayer>().FromComponentInNewPrefab(audioPlayer).AsSingle().NonLazy();
            Container.Bind<SceneLoader>().FromComponentInNewPrefab(sceneLoader).AsSingle().NonLazy();
        }



        private void BindAnalytics()
        {
            Container.Bind<FirebaseService>().FromNew().AsSingle().NonLazy();
        }
        private void BindAds()
        {
            Container.Bind<AdsService>().FromNew().AsSingle().WithArguments(adsData).NonLazy();
            Container.Bind<IAdsProvider>().To<AdmobAdsProvider>().AsSingle().NonLazy();
        }
    }
}
