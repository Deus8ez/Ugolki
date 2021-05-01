using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GameRules;

public class SaveAndLoad : MonoBehaviour
{
    public static void SaveGame(BoardState state)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.fun";
        FileStream stream = new FileStream(path, FileMode.Create);

        GameData data = new GameData(state);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static GameData LoadGame()
    {
        string path = Application.persistentDataPath + "/player.fun";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            GameData data = formatter.Deserialize(stream) as GameData;

            stream.Close();

            return data;
        } else
        {
            Debug.LogError("save file not found" + path);
            return null;
        }
    }
}
