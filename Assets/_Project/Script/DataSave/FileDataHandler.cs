using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json;
using MeowStudio.Data;

namespace MeowStudio.DataSave
{
    public class FileDataHandler : IDataHandler<SaveData>
    {
        private string dataDirPath = "";
        private string dataFileName = "";
        private bool useEncryption = false;

        public void Initialize(string path, string fileName, bool useEncryption)
        {
            dataDirPath = path;
            dataFileName = fileName;
            this.useEncryption = useEncryption;
        }

        public SaveData Load()
        {
            string fullPath = Path.Combine(dataDirPath, dataFileName);
            SaveData loadedData = new SaveData();
            if(File.Exists(fullPath))
            {
                try
                {
                    string dataToLoad = "";
                    using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToLoad = reader.ReadToEnd();
                        }
                    }
                    if(useEncryption)
                    {
                        dataToLoad = EncryptDecrypt(dataToLoad);
                    }
                    loadedData = JsonConvert.DeserializeObject<SaveData>(dataToLoad);

                    Debug.Log($"Load data success from\n{fullPath}");
                }
                catch (Exception exeption)
                {
                    Debug.LogError($"Error occured when trying to load data from file:\n{fullPath}\n{exeption}");
                }
            }
            return loadedData;
        }
        public void Save(SaveData data)
        {
            string fullPath = Path.Combine(dataDirPath, dataFileName);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                string dataToSave = JsonConvert.SerializeObject(data);
                if(useEncryption)
                {
                    dataToSave = EncryptDecrypt(dataToSave);
                }
                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(dataToSave);
                    }
                }
                Debug.Log($"Save data success to\n{fullPath}");
            }
            catch (Exception exeption)
            {
                Debug.LogError($"Error occured when trying to save data to file:\n{fullPath}\n{exeption}");
            }
        }

        private string EncryptDecrypt(string data)
        {
            string modifiedData = "";

            for (int i = 0; i < data.Length; i++)
            {
                modifiedData += (char)data[i] ^ DataSaveConstants.encryptionCodeWord[i % DataSaveConstants.encryptionCodeWord.Length];
            }
            return modifiedData;
        }
    }
}
