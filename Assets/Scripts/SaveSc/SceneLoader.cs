using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    public enum GameLoadingMode { CreateNew, LoadFromWorldData, }
    public GameLoadingMode gameLoadingMode = GameLoadingMode.LoadFromWorldData;

    public GameState gameMode = GameState.SpaceStation;

    [Header("Loading screan")]
    [SerializeField] private GameObject LoadingScreen = null;
    [SerializeField] private ProgressBar bar = null;
    [SerializeField] private CanvasGroup canvasGroup = null;
    [SerializeField] private Text barText = null;  

    [Header("world settings")]
    public int numberOfStartItems = 2;
    public GeneralWorldData generalWorldData;

    [Header("Load world data")]
    public WorldData worldData = null;
    public bool canChooseStartPlace = false;

    private void Awake()
    {
        if (instance != null) { Debug.Log("More then one " + this + " on scen, return."); return; }

        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadingScreen.SetActive(false);
        SetDefaultGeneralWorldData();
    }

    private string sceneToLoadName;
     
    [System.Serializable]
    private enum animState { None, Showing, Hiding }
    [SerializeField]
    animState animationState = animState.None;
    public float transisiontTime = 0.5f;

    private void Update()
    {
        switch (animationState)
        {
            case animState.None: return;
            case animState.Showing:
                if (canvasGroup.alpha == 1)
                {
                    animationState = animState.None;
                    StartCoroutine(LoadScene());
                    return;
                }
                canvasGroup.alpha += Time.unscaledDeltaTime / transisiontTime;
                return;
            case animState.Hiding:
                if (canvasGroup.alpha == 0)
                {
                    animationState = animState.None;
                    LoadingScreen.gameObject.SetActive(false);
                    return;
                }
                if (Time.unscaledDeltaTime > 0.5f) { return; }
                canvasGroup.alpha -= Time.unscaledDeltaTime / transisiontTime;
                return;
        }
    }

    private IEnumerator LoadScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoadName);

        while (!operation.isDone)
        {
            float percent = Mathf.Clamp01(operation.progress / 0.9f)  * 100.00f;
            bar.curr = (int)percent;

            yield return null;
        }
        animationState = animState.Hiding;
    }
    
    private void StartLoading()
    {
        LoadingScreen.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        bar.curr = 0;
        animationState = animState.Showing;
        SetPostscript("");
    }

    public void LoadWorldScene()
    {
        sceneToLoadName = "MainGameScene"; 
        StartLoading();
    }
    public void LoadMainMenuScene()
    {
        sceneToLoadName = "MainMenu";
        StartLoading();
    }
    public void LoadSpaceStationScene()
    {
        sceneToLoadName = "SpaceStation";
        StartLoading();
    }
    public void LoadGMLoaderScene()
    {
        sceneToLoadName = "GMLoaderScene";
        StartLoading();
    }

    public void SetDefaultGeneralWorldData()
    {
        //Debug.Log("Set default GeneralWorldData");

        string actDate = System.DateTime.Now.ToString("hh:mm") + " " + System.DateTime.Now.ToString("MM/dd/yyyy");
        generalWorldData = new GeneralWorldData
        (
        "none",
        SettingsManager.instance.gameVersion,
        GameState.Sandbox,
        actDate,
        "none",
        80,
        80,
        0,
        0f,
        Difficulty.Normal
        );
    }
    public void SetPostscript(string ps)
    {
        if (ps == "") { barText.text = "Loading..."; }
        else { barText.text = "Loading... (" + ps + ")"; }
    }
}
