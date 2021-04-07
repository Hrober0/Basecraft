using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuControler : MonoBehaviour
{
    public Text versionText;

    [Header("Sandbox")]
    public Transform SandboxPanel;
    public Text mapWidthText;
    public Slider mapWidthSlider;
    public Text mapHeightText;
    public Slider mapHeightSlider;
    public Text startItemsText;
    public Slider startItemsSlider;
    public Text mountainsSizesText;
    public Slider mountainsSizesSlider;
    public Text oreSizesText;
    public Slider oreSizesSlider;
    public Text forestSizesText;
    public Slider forestSizesSlider;

    public Text difficultyText;
    public Slider difficultySlider;

    [Header("Settings")]
    public Transform SettingsPanel;
    public Toggle resImageInBuildingToggl;
    public Toggle fullscreenToggl;

    [Header("Save")]
    public Transform SavePanel;
    public Transform SaveButtonsParent;
    public GameObject SaveButton;
    public GameObject ConfirmSavePanel;
    public InputField SaveNameInput;
    public GameObject ConfirmDeleteInSavePanel;

    [Header("Load")]
    public Transform LoadPanel;
    public Transform LoadButtonsParent;
    public GameObject LoadButton;
    public GameObject ConfirmDeleteInLoadPanel;


    [Header("Veribals")]
    private string[] savesPaths;
    private string nameOfSave = "";
    private string pathOfOldSave;
    private string pathOfSaveToDelete;

    private Transform nowOpenPanle = null;

    public static MenuControler instance;
    private void Awake()
    {
        if (instance == null) { instance = this; }
    }
    private void Start()
    {
        SetOnStart();
    }
    private void OnEnable()
    {
        SetOnStart();
    }
    private void SetOnStart()
    {
        CloseNowOpenPanel();
        if (SceneLoader.instance == null) { return; }
        if (SceneLoader.instance.gameMode == GameState.MainMenu)
        {
            SetSandboxDefaulValues();
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 1f;
            versionText.text = "version " + SettingsManager.instance.gameVersion;
        }
        SetAllSettings();
    }

    
    public void ResumeGame()
    {
        if (SceneLoader.instance.gameMode != GameState.MainMenu)
        {
            GuiControler.instance.HidePauseMenu();
            switch (ClickMenager.instance.saveGameSpeed)
            {
                case 0: WorldMenager.instance.SetGameSpeed(0); break;
                case 1: WorldMenager.instance.SetGameSpeed(1); break;
                case 2: WorldMenager.instance.SetGameSpeed(2); break;
            }
        }
    }
    public void RetryGame()
    {
        SaveLoadControler.RetryGame();
    }
    public void GoToMainMenu()
    {
        SceneLoader.instance.gameMode = GameState.MainMenu;
        SceneLoader.instance.LoadMainMenuScene();
    }
    public void QuitGame()
    {
        Debug.Log("Exiting..");
        Application.Quit();
    }

    public void CloseNowOpenPanel()
    {
        if (nowOpenPanle == null) { return; }
        nowOpenPanle.gameObject.SetActive(false);
        nowOpenPanle = null;
    }

    //Campaign


    //Sandbox
    public void LoadSandBox()
    {
        SceneLoader.instance.gameMode = GameState.Sandbox;
        SceneLoader.instance.gameLoadingMode = SceneLoader.GameLoadingMode.CreateNew;
        SceneLoader.instance.canChooseStartPlace = true;
        SceneLoader.instance.LoadWorldScene();
    }
    public void OpenSandboxPanel()
    {
        CloseNowOpenPanel();
        nowOpenPanle = SandboxPanel;
        SandboxPanel.gameObject.SetActive(true);
    }
    private void SetSandboxDefaulValues()
    {
        SceneLoader.instance.SetDefaultGeneralWorldData();

        //map
        ChangeMapWidth(0.5f);
        mapWidthSlider.value = 0.5f;
        ChangeMapHeight(0.5f);
        mapHeightSlider.value = 0.5f;
        ChangeMountainsSizes(1f);
        mountainsSizesSlider.value = 1f;
        ChangeOreSizes(1f);
        oreSizesSlider.value = 1f;
        ChangeForestSizes(1f);
        forestSizesSlider.value = 1f;
        ChangeStartItems(2);
        startItemsSlider.value = 2;

        //enemy
        Difficulty defDifficulty = Difficulty.Normal;
        SceneLoader.instance.generalWorldData.difficulty = defDifficulty;
        difficultySlider.value = (int)defDifficulty;
    }
    public void ChangeMapWidth(float value)
    {
        int trueValue = ScaleFTI(value);
        SceneLoader.instance.generalWorldData.mapWidth = trueValue;
        mapWidthText.text = string.Format("Map width: {0}", trueValue);
    }
    public void ChangeMapHeight(float value)
    {
        int trueValue = ScaleFTI(value);
        SceneLoader.instance.generalWorldData.mapHeight = trueValue;
        mapHeightText.text = string.Format("Map height: {0}", trueValue);
    }
    public void ChangeStartItems(float value)
    {
        switch (value)
        {
            case 0: startItemsText.text = "Starting resources: none";       SceneLoader.instance.numberOfStartItems = 0; break;
            case 1: startItemsText.text = "Starting resources: a few";      SceneLoader.instance.numberOfStartItems = 1; break;
            case 2: startItemsText.text = "Starting resources: default";    SceneLoader.instance.numberOfStartItems = 2; break;
            case 3: startItemsText.text = "Starting resources: many";       SceneLoader.instance.numberOfStartItems = 3; break;
            case 4: startItemsText.text = "Starting resources: tons";       SceneLoader.instance.numberOfStartItems = 4; break;
        }
    }
    public void ChangeMountainsSizes(float value)
    {
        mountainsSizesText.text = string.Format("Mountains: {0}%", (int)(value * 100));
        SceneLoader.instance.generalWorldData.mountainsSizes = value;
    }
    public void ChangeOreSizes(float value)
    {
        oreSizesText.text = string.Format("Ore: {0}%", (int)(value * 100));
        SceneLoader.instance.generalWorldData.oreSizes = value;
    }
    public void ChangeForestSizes(float value)
    {
        forestSizesText.text = string.Format("Forest: {0}%", (int)(value * 100));
        SceneLoader.instance.generalWorldData.forestSizes = value;
    }

    public void ChangeDifficulty(float value)
    {
        switch ((Difficulty)value)
        {
            case Difficulty.Peaceful:  SceneLoader.instance.generalWorldData.difficulty = Difficulty.Peaceful; difficultyText.text = "Difficulty level: peaceful"; break;
            case Difficulty.Normal:    SceneLoader.instance.generalWorldData.difficulty = Difficulty.Normal; difficultyText.text = "Difficulty level: normal"; break;
        }
    }

    //Settings
    public void OpenSettingsPanel()
    {
        CloseNowOpenPanel();
        nowOpenPanle = SettingsPanel;
        SettingsPanel.gameObject.SetActive(true);
    }
    private void SetAllSettings()
    {
        resImageInBuildingToggl.isOn = SettingsManager.instance.GetResImageInBuilding();
        fullscreenToggl.isOn = SettingsManager.instance.GetFullscreen();
    }
    public void SettingsSetResImageInBuilding(bool value) => SettingsManager.instance.SetResImageInBuilding(value);
    public void SettingsSetFullscreen(bool value) => SettingsManager.instance.SetFullscreen(value);

    //Saves
    public void OpenSavePanel()
    {
        CloseNowOpenPanel();
        nowOpenPanle = SavePanel;
        SavePanel.gameObject.SetActive(true);
        ConfirmSavePanel.SetActive(false);

        nameOfSave = "";
        SaveNameInput.text = "";
        savesPaths = SaveLoadControler.GetAllSaves();
        SaveButton.SetActive(false);
        ConfirmDeleteInSavePanel.SetActive(false);
        ConfirmSavePanel.SetActive(false);
        for (int i = 1; i < SaveButtonsParent.childCount; i++) { Destroy(SaveButtonsParent.GetChild(i).gameObject); }
        for (int i = 0; i < savesPaths.Length; i++)
        {
            WorldData Data = SaveLoadControler.GetWorldData(savesPaths[i]);
            if (Data == null) { continue; }
            if (Data.General == null) { continue; }

            string name = SaveLoadControler.GetNameFromPath(savesPaths[i]);
            GameObject newButton = Instantiate(SaveButton);
            newButton.transform.SetParent(SaveButtonsParent, false);
            newButton.name = name;
            newButton.SetActive(true);
            newButton.GetComponentInChildren<Text>().text = name;
            
            string typeText = "Sandbox time: ";
            int timeInt = (int)Data.General.worldTime;
            int s = timeInt % 60;
            int m = ((timeInt - s) / 60) % 60;
            int h = (timeInt - m * 60 - s) / 3600;
            newButton.transform.GetChild(0).GetComponent<Text>().text = name;
            newButton.transform.GetChild(1).GetComponent<Text>().text = Data.General.date;
            newButton.transform.GetChild(2).GetComponent<Text>().text = typeText + string.Format("{0}:{1}:{2}", h, m, s);
        }
    }
    public void SaveGameAsOld()
    {
        int index = EventSystem.current.currentSelectedGameObject.transform.GetSiblingIndex();
        index--;
        if (index < 0 || index >= savesPaths.Length) { Debug.Log("ERROR! path not found"); return; }
        string path = savesPaths[index];
        pathOfOldSave = path;
        nameOfSave = SaveLoadControler.GetNameFromPath(path);
        ConfirmSavePanel.SetActive(true);
        SaveNameInput.transform.Find("Placeholder").GetComponent<Text>().text = nameOfSave;
    }
    public void SaveGameAsNew()
    {
        ConfirmSavePanel.SetActive(true);
        nameOfSave = "";
        pathOfOldSave = "";
        switch (SceneLoader.instance.gameMode)
        {
            case GameState.Level: nameOfSave = "Level"; break;
            case GameState.Sandbox: nameOfSave = "Sandbox"; break;
        }
        SaveNameInput.transform.Find("Placeholder").GetComponent<Text>().text = nameOfSave;

    }
    public void ChangeNameOfSave(string name)
    {
        nameOfSave = name;
    }
    public void SaveGame()
    {
        if (nameOfSave == "")
        {
            nameOfSave = SaveNameInput.transform.Find("Placeholder").GetComponent<Text>().text;
        }
        string path = SaveLoadControler.GetSavesPath() + nameOfSave;

        //Debug.Log(pathOfOldSave + " " + path);
        if(pathOfOldSave != "" && path != pathOfOldSave)
        {
            Debug.Log("Neet do delete save (" + pathOfOldSave + ") because it will be replaced by (" + path+")");
            SaveLoadControler.DeleteSave(pathOfOldSave);
        }

        SaveLoadControler.SaveWorld(nameOfSave, SaveLoadControler.GetSavesPath());
        ConfirmSavePanel.SetActive(false);

        OpenSavePanel();
    }
    public void ClickDeleteInSaveSavePanel()
    {
        int index = EventSystem.current.currentSelectedGameObject.transform.parent.GetSiblingIndex();
        index--;
        if (index < 0 || index >= savesPaths.Length) { Debug.Log("ERROR! path not found"); return; }
        pathOfSaveToDelete = savesPaths[index];
        ConfirmDeleteInSavePanel.SetActive(true);
    }

    //Load
    public void LoadSave()
    {
        int index = EventSystem.current.currentSelectedGameObject.transform.GetSiblingIndex();
        index--;
        if (index < 0 || index >= savesPaths.Length) { Debug.Log("ERROR! path not found"); return; }
        string path = savesPaths[index];
        if (SaveLoadControler.IsSavesExist(path))
        {
            WorldData worldData = SaveLoadControler.GetWorldData(path);

            SceneLoader.instance.gameMode = GameState.Level;
            SceneLoader.instance.gameLoadingMode = SceneLoader.GameLoadingMode.LoadFromWorldData;
            SceneLoader.instance.worldData = worldData;
            SceneLoader.instance.LoadWorldScene();
        }
    }
    public void OpenLoadPanel()
    {
        CloseNowOpenPanel();
        nowOpenPanle = LoadPanel;
        LoadPanel.gameObject.SetActive(true);

        savesPaths = SaveLoadControler.GetAllSaves();
        for (int i = 0; i < savesPaths.Length; i++)
        LoadButton.SetActive(false);
        ConfirmDeleteInLoadPanel.SetActive(false);
        for (int i = 1; i < LoadButtonsParent.childCount; i++) { Destroy(LoadButtonsParent.GetChild(i).gameObject); }
        for (int i = 0; i < savesPaths.Length; i++)
        {
            WorldData Data = SaveLoadControler.GetWorldData(savesPaths[i]);
            if (Data == null) { continue; }
            if (Data.General == null) { continue; }

            string name = SaveLoadControler.GetNameFromPath(savesPaths[i]);
            GameObject newButton = Instantiate(LoadButton);
            newButton.transform.SetParent(LoadButtonsParent, false);
            newButton.name = name;
            newButton.SetActive(true);

            string typeText = "Sandbox time: ";
            int timeInt = (int)Data.General.worldTime;
            int s = timeInt % 60;
            int m = ((timeInt - s) / 60) % 60;
            int h = (timeInt - m * 60 - s) / 3600;
            string ss = s.ToString(); if (s < 10) { ss = "0" + ss; }
            string sm = m.ToString(); if (m < 10) { sm = "0" + sm; }
            string sh = h.ToString();
            newButton.transform.GetChild(0).GetComponent<Text>().text = name;
            newButton.transform.GetChild(1).GetComponent<Text>().text = Data.General.date;
            newButton.transform.GetChild(2).GetComponent<Text>().text = typeText + string.Format("{0}:{1}:{2}", sh, sm, ss);
        }
    }
    public void ClickDeleteInSaveLoadPanel()
    {
        int index = EventSystem.current.currentSelectedGameObject.transform.parent.GetSiblingIndex();
        index--;
        if (index < 0 || index >= savesPaths.Length) { Debug.Log("ERROR! path not found"); return; }
        pathOfSaveToDelete = savesPaths[index];
        ConfirmDeleteInLoadPanel.SetActive(true);
    }

    //delete
    public void ClickYesInDeletePanel()
    {
        Debug.Log("delete file " + pathOfSaveToDelete);
        SaveLoadControler.DeleteSave(pathOfSaveToDelete);
        if (ConfirmDeleteInLoadPanel != null) { ConfirmDeleteInLoadPanel.SetActive(false); }
        if (ConfirmDeleteInSavePanel != null) { ConfirmDeleteInSavePanel.SetActive(false); }
        if (nowOpenPanle == SavePanel) { OpenSavePanel(); }
        else if(nowOpenPanle == LoadPanel) { OpenLoadPanel(); }
    }
    public void ClickNoInDeletePanel()
    {
        pathOfSaveToDelete = "";
        if (ConfirmDeleteInLoadPanel != null) { ConfirmDeleteInLoadPanel.SetActive(false); }
        if (ConfirmDeleteInSavePanel != null) { ConfirmDeleteInSavePanel.SetActive(false); }
    }

    //dif
    private int ScaleFTI(float value)
    {
        int trueValue;
        if (value <= 0.10f)
        {
            value /= 0.10f;
            trueValue = (int)(value * 5f) + 5;
        }
        else if (value <= 0.60f)
        {
            value -= 0.10f;
            value /= 0.50f;
            trueValue = (int)(value * 90f) + 10;
        }
        else
        {
            value -= 0.60f;
            value /= 0.399f;
            trueValue = (int)(value * 201f) + 99;
        }
        return trueValue;
    }
}
