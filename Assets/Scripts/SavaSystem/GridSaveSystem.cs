using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary; 

public static class GridSaveSystem 
{
    // Save Data by PlayerData in Menu Scene
    public static void SaveData( PlayerData playerData)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/User.grid";
        FileStream stream = new FileStream(path, FileMode.Create);

        SavedData data = new SavedData(playerData);
        formatter.Serialize(stream, data);
        Debug.Log(path);
        stream.Close(); 
    }

    // Save Data by UserData in Game Scene 
    public static void SaveData(UserData userData)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/User.grid";
        FileStream stream = new FileStream(path, FileMode.Create);

        SavedData data = new SavedData(userData);
        formatter.Serialize(stream, data);
        Debug.Log(path);
        stream.Close();
    }

    public static SavedData LoadData()
    {
        string path = Application.persistentDataPath + "/User.grid";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            SavedData data = formatter.Deserialize(stream) as SavedData;
            stream.Close();
            return data; 
        }
        else
        {
            Debug.Log("File not found in " + path); 
        }
        Debug.Log(path);
        return null;
    }
}
