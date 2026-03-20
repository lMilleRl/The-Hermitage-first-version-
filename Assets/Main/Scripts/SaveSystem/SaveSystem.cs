using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class SaveSystem
{
   
    private static string saveFilePath = Application.persistentDataPath + "/";

    private static JsonSerializerSettings settings = new JsonSerializerSettings
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    [System.Serializable]
    public class SavedData<T>
    {
        public T data;

        public SavedData(T data)
        {
            this.data = data;
        }
    }

    // Сохранение данных
    public static void SaveData<T>(T data, string fileName)
    {
        SavedData<T> saveData = new SavedData<T>(data);
        string json = JsonConvert.SerializeObject(saveData, settings);
        File.WriteAllText(saveFilePath + fileName + ".json", json);
    }

    // Загрузка данных
    public static SavedData<T> LoadData<T>(string fileName)
    {
        string fullName = saveFilePath + fileName + ".json";
        if (File.Exists(fullName))
        {
            string json = File.ReadAllText(fullName);
            SavedData<T> data = JsonConvert.DeserializeObject<SavedData<T>>(json, settings);

            return data;
        }
        else
        {
            // Если файла нет, возвращаем позицию по умолчанию
            return null;
        }
    }

    // Удаление файла сохранения
    public static void TryDeleteData(string fileName)
    {
        string fullName = saveFilePath + fileName + ".json";
        if (File.Exists(fullName))
        {
            File.Delete(fullName);
            Debug.Log("Сохраненный файл удален.");
        }
        else
        {
            Debug.Log("Сохраненный файл не найден.");
        }
    }    
}

