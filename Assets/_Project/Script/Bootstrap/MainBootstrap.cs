using MeowStudio.Ads;
using MeowStudio.Audio;
using MeowStudio.DataSave;
using MeowStudio.SceneManagement;
using UnityEngine;
using Zenject;

namespace MeowStudio.Bootstrap
{
    public class MainBootstrap: MonoBehaviour
    {
        [Inject] private SaveDataManager dataSaveManager;
        [Inject] private SaveDataProvider saveDataProvider;
        [Inject] private AudioPlayer audioPlayer;
        [Inject] private SceneLoader sceneLoader;

        private void Awake()
        {
            dataSaveManager.Initialize();
            dataSaveManager.LoadGame();
            saveDataProvider.Initialize(dataSaveManager);
            audioPlayer.Initialize(saveDataProvider);
            sceneLoader.LoadScene(SceneNames.Menu);
        }
    }
}
