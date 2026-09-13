using UnityEngine;
using MeowStudio.Data;
using Zenject;
using MeowStudio.SceneManagement;

namespace MeowStudio.DataSave
{
    public class SaveDataManager: MonoBehaviour
    {
        [Inject] private AppStatusService appStatusService;

        public SaveData gameData;
        private FileDataHandler dataHandler;

        public void Initialize()
        {
            dataHandler = new FileDataHandler();
            dataHandler.Initialize(Application.persistentDataPath,
                DataSaveConstants.dataFileName,
                DataSaveConstants.useEncryption);
            Debug.Log("DataSaveManager created dataHandler");

            appStatusService.OnFocus += LoadGame;
            appStatusService.OnUnfocus += SaveGame;
            appStatusService.OnQuit += SaveGame;
        }
        private void OnDisable()
        {
            appStatusService.OnFocus -= LoadGame;
            appStatusService.OnUnfocus -= SaveGame;
            appStatusService.OnQuit += SaveGame;
        }

        public void NewGame()
        {
            this.gameData = new SaveData();
            Debug.Log("New save game data created");

        }
        public void LoadGame()
        {
            gameData = dataHandler.Load();
            if (gameData == null)
            {
                Debug.Log("No save data was found");
                NewGame();
            }
            Debug.Log("Game data loaded");
        }
        public void SaveGame()
        {
            dataHandler.Save(gameData);
            Debug.Log("Game data saved");
        }
    }
}