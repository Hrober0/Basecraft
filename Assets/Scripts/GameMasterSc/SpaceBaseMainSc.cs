using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class SpaceBaseMainSc : MonoBehaviour
{
    [System.Serializable]
    public class ItemsData
    {
        public int[] itemsInStorage;
        public int[] itemsInDumpster;

        public Res[] resToRemoveList;
        public float dumpsterTimeToRemoveItem;
        public bool activeRemovingBar;

        public ItemsData(int[] itemsInStorage, int[] itemsInDumpster, Res[] resToRemoveList, float dumpsterTimeToRemoveItem, bool activeRemovingBar)
        {
            this.itemsInStorage = itemsInStorage;
            this.itemsInDumpster = itemsInDumpster;
            this.resToRemoveList = resToRemoveList;
            this.dumpsterTimeToRemoveItem = dumpsterTimeToRemoveItem;
            this.activeRemovingBar = activeRemovingBar;
        }
    }
    
    [System.Serializable]
    public class TechnologiesData
    {
        public int[] technologies;

        public TechnologiesData(int[] technologies)
        {
            this.technologies = technologies;
        }
    }

    [System.Serializable]
    public class GeneralGameData
    {
        public ColonyNames[] availableColonies;
        public ColonyNames[] unlockedColonies;

        public GameEventControler.GameEvent[] activeGameEvent;
        public GameEventControler.GameEvent[] complateGameEvent;
        public bool skippedTotorial = false;
        public int[] evenValues;

        public string date;

        public ColonyNames[] renewingColonies;
        public string renewingColoniesDate;

        public int[][] stats;

        public GeneralGameData(ColonyNames[] availableColonies, ColonyNames[] unlockedColonies, GameEventControler.GameEvent[] activeGameEvent, GameEventControler.GameEvent[] complateGameEvent, bool skippedTotorial, int[] evenValues, string date, int[][] stats)
        {
            this.availableColonies = availableColonies;
            this.unlockedColonies = unlockedColonies;
            this.activeGameEvent = activeGameEvent;
            this.complateGameEvent = complateGameEvent;
            this.skippedTotorial = skippedTotorial;
            this.evenValues = evenValues;
            this.date = date;
            this.stats = stats;
        }
    }

    [Header("Settings")]
    public bool GetAllTechs = false;
    public bool CreativeModeOn = false;
    public bool NeedItemToBuild = true;

    [Header("Veribals")]
    public int numberOfRes;
    private float timer = 0f;
    public float colonyAutoSaveDelay = 120f;
    private float avaTimeToAutoSaveColony = 0f;
    private float colonySaveMinDelay = 10f;
    private float avaTimeToSaveColony = 10f;

    [Header("General game data")]
    public List<ColonyNames> newColoniesToScan = new List<ColonyNames>();
    public List<ColonyNames> unlockedColonies = new List<ColonyNames>();
    public List<ColonyNames> availableColonies = new List<ColonyNames>();
    private GeneralWorldData[] generalWordDatas;
    public int selectedColony = -1;
    public bool skippedTotorial = false;

    public List<ColonyNames> coloniesToReset = new List<ColonyNames>();
    private List<DateTime> coloniesToResetDateTime = new List<DateTime>();
    private float checkingTime = 1f;
    private float avaTimeToCheck = 1f;

    [Header("Storage")]
    [SerializeField] private int maxEmptySpaceInStorage = 500;
    [SerializeField] private int[] itemsInStorage;

    [Header("Dumpster")]
    [SerializeField] private int maxEmptySpaceInDumpster = 50;
    [SerializeField] private int[] itemsInDumpster;
    public bool activeRemovingBar = false;
    public float dumpsterItemRemovingTime = 30f;
    public float avaTimeToRemoveItem = 0f;

    [Header("Technologies")]
    [SerializeField] private List<Technologies> discoveredTechnologies;
    

    public static SpaceBaseMainSc instance;
    private void Awake()
    {
        if (instance != null) { Debug.Log("More then one " + this + " on scen, return."); Destroy(this.gameObject); return; }
        instance = this;
        numberOfRes = Enum.GetNames(typeof(Res)).Length;

        SetAfterStart();
    }
    private void Start()
    {
        //play music
        switch (SceneLoader.instance.gameMode)
        {
            case GameState.SpaceStation: AudioManager.instance.PlayMusicOfMenu(); break;
            case GameState.Colony: AudioManager.instance.PlayMusicOfColony(); break;
        }
        AudioManager.instance.FadeUpMusic();
    }
    private void SetAfterStart()
    {
        Time.timeScale = 1f; Time.fixedDeltaTime = 1f;

        itemsInStorage = new int[numberOfRes];
        SetMaxEmptySpaceInStorage(maxEmptySpaceInStorage);

        itemsInDumpster = new int[numberOfRes];
        SetMaxEmptySpaceInDumpster(maxEmptySpaceInDumpster);

        ReadGeneralGameData();
        ReadTechnologyData();
        ReadItemsData();

        SetToRemoveRes();

        selectedColony = -1;
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (activeRemovingBar)
        {
            if (avaTimeToRemoveItem < timer) { RemoveLastRes(); }
        }

        if (avaTimeToCheck < timer)
        {
            avaTimeToCheck = checkingTime + timer;
            CheckColonyToReset();
        }

        if (selectedColony != -1 && colonyAutoSaveDelay > 0)
        {
            if (avaTimeToAutoSaveColony < timer)
            {
                avaTimeToAutoSaveColony = colonyAutoSaveDelay + timer;
                SaveColony();
            }
        }
    }

    //general data
    private List<GameEventControler.GameEvent> eventsToRun = new List<GameEventControler.GameEvent>();
    private void ReadGeneralGameData()
    {
        string[] colonyNames = Enum.GetNames(typeof(ColonyNames));
        int numOfColonies = colonyNames.Length;
        unlockedColonies = new List<ColonyNames>();
        availableColonies = new List<ColonyNames>();
        eventsToRun = new List<GameEventControler.GameEvent>();

        generalWordDatas = new GeneralWorldData[numOfColonies];
        for (int i = 0; i < numOfColonies; i++)
        {
            generalWordDatas[i] = SaveLoadControler.GetGeneralWorldDataFromResources(colonyNames[i]);
        }

        GameEventControler.activeGameEvents = new List<GameEventControler.GameEvent>();
        GameEventControler.complateGameEvent = new List<GameEventControler.GameEvent>();

        GeneralGameData generalData = SaveLoadControler.ReadGeneralGameData();
        if (generalData == null)
        {
            Debug.Log("Dont found general game data file, setting default");
            eventsToRun.Add(GameEventControler.GameEvent.P1_1);
            Invoke("InvokeEvents", 1f);
            return;
        }

        foreach (int colony in generalData.availableColonies) { availableColonies.Add((ColonyNames)colony); }
        foreach (int colony in generalData.unlockedColonies) { unlockedColonies.Add((ColonyNames)colony); }

        foreach (GameEventControler.GameEvent gameEvent in generalData.complateGameEvent) { GameEventControler.complateGameEvent.Add(gameEvent); }
        foreach (GameEventControler.GameEvent gameEvent in generalData.activeGameEvent) { eventsToRun.Add(gameEvent); }
        Invoke("InvokeEvents", 1f);

        TaskManager.instance.SetStats(generalData.stats);
    }
    public void SaveGeneralData()
    {
        string date = System.DateTime.Now.ToLongTimeString() + " " + System.DateTime.Now.ToShortDateString();
        GeneralGameData generalGameData = new GeneralGameData(
            availableColonies.ToArray(),
            unlockedColonies.ToArray(),
            GameEventControler.activeGameEvents.ToArray(),
            GameEventControler.complateGameEvent.ToArray(),
            skippedTotorial,
            new int[0],
            date,
            TaskManager.instance.GetStats()
            );
        SaveLoadControler.SaveGeneralGameData(generalGameData);
    }
    public GeneralWorldData GetWorldData(ColonyNames colonyName) => generalWordDatas[(int)colonyName];
    private void InvokeEvents()
    {
        if (eventsToRun.Count == 0) { GameEventControler.CallNextAvailableEvent(); return; }

        foreach (GameEventControler.GameEvent gameEvent in eventsToRun) { GameEventControler.StartEvent(gameEvent); }
        eventsToRun = new List<GameEventControler.GameEvent>();
    }

    //Scene changing
    public void BackToMenu(bool saveWorld = true)
    {
        Debug.Log("Back to menu");

        SaveGeneralData();

        if (saveWorld) { SaveColony(); }

        Time.timeScale = 1f; Time.fixedDeltaTime = 1f;

        selectedColony = -1;

        AudioManager.instance.FadeDownMusic();

        SceneLoader.instance.gameMode = GameState.SpaceStation;
        SceneLoader.instance.LoadSpaceStationScene();

        AudioManager.instance.PlayMusicOfMenu();
        AudioManager.instance.FadeUpMusic();
    }
    public void LoadColony(WorldData worldData)
    {
        string colonyName = worldData.General.saveName;
        Debug.Log("Load colony "+ colonyName);
        if(Enum.TryParse(colonyName, out ColonyNames colonyE)) { selectedColony = (int)colonyE; }
        else { selectedColony = -1; Debug.Log("ERROR! Loaded colony have wrong name!"); }
        avaTimeToAutoSaveColony = colonyAutoSaveDelay + timer;

        AudioManager.instance.FadeDownMusic();

        SceneLoader.instance.gameMode = GameState.Colony;
        SceneLoader.instance.gameLoadingMode = SceneLoader.GameLoadingMode.LoadFromWorldData;
        SceneLoader.instance.worldData = worldData;
        SceneLoader.instance.LoadWorldScene();

        AudioManager.instance.PlayMusicOfColony();
        AudioManager.instance.FadeUpMusic();
    }

    //colony
    public void LeaveColony(ColonyNames colony)
    {
        unlockedColonies.Remove(colony);
        SaveLoadControler.DeleteColony(colony.ToString());

        if (selectedColony != -1)
        {
            ColonyNames selectedColonyName = (ColonyNames)selectedColony;
            if (selectedColonyName == colony) { BackToMenu(false); }
        }

        GeneralWorldData generalWorldData = generalWordDatas[(int)colony];
        if (generalWorldData == null) { return; }

        DateTime dateTime = System.DateTime.Now;
        int cooldown = generalWorldData.colonyCooldown;
        if (cooldown == 0) { cooldown = 30; }
        dateTime = dateTime.AddSeconds(cooldown);
        coloniesToReset.Add(colony);
        coloniesToResetDateTime.Add(dateTime);
    }
    private void CheckColonyToReset()
    {
        DateTime dateTime = System.DateTime.Now;
        int removedIndexs = 0;
        int lenght = coloniesToReset.Count;
        int index;
        for (int i = 0; i < lenght; i++)
        {
            index = i - removedIndexs;
            if (DateTime.Compare(dateTime, coloniesToResetDateTime[index]) == 1)
            {
                coloniesToReset.RemoveAt(index);
                coloniesToResetDateTime.RemoveAt(index);
                removedIndexs++;
            }
        }
    }
    public DateTime GetDataTimeOfColonyToReset(ColonyNames colony)
    {
        for (int i = 0; i < coloniesToReset.Count; i++)
        {
            if (coloniesToReset[i]== colony)
            {
                return coloniesToResetDateTime[i];
            }
        }
        return DateTime.Now;
    }
    public void ResetAllGameProgrs()
    {
        SaveLoadControler.DeleteMainGameData();

        Debug.Log("Restart scripts");
        MessageManager.instance.CloseAllMess();
        TaskManager.instance.ResetAllTask();
        LeftPanel.instance.ResetSciencePanel();

        SetAfterStart();

        SceneLoader.instance.LoadSpaceStationScene(); 
    }
    public void SaveColony()
    {
        if (selectedColony == -1) { return; }

        if (avaTimeToSaveColony > timer) { Debug.Log("Did't saved world because last save was " + (colonySaveMinDelay - (avaTimeToSaveColony - timer)) + "s ago."); return; }
        avaTimeToSaveColony = timer + colonySaveMinDelay;

        if (WorldMenager.instance == null) { Debug.Log("Cant save world because WorldMenager is equal null"); return; }

        LeftPanel.instance.SetActiveGameSavingPanel(true);
        SaveLoadControler.SaveWorld(SceneLoader.instance.generalWorldData.saveName, SaveLoadControler.GetColoniesPath());
        LeftPanel.instance.SetActiveGameSavingPanel(false);
    }
    public void UnlockColony(ColonyNames colony)
    {
        availableColonies.Add(colony);
        newColoniesToScan.Add(colony);
        if (SpaceStationUIControler.instance != null) { SpaceStationUIControler.instance.ScanPlanet(); }
    }

    //items data
    private void ReadItemsData()
    {
        ItemsData itemsData = SaveLoadControler.ReadItemsData();
        if (itemsData == null) { Debug.Log("Dont found items file!"); return; }

        //storage
        for (int i = 1; i < itemsData.itemsInStorage.Length; i++) { itemsInStorage[i] = itemsData.itemsInStorage[i]; }
        SetMaxEmptySpaceInStorage(maxEmptySpaceInStorage);

        //dumpster
        for (int i = 1; i < itemsData.itemsInDumpster.Length; i++) { itemsInDumpster[i] = itemsData.itemsInDumpster[i]; }
        SetMaxEmptySpaceInDumpster(maxEmptySpaceInDumpster);

        resToRemoveList = new List<Res>();
        for (int i = 0; i < itemsData.resToRemoveList.Length; i++) { resToRemoveList.Add(itemsData.resToRemoveList[i]); }

        avaTimeToRemoveItem = itemsData.dumpsterTimeToRemoveItem;
        activeRemovingBar = itemsData.activeRemovingBar;
    }
    public void SaveItems()
    {
        ItemsData itemsData = new ItemsData
            (
            itemsInStorage,
            itemsInDumpster,
            resToRemoveList.ToArray(),
            avaTimeToRemoveItem,
            activeRemovingBar
            );

        SaveLoadControler.SaveItemsData(itemsData);
    }

    //storage
    public void AddResToStorage(Res res, int qua, bool save=true)
    {
        itemsInStorage[0] -= qua;
        itemsInStorage[(int)res] += qua;

        if (save) { SaveItems(); }
    }
    public int GetResQuaOfStorage(Res res) => itemsInStorage[(int)res];
    public int GetFreeSpaceOfStorage() => itemsInStorage[0];
    public int GetMaxEmptySpaceOfStorage() => maxEmptySpaceInStorage;
    public void SetMaxEmptySpaceInStorage(int qua)
    {
        maxEmptySpaceInStorage = qua;
        itemsInStorage[0] = maxEmptySpaceInStorage;
        for (int i = 1; i < itemsInStorage.Length; i++)
        {
            itemsInStorage[0] -= itemsInStorage[i];
        }
    }

    //dumpster
    private List<Res> resToRemoveList = new List<Res>();
    private void RemoveLastRes()
    {
        avaTimeToRemoveItem = dumpsterItemRemovingTime + timer;

        int missnumber = 0;
        int lenght = resToRemoveList.Count;
        for (int i = 0; i < lenght; i++)
        {
            int index = i - missnumber;
            Res res = resToRemoveList[index];
            if (itemsInDumpster[(int)res] <= 0) { missnumber++; resToRemoveList.RemoveAt(index); }
        }

        if (resToRemoveList.Count == 0) { activeRemovingBar = false; return; }

        Res resTR = resToRemoveList[0];
        itemsInDumpster[(int)resTR]--;
        itemsInDumpster[0]++;
        if (itemsInDumpster[(int)resTR] <= 0) { resToRemoveList.RemoveAt(0); }
    }
    private void SetToRemoveRes()
    {
        if (resToRemoveList.Count == 0) { activeRemovingBar = false; return; }

        activeRemovingBar = true;
        avaTimeToRemoveItem = dumpsterItemRemovingTime;
    }
    private void AddResToDumpster(Res res, int qua, bool save=true)
    {
        itemsInDumpster[(int)res] += qua;
        itemsInDumpster[0] -= qua;

        if (save) { SaveItems(); }
    }
    public bool MoveItemToDumpster(Res res, int qua)
    {
        int resIndex = (int)res;

        if (qua <= 0) { return false; }
        if (itemsInStorage[resIndex] < qua) { return false; }
        if (itemsInDumpster[0] < qua) { return false; }

        AddResToStorage(res, -qua, false);
        AddResToDumpster(res, qua, false);
        if (resToRemoveList.Contains(res) == false) { resToRemoveList.Add(res); }

        SaveItems();

        if (activeRemovingBar==false)
        {
            avaTimeToRemoveItem = dumpsterItemRemovingTime;
            activeRemovingBar = true;
        }
        
        return true;
    }
    public bool MoveItemToStorage(Res res, int qua)
    {
        int resIndex = (int)res;

        if (qua <= 0) { return false; }
        if (itemsInStorage[0] < qua) { return false; }
        if (itemsInDumpster[resIndex] < qua) { return false; }

        AddResToStorage(res, qua, false);
        AddResToDumpster(res, -qua, false);

        SaveItems();

        return true;
    }
    public int GetResQuaOfDumpster(Res res) => itemsInDumpster[(int)res];
    public int GetFreeSpaceOfDumpster() => itemsInDumpster[0];
    public int GetMaxEmptySpaceOfDumpster() => maxEmptySpaceInDumpster;
    public void SetMaxEmptySpaceInDumpster(int qua)
    {
        maxEmptySpaceInDumpster = qua;
        itemsInDumpster[0] = maxEmptySpaceInDumpster;
        for (int i = 1; i < itemsInDumpster.Length; i++)
        {
            itemsInDumpster[0] -= itemsInDumpster[i];
        }
    }

    //technology
    private void ReadTechnologyData()
    {
        discoveredTechnologies = new List<Technologies>();

        if (GetAllTechs)
        {
            discoveredTechnologies = GetAllTechnologies();
            Debug.Log("AddAllTechnologies");
        }
        else
        {
            TechnologiesData technologiesData = SaveLoadControler.ReadTechnologiesData();
            if (technologiesData == null)
            {
                Debug.Log("Dont found technologies file, set default");
                discoveredTechnologies = GetDefaultTechnologies();
            }
            else
            {
                for (int i = 0; i < technologiesData.technologies.Length; i++)
                {
                    discoveredTechnologies.Add((Technologies)technologiesData.technologies[i]);
                }

                //check default
                List<Technologies> technologies = GetDefaultTechnologies();
                foreach (Technologies tech in technologies)
                {
                    if (discoveredTechnologies.Contains(tech) == false) { discoveredTechnologies.Add(tech); }
                }
            }
        }

        SaveTechs();

        UpdateTechs();

        List<Technologies> GetDefaultTechnologies()
        {
            List<Technologies> tech = new List<Technologies>
            {
            Technologies.DroneComunication, Technologies.Connection1,
            Technologies.Warehouse1, Technologies.Launchpad,
            Technologies.Smelter, Technologies.StoneBrick, Technologies.CopperPlate,
            Technologies.BasicCrafter, Technologies.CopperCable, Technologies.Planks,
            };

            return tech;
        }
        List<Technologies> GetAllTechnologies()
        {
            List<Technologies> techs = new List<Technologies>();
            string[] names = Enum.GetNames(typeof(Technologies));
            foreach (string sName in names)
            {
                Enum.TryParse(sName, out Technologies res);
                techs.Add(res);
            }
            return techs;
        }
    }
    public void UpdateTechs()
    {
        AllRecipes.instance.ActActiveOfRecipe();
        AllRecipes.instance.ActUnlockRes();
        LeftPanel.instance.UpdateGlossaryItemCurtains();
        if (GuiControler.instance != null)
        {
            GuiControler.instance.SetAllBuildButtons();
            GuiControler.instance.SetAllRecipesListPanel();
            GuiControler.instance.SetPlatformItemSelectPanel();
        }
    }
    private void SaveTechs()
    {
        int[] techTab = new int[discoveredTechnologies.Count];
        for (int i = 0; i < discoveredTechnologies.Count; i++) { techTab[i] = (int)discoveredTechnologies[i]; }
        TechnologiesData technologiesData = new TechnologiesData(techTab);
        SaveLoadControler.SaveTechnolgiesData(technologiesData);
    }
    public bool IsTechnologyDiscovered(Technologies tech)
    {
        return discoveredTechnologies.Contains(tech);
    }
    public void DiscoverTechnology(TechnologySc techSc)
    {
        if (techSc.discovered == true) { return; }

        if (CanDiscoverTechnology(techSc) == false) { return; }

        foreach (ItemRAQ item in techSc.needItems)
        {
            AddResToStorage(item.res, -item.qua);
        }

        techSc.discovered = true;

        Debug.Log("Discover technology (" + techSc.thisTechnology + ")");
        discoveredTechnologies.Add(techSc.thisTechnology);
        LeftPanel.instance.UnlockTech(techSc);

        SaveTechs();

        UpdateTechs();
    }
    public bool CanDiscoverTechnology(TechnologySc techSc)
    {
        if (techSc.needItems.Count == 0) { return false; }
        foreach (ItemRAQ item in techSc.needItems)
        {
            if (GetResQuaOfStorage(item.res) < item.qua) { return false; }
        }
        foreach (Technologies tech in techSc.needTechnology)
        {
            if (IsTechnologyDiscovered(tech) == false) {  return false; }
        }
        return true;
    }
}