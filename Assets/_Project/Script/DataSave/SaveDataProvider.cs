using MeowStudio.Data;
using UnityEngine;

namespace MeowStudio.DataSave
{
    public class SaveDataProvider : MonoBehaviour
    {
        private SaveDataManager saveDataManager;

        public void Initialize(SaveDataManager saveDataManager)
        {
            this.saveDataManager = saveDataManager;
        }

        public SaveData saveData
        {
            get
            {
                return saveDataManager.gameData;
            }
            set
            {
                saveDataManager.gameData = value;
            }
        }
    }
}