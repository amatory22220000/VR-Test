using System;

namespace MeowStudio.Data
{
    [Serializable]
    public class SaveData
    {
        public int money;
        public float soundVolume;
        public float musicVolume;

        public int lives;
        public long startLifeRestoreDate;
        public long finishInfLifesDate;

        public SaveData()
        {
            money = 0;
            soundVolume = 1;
            musicVolume = 1;

            lives = 5;

            startLifeRestoreDate = 0;
            finishInfLifesDate = 0;
        }
    }
}
