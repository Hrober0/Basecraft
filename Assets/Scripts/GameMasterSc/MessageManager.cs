using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MessageManager : MonoBehaviour
{
    [Header("Left messages")]
    [SerializeField] private Transform LMGroup = null;
    [SerializeField] private GameObject LMSingleMessage = null;
    [SerializeField] private Sprite InfoIcon = null;
    [SerializeField] private Sprite ErrorIcon = null;
    [SerializeField] private Sprite TaskIcon = null;
    [SerializeField] private Color NormalBordercolor = Color.white;
    [SerializeField] private Color ActiveBordercolor = Color.yellow;
    private readonly List<LMessage> mUsingList = new List<LMessage>();
    private readonly List<GameObject> mUnusedList = new List<GameObject>();
    private readonly float mSpacing = 6f;
    private readonly float messShowinTime = 20f;
    private readonly float messOpenTime = 0.5f;
    private float messYOffset = 0;
    private bool isOpening = false;
    private readonly int defNumOfMaxVisableMess = 8;
    [SerializeField]private int numOfMaxVisableMess = 0;
    private bool isVisable = true;
    private int numOfHidingMess = 0;
    private readonly List<Image> highlightBorderList = new List<Image>();

    enum MesType { None, Error, Warning, Message, Task };

    private class LMessage
    {
        public GameObject gameObject;

        public GameEventControler.GameEvent eventNum;
        public bool wasOpen = false;

        public bool canHide = true;
        public float timeToClose = -1f;

        public LMessage(GameObject gameObject, GameEventControler.GameEvent eventNum)
        {
            this.gameObject = gameObject;
            this.eventNum = eventNum;
        }
    }

    public static MessageManager instance;
    private void Awake()
    {
        if (instance != null) { return; }
        instance = this;
        LMSingleMessage.SetActive(false);
    }
    private void Update()
    {
        /*
        if (!isOpening && !isVisable && LeftPanel.instance.nowOpenPanel == null)
        {
            SetVisibilityMess(true);
        }
        */

        if (numOfMaxVisableMess != defNumOfMaxVisableMess && LeftPanel.instance.nowOpenPanel == null)
        {
            SetMaxNumOfMess(defNumOfMaxVisableMess);
        }

        int l = mUsingList.Count;
        int u = 0;
        for (int i = 0; i < l; i++)
        {
            LMessage message = mUsingList[i-u];
            if (!message.canHide) continue;

            message.timeToClose -= Time.unscaledDeltaTime;
            if (message.timeToClose < 0)
            {
                HideMes(message.gameObject);
                u++;
            }
        }
    }

    public void ShowMessage(Messages mes, int v1 = -1)
    {
        Debug.Log("<b>Message</b> " + mes);

        MesType mesType = MesType.None;
        string textS = GetTextS();
        ShowLeftMessage(mesType, textS, GameEventControler.GameEvent.None, true);

        string GetTextS()
        {
            switch (mes)
            {
                case Messages.CantBuildItHere:                mesType = MesType.Error;    return "You can't build it here. You should choose an empty place!";
                case Messages.CantConnectThisObj:             mesType = MesType.Error;    return "Can't connect this object!";
                case Messages.CantBuildConnectionThroughObj:  mesType = MesType.Error;    return "Can't build connection through objects!";
                case Messages.DoesntSelectConToBuild:         mesType = MesType.Error;    return "Does not select connection to build type!";
                case Messages.ConnectionIsAlreadyBuilt:       mesType = MesType.Error;    return "Connection is already built!";

                case Messages.CantDemolitionObj:              mesType = MesType.Error;    return "You can't demolition this object!";
                case Messages.CantDisasembleObj:              mesType = MesType.Error;    return "You can't disasemble this object!";
                case Messages.CantCutObj:                     mesType = MesType.Error;    return "You can't cut it!";
                case Messages.CantPlantHere:                  mesType = MesType.Error;    return "You can't plant it here!";
                case Messages.CantMineObj:                    mesType = MesType.Error;    return "You can't mine it!";

                case Messages.CantStartHere:                  mesType = MesType.Error;    return "You can't start here. You should choose an empty place!";

                case Messages.YouAreUnderAttack:             mesType = MesType.Warning;  return "You are under attack!";
                case Messages.AttackIsComing:                mesType = MesType.Warning;  return "You will be attacked in a moment!";

                case Messages.NoDronStation:                 mesType = MesType.Warning;  return "There are no drone stations available. Our drones can't function!";
                case Messages.NoTransmisonTower:             mesType = MesType.Message;  return "U can't open electricity panel!";
            }
            return "";
        }
    }
    public void ShowTask(string textS, GameEventControler.GameEvent gameEvent)
    {
        ShowLeftMessage(MesType.Task, textS, gameEvent, false);
    }

    public void ClickTask(GameEventControler.GameEvent gameEvent)
    {
        if (gameEvent == GameEventControler.GameEvent.None) { return; }
        foreach (LMessage item in mUsingList)
        { if (item.eventNum == gameEvent) { item.wasOpen = true; break; } }
        TaskManager.instance.OpenTask(gameEvent);
    }

    public void CloseTask(GameEventControler.GameEvent gameEvent)
    {
        if (gameEvent == GameEventControler.GameEvent.None) { return; }
        for (int i = 0; i < mUsingList.Count; i++)
        {
            if (mUsingList[i].eventNum == gameEvent)
            {
                HideMes(mUsingList[i].gameObject);
                return;
            }
        }
    }
    public void CloseAllMess()
    {
        int l = mUsingList.Count;
        for (int i = 0; i < l; i++)
        {
            HideMes(mUsingList[0].gameObject);
        }
    }
    public void SetVisibilityMess(bool value, bool forceHide=false)
    {
        if (isOpening) { return; }
        isOpening = true;
        isVisable = value;

        RectTransform mRT = LMSingleMessage.GetComponent<RectTransform>();
        float posX;
        float dealy = 0f;
        if (value)
        {
            posX = 0f;
            for (int i = 0; i < mUsingList.Count; i++)
            {
                RectTransform rt = mUsingList[i].gameObject.GetComponent<RectTransform>();
                rt.DOAnchorPosX(posX, messOpenTime / 2).SetDelay(dealy).SetUpdate(true);
                dealy += 0.02f;
            }
        }
        else
        {
            posX = -mRT.sizeDelta.x-5f;
            for (int i = mUsingList.Count - 1; i >= 0; i--)
            {
                if (!forceHide && !mUsingList[i].canHide) continue;
                RectTransform rt = mUsingList[i].gameObject.GetComponent<RectTransform>();
                rt.DOAnchorPosX(posX, messOpenTime / 2).SetDelay(dealy).SetUpdate(true);
                dealy += 0.02f;
            }
        }
        mRT.DOAnchorPosX(posX, messOpenTime / 2).SetDelay(dealy).SetUpdate(true).OnComplete(() => { isOpening = false; });
    }
    public void SetMaxNumOfMess(int num)
    {
        numOfMaxVisableMess = num;

        int l = mUsingList.Count;
        int toHide = l - numOfMaxVisableMess;
        int hidden = 0;
        for (int i = 0; i < l; i++)
        {
            if (hidden >= toHide) break;

            int index = i - hidden;

            if (!mUsingList[index].canHide) continue;

            HideMes(mUsingList[index].gameObject);
            hidden++;
        }
    }
    public void SetYMessOffset(float offset)
    {
        if (messYOffset == offset) return;

        messYOffset = offset;
        SetLMPosY();
    }

    public void OffHighlightBorderOfTask()
    {
        int l = highlightBorderList.Count;
        for (int i = 0; i < l; i++)
        {
            HighlightBorder(highlightBorderList[0], false);
        }
    }
    public void SetHighlightBorderOfTask(GameEventControler.GameEvent gameEvent)
    {
        if (gameEvent == GameEventControler.GameEvent.None) { return; }
        for (int i = 0; i < mUsingList.Count; i++)
        {
            if (mUsingList[i].eventNum == gameEvent)
            {
                Image image = mUsingList[i].gameObject.transform.Find("Border").GetComponent<Image>();
                HighlightBorder(image, true);
                return;
            }
        }
    }

    private void ShowLeftMessage(MesType mesType, string textS, GameEventControler.GameEvent gameEvent, bool canHide)
    {
        if (textS == "") { return; }

        //hide old mes
        if (mUsingList.Count >= numOfMaxVisableMess)
        {
            foreach (LMessage item in mUsingList)
            {
                if (item.canHide) { HideMes(item.gameObject); break; }
            }
        }
        
        GameObject usingM;
        if (mUnusedList.Count == 0) { usingM = CreateNewM(); }
        else { usingM = mUnusedList[0]; mUnusedList.RemoveAt(0); }

        //set position
        int mesIndexInList = 0;
        if (canHide) { mesIndexInList = mUsingList.Count; }
        else { foreach (LMessage item in mUsingList) { if (!item.canHide) mesIndexInList++; } }
        int visableIndex = mesIndexInList + numOfHidingMess;
        RectTransform rt = usingM.GetComponent<RectTransform>();
        float yPos = -mSpacing - messYOffset - visableIndex * (mSpacing + rt.sizeDelta.y);
        rt.anchoredPosition = new Vector2(-rt.sizeDelta.x, yPos);
        usingM.transform.SetSiblingIndex(visableIndex + 1);
        if (isVisable ||  mesType==MesType.Task) { rt.DOAnchorPosX(0f, messOpenTime / 2).SetUpdate(true); }

        usingM.SetActive(true);

        //sprite & text
        Sprite sprite = null;
        switch (mesType)
        {
            case MesType.Error: sprite = ErrorIcon; break;
            case MesType.Warning: sprite = ErrorIcon; break;
            case MesType.Message: sprite = InfoIcon; break;
            case MesType.Task: sprite = TaskIcon; break;
        }
        usingM.GetComponentInChildren<Text>().text = textS;
        usingM.transform.Find("Image").GetComponent<Image>().sprite = sprite;
        usingM.name = mesType.ToString();

        LMessage message = new LMessage(usingM, gameEvent);
        message.canHide = canHide;

        Button button = usingM.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        mUsingList.Insert(mesIndexInList, message);

        switch (mesType)
        {
            case MesType.Task:
                button.interactable = true;
                button.onClick.AddListener(() => ClickTask(gameEvent));
                AnimBorder(message);
                break;
            default:
                button.interactable = false;
                message.timeToClose = messShowinTime;
                break;
        }

        SetLMPosY();

        GameObject CreateNewM()
        {
            GameObject mess = Instantiate(LMSingleMessage);
            mess.name = "LeftMessage" + LMGroup.childCount.ToString();
            mess.transform.SetParent(LMGroup, false);
            return mess;
        }
    }
    private void AnimBorder(LMessage message)
    {
        float zt = 0.3f;
        float wt = 0.6f;
        Image image = message.gameObject.transform.Find("Border").GetComponent<Image>();
        DoSquence();

        void DoSquence()
        {
            if (!message.gameObject.activeSelf || message.wasOpen)
            {
                if (highlightBorderList.Contains(image)) image.color = ActiveBordercolor;
                return;
            }
            DOTween.Sequence()
            .SetUpdate(true)
            .Append(image.DOColor(ActiveBordercolor, zt))
            .AppendInterval(wt)
            .OnComplete(Dopart2)
            ;
        }
        void Dopart2()
        {
            if (highlightBorderList.Contains(image)) { image.color = ActiveBordercolor; return; }
            DOTween.Sequence()
            .Append(image.DOColor(NormalBordercolor, zt))
            .AppendInterval(wt)
            .OnComplete(DoSquence)
            ;
        }
    }
    private void HighlightBorder(Image image, bool active)
    {
        if (active)
        {
            image.color = ActiveBordercolor;
            highlightBorderList.Add(image);
        }
        else
        {
            highlightBorderList.Remove(image);
            image.color = NormalBordercolor;
        }
    }
    private void HideMes(GameObject messGO)
    {
        for (int i = 0; i < mUsingList.Count; i++)
        {
            if (mUsingList[i].gameObject == messGO) { mUsingList.RemoveAt(i); break; }
        }
        messGO.name = "hiding";
        numOfHidingMess++;

        messGO.GetComponentInChildren<Button>().onClick.RemoveAllListeners();

        RectTransform objectRT = messGO.GetComponent<RectTransform>();
        if (objectRT.anchoredPosition.x < 10f - objectRT.sizeDelta.x) EndIt();
        else
        {
            objectRT.DOAnchorPosX(-objectRT.sizeDelta.x, messOpenTime).SetUpdate(true).OnComplete(EndIt);
        }

        void EndIt()
        {
            messGO.name = "usused";
            numOfHidingMess--;
            messGO.SetActive(false);
            messGO.transform.SetAsLastSibling();
            mUnusedList.Add(messGO);
            SetLMPosY();
        }
    }
    private void SetLMPosY()
    {
        float yPos = -mSpacing - messYOffset;
        foreach (Transform item in LMGroup)
        {
            if (!item.gameObject.activeSelf) { continue; }

            RectTransform rt = item.GetComponent<RectTransform>();
            rt.DOAnchorPosY(yPos, 0.2f).SetUpdate(true);

            yPos -= mSpacing + rt.sizeDelta.y;
        }
    }
}