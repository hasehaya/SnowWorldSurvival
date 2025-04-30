using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;


public static class SaveSystem
{
    /// <summary>
    /// Saves data to a file in binary format.
    /// The data is serialized and stored in the application's persistent data path.
    /// </summary>
    /// <typeparam name="T">The type of the data to be saved.</typeparam>
    /// <param name="data">The data to be saved.</param>
    /// <param name="fileName">The name of the file to save the data in.</param>
    public static void SaveData<T>(T data, string fileName)
    {
        string filePath = Application.persistentDataPath + "/" + fileName + ".dat";
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream fileStream = new FileStream(filePath, FileMode.Create);
        formatter.Serialize(fileStream, data);
        fileStream.Close();
    }

    /// <summary>
    /// Loads data from a file in binary format.
    /// If the file doesn't exist, the default value of the data type is returned.
    /// </summary>
    /// <typeparam name="T">The type of the data to be loaded.</typeparam>
    /// <param name="fileName">The name of the file to load the data from.</param>
    /// <returns>The loaded data, or the default value if the file doesn't exist.</returns>
    public static T LoadData<T>(string fileName)
    {
        string filePath = Application.persistentDataPath + "/" + fileName + ".dat";
        if (File.Exists(filePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            T data = (T)formatter.Deserialize(fileStream);
            fileStream.Close();
            return data;
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Deletes the save data file with the given file name.
    /// </summary>
    /// <param name="fileName">The name of the file whose data should be deleted.</param>
    public static void DeleteData(string fileName)
    {
        string filePath = Application.persistentDataPath + "/" + fileName + ".dat";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Deleted save file: " + filePath);
        }
    }
}
