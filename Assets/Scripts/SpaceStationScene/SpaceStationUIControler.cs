using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class SpaceStationUIControler : MonoBehaviour
{
    [Header("Title")]
    //[SerializeField] private RectTransform TitleRT = null;
    [SerializeField] private Text VersionText = null;
    //private Vector3 defaultTitlePos;
    //private Vector3 enlargedTitlePos;

    [Header("Planet")]
    public float approachTime = 0.5f;
    [SerializeField] private Material planetMat = null;
    [SerializeField] private float scanSpeed = 0.5f;
    private float scanPosition = 1f;
    private bool scaning = false;

    [Header("Camera")]
    public Camera mainCamera;
    public Camera blurCamera;
    [SerializeField] private Vector3 cameraDefPos = new Vector3(0f, 0f, -10f);
    [SerializeField] private float cameraDefSize = 10f;
    [SerializeField] private float cameraShortSize = 10f;
    [SerializeField] private Vector2 cameraShift = new Vector2(-10f, 0f);

    [Header("About Panel")]
    [SerializeField] private float openAboutPanelTime = 0.3f;
    [SerializeField] private GameObject aboutButton = null;
    [SerializeField] private Transform aboutPanel = null;
    private CanvasGroup aboutPanelCG;

    [Header("Colony")]
    [SerializeField] private Transform colonyParent = null;
    [SerializeField] private Sprite pointerImage = null;
    [SerializeField] private Sprite colonyImage = null;
    public Color colonyDeselectColor;
    public Color colonySelectColor;
    public Color colonyDeselectOutlineColor;
    public Color colonySelectOutlineColor;
    [SerializeField] private GameObject colonySmallText = null;
    [SerializeField] private GameObject colonyBigText = null;
    private Image selectColonyImage;

    [Header("Colony panel")]
    [SerializeField] private Transform ColonyPanel = null;
    private CanvasGroup ColonyPanelCG;
    [SerializeField] private Text CPNameText = null;
    [SerializeField] private Text CPDescriptionText = null;
    [SerializeField] private Text CPStatusText = null;
    [SerializeField] private Text CPDifficultyText = null;
    [SerializeField] private Transform ResParent = null;
    [SerializeField] private GameObject ResImage = null;
    [SerializeField] private GameObject Button = null;
    [SerializeField] private Text ButtonText = null;
    [SerializeField] private Text CooldownText = null;
    private ColonyNames selectColony;
    private bool isOccupied;
    private float updateTimeUI = 1f;
    private float timeToUpdateUI = 1f;

    public static SpaceStationUIControler instance;
    private void Awake()
    {
        if (instance != null) { return; }
        instance = this;

        //defaultTitlePos = TitleRT.anchoredPosition;
        //enlargedTitlePos = defaultTitlePos; enlargedTitlePos.y = 500f;
    }

    private void Start()
    {
        SceneLoader.instance.gameMode = GameState.SpaceStation;
        LeftPanel.instance.SetCloseButton();

        ColonyPanel.localScale = new Vector2(0f, 0f);
        ColonyPanel.gameObject.SetActive(true);
        ColonyPanelCG = ColonyPanel.GetComponent<CanvasGroup>();
        ColonyPanelCG.alpha = 0;

        aboutPanel.localScale = new Vector2(0f, 0f);
        aboutPanel.gameObject.SetActive(true);
        aboutButton.SetActive(true);
        aboutPanelCG = aboutPanel.GetComponent<CanvasGroup>();
        aboutPanelCG.alpha = 0;

        Vector2 pos = new Vector2(0, 10);
        foreach (Transform trans in colonyParent)
        {
            Transform smallT = trans.Find("SmallText");
            if (smallT == null) { smallT = Instantiate(colonySmallText).transform; smallT.SetParent(trans, false); smallT.name = "SmallText"; }
            smallT.GetComponent<RectTransform>().anchoredPosition = pos;
            smallT.GetComponent<Text>().text = trans.name;

            Transform bigT = trans.Find("BigText");
            if (bigT == null) { bigT = Instantiate(colonyBigText).transform; bigT.SetParent(trans, false); bigT.name = "BigText"; }
            bigT.GetComponent<RectTransform>().anchoredPosition = pos;
            bigT.GetComponent<Text>().text = trans.name;
        }

        VersionText.text = "version " + SettingsManager.instance.gameVersion;

        mainCamera.orthographicSize = cameraDefSize;
        blurCamera.orthographicSize = cameraDefSize;

        UpdateColonyPointers();
    }

    private void Update()
    {
        if (ColonyPanel.gameObject.activeSelf == false) { return; }

        timeToUpdateUI -= Time.unscaledDeltaTime;
        if (timeToUpdateUI <= 0)
        {
            timeToUpdateUI = updateTimeUI;

            if (SpaceBaseMainSc.instance.coloniesToReset.Contains(selectColony) == false)
            {
                Button.SetActive(true);
                CooldownText.gameObject.SetActive(false);
                return;
            }
            DateTime arrival = SpaceBaseMainSc.instance.GetDataTimeOfColonyToReset(selectColony);
            TimeSpan travelTime = arrival - System.DateTime.Now;
            int seconds = (int)travelTime.TotalSeconds + 1;
            string text = "The colony will be reinhabitable\nin ";
            if (seconds <= 60) { text += seconds + " seconds"; }
            else { int minuts = seconds / 60; text += minuts + " minuts"; }
            CooldownText.text = text;
        }

        if (scaning)
        {
            planetMat.SetFloat("_Position", scanPosition);
            scanPosition -= Time.unscaledDeltaTime * scanSpeed;
            if (scanPosition <= -1f)
            {
                scanPosition = -1f;
                scaning = false;
                UpdateColonyPointers();
            }
        }
    }

    public void ScanPlanet()
    {
        scanPosition = 1f;
        scaning = true;
    }

    public void UpdateColonyPointers()
    {
        int numOfColonies = Enum.GetNames(typeof(ColonyNames)).Length;
        for (int i = 0; i < numOfColonies; i++)
        {
            ColonyNames colonyN = (ColonyNames)i;
            Transform colonyT = colonyParent.Find(colonyN.ToString());
            if (colonyT == null) { Debug.Log("Dont found " + colonyN + " pointer"); return; }
            GeneralWorldData generalWorldData = SpaceBaseMainSc.instance.GetWorldData(colonyN);
            if (generalWorldData == null || SpaceBaseMainSc.instance.availableColonies.Contains(colonyN) == false)
            { colonyT.gameObject.SetActive(false); }
            else
            {
                colonyT.gameObject.SetActive(true);
                if (SpaceBaseMainSc.instance.unlockedColonies.Contains(colonyN))
                { colonyT.GetComponent<Image>().sprite = colonyImage; }
                else
                { colonyT.GetComponent<Image>().sprite = pointerImage; }

                if (SpaceBaseMainSc.instance.newColoniesToScan.Contains(colonyN))
                {
                    SpaceBaseMainSc.instance.newColoniesToScan.Remove(colonyN);
                    colonyT.localScale = new Vector3(0, 0, 0);
                    colonyT.DOScale(new Vector3(1, 1, 1), 1f).SetEase(Ease.OutBounce).From(2f);
                }
            }
        }
    }

    public void DeselectColony()
    {
        if (selectColonyImage == null) { return; }

        ColonyPanel.DOScale(0f, approachTime).SetEase(Ease.OutQuad).SetUpdate(true);
        ColonyPanelCG.DOFade(0, approachTime).SetEase(Ease.OutQuad).SetUpdate(true);

        mainCamera.DOOrthoSize(cameraDefSize, approachTime).SetUpdate(true);
        mainCamera.transform.DOLocalMove(cameraDefPos, approachTime).SetUpdate(true);
        blurCamera.DOOrthoSize(cameraDefSize, approachTime).SetUpdate(true);

        //TitleRT.DOAnchorPos(defaultTitlePos, approachTime).SetUpdate(true);

        selectColonyImage.color = colonyDeselectColor;
        Transform textTrans = selectColonyImage.transform.Find("BigText");
        textTrans.GetComponent<Text>().color = colonyDeselectColor;
        textTrans.GetComponent<Outline>().effectColor = colonyDeselectOutlineColor;

        selectColonyImage = null;

        foreach (Transform trans in colonyParent)
        {
            trans.Find("SmallText").gameObject.SetActive(true);
            trans.Find("BigText").gameObject.SetActive(false);
        }
    }
    public void ClickColonyPointerButton()
    {
        GameObject clickedGO = EventSystem.current.currentSelectedGameObject;
        string name = clickedGO.name;

        DeselectColony();

        selectColonyImage = clickedGO.GetComponent<Image>();
        selectColonyImage.color = colonySelectColor;
        Transform textTrans = clickedGO.transform.Find("BigText");
        textTrans.GetComponent<Text>().color = colonySelectColor;
        textTrans.GetComponent<Outline>().effectColor = colonySelectOutlineColor;

        Vector2 colonyPos = clickedGO.GetComponent<RectTransform>().anchoredPosition;
        Vector3 cameraPos = colonyPos * 0.02f + cameraShift;
        cameraPos += cameraDefPos;
        mainCamera.DOOrthoSize(cameraShortSize, approachTime).SetUpdate(true);
        mainCamera.transform.DOLocalMove(cameraPos, approachTime).SetUpdate(true);
        blurCamera.DOOrthoSize(cameraShortSize, approachTime).SetUpdate(true);

        //TitleRT.DOAnchorPos(enlargedTitlePos, approachTime).SetUpdate(true);

        foreach (Transform trans in colonyParent)
        {
            trans.Find("SmallText").gameObject.SetActive(false);
            trans.Find("BigText").gameObject.SetActive(true);
        }

        if (Enum.TryParse(name, out ColonyNames colony)) { OpenColonyPanel(colony); };
    }
    public void ClickColonyVisitButton()
    {
        LeftPanel.instance.CloseNowOpenPanel();
        WorldData worldData = null;
        string colonyName = selectColony.ToString();
        if (isOccupied)
        {
            worldData = SaveLoadControler.GetWorldData(SaveLoadControler.GetColoniesPath() + colonyName);
            if (worldData == null) { Debug.Log("Dont found saved colony, so set default"); }
        }
        else if(SpaceBaseMainSc.instance.unlockedColonies.Contains(selectColony)==false)
        {
            SpaceBaseMainSc.instance.unlockedColonies.Add(selectColony);
            SpaceBaseMainSc.instance.SaveGeneralData();
        }
        if (worldData == null)
        {
            worldData = SaveLoadControler.GetWorldDataFromResources(colonyName);
        }
        if (worldData == null)
        {
            Debug.Log("ERROR! Dont found colony!");
            return;
        }

        SpaceBaseMainSc.instance.LoadColony(worldData);
    }

    private void OpenColonyPanel(ColonyNames colony)
    {

        ColonyPanel.DOScale(1f, approachTime).SetEase(Ease.InQuad).SetUpdate(true);
        ColonyPanelCG.DOFade(1f, approachTime).SetEase(Ease.InQuad).SetUpdate(true);

        CPNameText.text = colony.ToString();

        selectColony = colony;

        if (SpaceBaseMainSc.instance.unlockedColonies.Contains(colony))
        {
            isOccupied = true;
            CPStatusText.text = "Occupied";
            ButtonText.text = "Visit";
        }
        else
        {
            isOccupied = false;
            CPStatusText.text = "Unoccupied";
            ButtonText.text = "Colonize ";
        }

        if (SpaceBaseMainSc.instance.coloniesToReset.Contains(colony))
        {
            Button.SetActive(false);
            CooldownText.gameObject.SetActive(true);
        }
        else
        {
            Button.SetActive(true);
            CooldownText.gameObject.SetActive(false);
        }

        List<GameObject> detectstRess = new List<GameObject>();
        foreach (Transform trans in ResParent) { detectstRess.Add(trans.gameObject); trans.gameObject.SetActive(false); }

        GeneralWorldData generalWorldData = SpaceBaseMainSc.instance.GetWorldData(colony);
        if (generalWorldData == null)
        {
            CPDifficultyText.text = "???";
            ButtonText.text = "just no";
        }
        else
        {
            CPDifficultyText.text = generalWorldData.difficulty.ToString();

            for (int i = 0; i < generalWorldData.detectedRes.Length; i++)
            {
                if(i >= detectstRess.Count) { AddToListNewRes(); }
                detectstRess[i].GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(generalWorldData.detectedRes[i]);
                detectstRess[i].GetComponentInChildren<Text>().text = GetNameOfDetectedRes(generalWorldData.detectedRes[i]);
                detectstRess[i].SetActive(true);
            }
        }

        string colonyDes = Language.GetText("ColonyDes." + colony);
        if (colonyDes == "null") { colonyDes = "???"; Debug.Log("ERROR!" + colony + " have no set description!"); }
        CPDescriptionText.text = colonyDes;

        void AddToListNewRes()
        {
            GameObject resGO = Instantiate(ResImage);
            resGO.transform.SetParent(ResParent, false);
            resGO.name = "Res";
            detectstRess.Add(resGO);
        }
        string GetNameOfDetectedRes(Res res)
        {
            switch (res)
            {
                case Res.Wood: return "Wood";
                case Res.StoneOre: return "Stone";
                case Res.CopperOre: return "Copper";
                case Res.IronOre: return "Iron";
                case Res.Coal: return "Coal";
                case Res.BottleWater: return "Water";
                case Res.BottleOil: return "Oil";
                case Res.Sand: return "Sand";
                default: return "<" + res + ">";
            }
        }
    }

    //about planel
    public void OpenAboutPanel()
    {
        aboutButton.SetActive(false);
        aboutPanel.gameObject.SetActive(true);
        aboutPanel.DOScale(1f, openAboutPanelTime).SetEase(Ease.OutQuad).SetUpdate(true);
        aboutPanelCG.DOFade(1f, openAboutPanelTime).SetEase(Ease.OutQuad).SetUpdate(true);
    }
    public void CloseAboutPanel()
    {
        aboutPanel.DOScale(0f, openAboutPanelTime).SetEase(Ease.OutQuad).SetUpdate(true);
        aboutPanelCG.DOFade(0f, openAboutPanelTime).SetEase(Ease.OutQuad).SetUpdate(true)
        .OnComplete(()=>
        {
            aboutButton.SetActive(true);
            aboutPanel.gameObject.SetActive(false);
        });
    }
}