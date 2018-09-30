using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad
{

    public static List<GameIstance> savedGames = new List<GameIstance>();

    public static void Save(GameManager gameManager)
    {
        SaveLoad.savedGames.Add(new GameIstance(gameManager));
        BinaryFormatter bf = new BinaryFormatter();
        //File.Delete(Path.Combine(UnityEngine.Application.persistentDataPath, "savedGames.gd"));
        FileStream file = File.Create(Path.Combine(UnityEngine.Application.persistentDataPath, "savedGames.gd")); //you can call it anything you want
        bf.Serialize(file, SaveLoad.savedGames);
        file.Close();
    }

    public static List<GameIstance> GetGameIstances()
    {
        if (File.Exists(Path.Combine(UnityEngine.Application.persistentDataPath, "savedGames.gd")))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Path.Combine(UnityEngine.Application.persistentDataPath, "savedGames.gd"), FileMode.Open);
            SaveLoad.savedGames = (List<GameIstance>)bf.Deserialize(file);
            file.Close();
        }
        return SaveLoad.savedGames;
    }

    public static void LoadIstance(GameManager gameManager, GameIstance gameIstance)
    {
        gameIstance.Restore(gameManager);
    }
}