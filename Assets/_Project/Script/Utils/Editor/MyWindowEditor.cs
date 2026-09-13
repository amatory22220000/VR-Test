using UnityEngine;
using UnityEditor;
using System.IO;
using MeowStudio.DataSave;

public class MyWindowEditor
{
    [MenuItem("My buttons/Clear all save", priority = 0)]
    static void ClearAllSave()
    {
        PlayerPrefs.DeleteAll();
        string path = Path.Combine(Application.persistentDataPath, DataSaveConstants.dataFileName);
        if (File.Exists(path)) File.Delete(path);
        Debug.Log("Save data cleared");
    }
}