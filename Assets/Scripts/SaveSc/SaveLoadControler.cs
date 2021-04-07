using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoadControler
{
    public static string GetSaveExtension => "bytes";

    //saves
    public static string GetSavesPath()
    {
        string path = Application.persistentDataPath + "/Saves";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path + "/";
    }
    public static string[] GetAllSaves()
    {
        string path = GetSavesPath();
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return Directory.GetDirectories(path);
    }
    public static string GetNameFromPath(string path)
    {
        string name = path.Replace(GetSavesPath(), "");
        name = name.Replace("." + GetSaveExtension, "");
        return name;
    }

    //main
    public static string GetMainFolderPath()
    {
        string path = Application.persistentDataPath + "/MainGameData";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path + "/";
    }

    //main game data
    public static void SaveTechnolgiesData(SpaceBaseMainSc.TechnologiesData technologiesData)
    {
        string path = GetMainFolderPath() + "TechnologiesData" + "." + GetSaveExtension;
        BinaryFormatter formater = new BinaryFormatter();
        if (technologiesData == null) { Debug.Log("ERROR! Cant save technologies data!"); return; }
        FileStream stream = new FileStream(path, FileMode.Create);
        formater.Serialize(stream, technologiesData);
        stream.Close();
    }
    public static SpaceBaseMainSc.TechnologiesData ReadTechnologiesData()
    {
        string path = GetMainFolderPath() + "TechnologiesData" + "." + GetSaveExtension;
        if (!File.Exists(path))
        { Debug.Log("File not found in: " + path); return null; }

        SpaceBaseMainSc.TechnologiesData data = null;
        BinaryFormatter formater = new BinaryFormatter();
        FileStream stream = new FileStream(path , FileMode.Open);
        try { data = formater.Deserialize(stream) as SpaceBaseMainSc.TechnologiesData; } catch { Debug.Log("ERROR! TechnologiesData in save has the wrong format"); }
        stream.Close();
        return data;
    }
    public static void SaveItemsData(SpaceBaseMainSc.ItemsData itemsData)
    {
        string path = GetMainFolderPath() + "ItemsData" + "." + GetSaveExtension;
        BinaryFormatter formater = new BinaryFormatter();
        if (itemsData == null) { Debug.Log("ERROR! Cant save technologies data!"); return; }
        FileStream stream = new FileStream(path, FileMode.Create);
        formater.Serialize(stream, itemsData);
        stream.Close();
    }
    public static SpaceBaseMainSc.ItemsData ReadItemsData()
    {
        string path = GetMainFolderPath() + "ItemsData" + "." + GetSaveExtension;
        if (!File.Exists(path))
        { Debug.Log("File not found in: " + path); return null; }

        SpaceBaseMainSc.ItemsData data = null;
        BinaryFormatter formater = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);
        try { data = formater.Deserialize(stream) as SpaceBaseMainSc.ItemsData; } catch { Debug.Log("ERROR! ItemsData in save has the wrong format"); }
        stream.Close();
        return data;
    }
    public static void SaveGeneralGameData(SpaceBaseMainSc.GeneralGameData generalData)
    {
        string path = GetMainFolderPath() + "GeneralGameData" + "." + GetSaveExtension;
        BinaryFormatter formater = new BinaryFormatter();
        if (generalData == null) { Debug.Log("ERROR! Cant save technologies data!"); return; }
        FileStream stream = new FileStream(path, FileMode.Create);
        formater.Serialize(stream, generalData);
        stream.Close();
    }
    public static SpaceBaseMainSc.GeneralGameData ReadGeneralGameData()
    {
        string path = GetMainFolderPath() + "GeneralGameData" + "." + GetSaveExtension;
        if (!File.Exists(path))
        { Debug.Log("File not found in: " + path); return null; }

        SpaceBaseMainSc.GeneralGameData data = null;
        BinaryFormatter formater = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);
        try { data = formater.Deserialize(stream) as SpaceBaseMainSc.GeneralGameData; } catch { Debug.Log("ERROR! GeneralGameData in save has the wrong format"); }
        stream.Close();
        return data;
    }
    public static void DeleteMainGameData()
    {
        string path = GetMainFolderPath();
        Debug.Log("Removing main game data");
        Directory.Delete(path, true);
    }

    //colony data
    public static string GetColoniesPath() { return GetMainFolderPath() + "Colonies/"; }
    public static GeneralWorldData GetGeneralWorldDataFromResources(string colonyName)
    {
        string path = "AvailbleMaps/" + colonyName + "/General";

        TextAsset textAsset = Resources.Load(path) as TextAsset;
        if (textAsset == null) { Debug.Log("Dont't found colony " + colonyName + " at path " + path); return null; }
        GeneralWorldData GeneralD = null;
        Stream stream = new MemoryStream(textAsset.bytes);
        BinaryFormatter formater = new BinaryFormatter();
        try { GeneralD = formater.Deserialize(stream) as GeneralWorldData; }
        catch { Debug.Log("ERROR! GeneralWorldData from " + colonyName + " has the wrong format"); }
        stream.Close();
        
        return GeneralD;
    }
    public static WorldData GetWorldDataFromResources(string colonyName)
    {
        string path;
        BinaryFormatter formater = new BinaryFormatter();
        WorldData data;
        TextAsset textAsset;

        //General
        GeneralWorldData GeneralD = null;
        path = "AvailbleMaps/" + colonyName + "/General";
        textAsset = Resources.Load(path) as TextAsset;
        if (textAsset == null) { Debug.Log("ERROR! Dont found General file in " + colonyName); }
        else
        {
            Stream stream = new MemoryStream(textAsset.bytes);
            try { GeneralD = formater.Deserialize(stream) as GeneralWorldData; }
            catch { Debug.Log("ERROR! General file in " + colonyName + " has the wrong format"); }
            stream.Close();
        }

        //Player
        PlayerData PlayerD = null;
        path = "AvailbleMaps/" + colonyName + "/Player";
        textAsset = Resources.Load(path) as TextAsset;
        if (textAsset == null) { Debug.Log("ERROR! Dont found Player file in " + colonyName); }
        else
        {
            Stream stream = new MemoryStream(textAsset.bytes);
            try { PlayerD = formater.Deserialize(stream) as PlayerData; }
            catch { Debug.Log("ERROR! Player file in " + colonyName + " has the wrong format"); }
            stream.Close();
        }

        //Terrain
        TerrainData TerrainD = null;
        path = "AvailbleMaps/" + colonyName + "/Terrain";
        textAsset = Resources.Load(path) as TextAsset;
        if (textAsset == null) { Debug.Log("ERROR! Dont found Terrain file in " + colonyName); }
        else
        {
            Stream stream = new MemoryStream(textAsset.bytes);
            try { TerrainD = formater.Deserialize(stream) as TerrainData; }
            catch { Debug.Log("ERROR! Terrain file in " + colonyName + " has the wrong format"); }
            stream.Close();
        }

        //Buildings
        BuildingsData BuildingsD = null;
        path = "AvailbleMaps/" + colonyName + "/Buildings";
        textAsset = Resources.Load(path) as TextAsset;
        if (textAsset == null) { Debug.Log("ERROR! Dont found Buildings file in " + colonyName); }
        else
        {
            Stream stream = new MemoryStream(textAsset.bytes);
            try { BuildingsD = formater.Deserialize(stream) as BuildingsData; }
            catch { Debug.Log("ERROR! Buildings file in " + colonyName + " has the wrong format"); }
            stream.Close();
        }

        //Enemy
        EnemyData EnemyD = null;
        path = "AvailbleMaps/" + colonyName + "/Enemy";
        textAsset = Resources.Load(path) as TextAsset;
        if (textAsset == null) { Debug.Log("ERROR! Dont found Enemy file in " + colonyName); }
        else
        {
            Stream stream = new MemoryStream(textAsset.bytes);
            try { EnemyD = formater.Deserialize(stream) as EnemyData; }
            catch { Debug.Log("ERROR! Enemy file in " + colonyName + " has the wrong format"); }
            stream.Close();
        }

        //Units
        UnitsData UnitsD = null;
        path = "AvailbleMaps/" + colonyName + "/Units";
        textAsset = Resources.Load(path) as TextAsset;
        if (textAsset == null) { Debug.Log("ERROR! Dont found Units file in " + colonyName); }
        else
        {
            Stream stream = new MemoryStream(textAsset.bytes);
            try { UnitsD = formater.Deserialize(stream) as UnitsData; }
            catch { Debug.Log("ERROR! Units file in " + colonyName + " has the wrong format"); }
            stream.Close();
        }

        data = new WorldData(GeneralD, PlayerD, TerrainD, BuildingsD, EnemyD, UnitsD);
        return data;
    }
    public static void DeleteColony(string colonyName)
    {
        string path = GetColoniesPath() + colonyName;
        if (!Directory.Exists(path))
        {
            Debug.Log("Colony was not deleted because it was not found in: " + path); return;
        }

        Directory.Delete(path, true);
    }

    public static void SaveWorld(string name, string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string path = directoryPath + name;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        Debug.Log("saving...");
        Debug.Log(path);

        WorldData wData = WorldMenager.instance.CrateWorldDate(name);
        if (wData == null) { Debug.Log("ERROR! Cant save, world data is missing"); return; }

        BinaryFormatter formater = new BinaryFormatter();
        FileStream stream;

        //General
        if (wData.General == null) { Debug.Log("ERROR! Cant save game! General is missing"); return; }
        stream = new FileStream(path + "/General" + "." + GetSaveExtension, FileMode.Create);
        formater.Serialize(stream, wData.General);
        stream.Close();
        //Player
        if (wData.Player == null) { Debug.Log("ERROR! Cant save game! Player is missing"); return; }
        stream = new FileStream(path + "/Player" + "." + GetSaveExtension, FileMode.Create);
        formater.Serialize(stream, wData.Player);
        stream.Close();
        //Terrain
        if (wData.Terrain == null) { Debug.Log("ERROR! Cant save game! Terrain is missing"); return; }
        stream = new FileStream(path + "/Terrain" + "." + GetSaveExtension, FileMode.Create);
        formater.Serialize(stream, wData.Terrain);
        stream.Close();
        //Buildings
        if (wData.Buildings == null) { Debug.Log("ERROR! Cant save game! Buildings is missing"); return; }
        stream = new FileStream(path + "/Buildings" + "." + GetSaveExtension, FileMode.Create);
        formater.Serialize(stream, wData.Buildings);
        stream.Close();
        //Enemy
        if (wData.Enemy == null) { Debug.Log("ERROR! Cant save game! Enemy is missing"); return; }
        stream = new FileStream(path + "/Enemy" + "." + GetSaveExtension, FileMode.Create);
        formater.Serialize(stream, wData.Enemy);
        stream.Close();
        //Units
        if (wData.Units == null) { Debug.Log("ERROR! Cant save game! Units is missing"); return; }
        stream = new FileStream(path + "/Units" + "." + GetSaveExtension, FileMode.Create);
        formater.Serialize(stream, wData.Units);
        stream.Close();
    }
    public static WorldData GetWorldData(string path)
    {
        if (!Directory.Exists(path))
        {
            Debug.Log("directory not found in: " + path);
            return null;
        }

        BinaryFormatter formater = new BinaryFormatter();
        FileStream stream;
        WorldData data;
        string filePath;

        //General
        GeneralWorldData GeneralD = null;
        filePath = path + "/General" + "." + GetSaveExtension;
        if (File.Exists(filePath))
        {
            stream = new FileStream(filePath, FileMode.Open);
            try { GeneralD = formater.Deserialize(stream) as GeneralWorldData; } catch { Debug.Log("ERROR! General in save has the wrong format"); }
            stream.Close();
        }
        //Player
        PlayerData PlayerD = null;
        filePath = path + "/Player" + "." + GetSaveExtension;
        if (File.Exists(filePath))
        {
            stream = new FileStream(filePath, FileMode.Open);
            try { PlayerD = formater.Deserialize(stream) as PlayerData; } catch { Debug.Log("ERROR! Player in save has the wrong format"); }
            stream.Close();
        }
        //Terrain
        TerrainData TerrainD = null;
        filePath = path + "/Terrain" + "." + GetSaveExtension;
        if (File.Exists(filePath))
        {
            stream = new FileStream(filePath, FileMode.Open);
            try { TerrainD = formater.Deserialize(stream) as TerrainData; } catch { Debug.Log("ERROR! Terrain in save has the wrong format"); }
            stream.Close();
        }
        //Buildings
        BuildingsData BuildingsD = null;
        filePath = path + "/Buildings" + "." + GetSaveExtension;
        if (File.Exists(filePath))
        {
            stream = new FileStream(filePath, FileMode.Open);
            try { BuildingsD = formater.Deserialize(stream) as BuildingsData; } catch { Debug.Log("ERROR! Buildings in save has the wrong format"); }
            stream.Close();
        }
        //Enemy
        EnemyData EnemyD = null;
        filePath = path + "/Enemy" + "." + GetSaveExtension;
        if (File.Exists(filePath))
        {
            stream = new FileStream(filePath, FileMode.Open);
            try { EnemyD = formater.Deserialize(stream) as EnemyData; } catch { Debug.Log("ERROR! Enemy in save has the wrong format"); }
            stream.Close();
        }
        //Units
        UnitsData UnitsD = null;
        filePath = path + "/Units" + "." + GetSaveExtension;
        if (File.Exists(filePath))
        {
            stream = new FileStream(filePath, FileMode.Open);
            try { UnitsD = formater.Deserialize(stream) as UnitsData; } catch { Debug.Log("ERROR! Units in save has the wrong format"); }
            stream.Close();
        }

        data = new WorldData(GeneralD, PlayerD, TerrainD, BuildingsD, EnemyD, UnitsD);
        return data;
    }

    public static void RetryGame()
    {
        SceneLoader.instance.LoadWorldScene();
    }
    public static bool IsSavesExist(string path)
    {
        if (Directory.Exists(path))
        {
            return true;
        }

        Debug.Log("File not found in: " + path);
        return false;
    }
    public static void DeleteSave(string path)
    {
        if (!Directory.Exists(path))
        {
            Debug.Log("Save not found in: " + path); return;
        }

        Directory.Delete(path, true);
    }
}