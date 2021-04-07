using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.UI;

public class TaskManager : MonoBehaviour
{
    public static TaskManager instance;

    [Header("Task objects")]
    [SerializeField] private GameObject taskPanelGO = null;
    [SerializeField] private RectTransform group = null;
    [SerializeField] private Text nameText = null;
    [SerializeField] private Text mainText = null;
    [SerializeField] private Text subTasksText = null;
    [SerializeField] private Button skipButton = null;
    [SerializeField] private Button callEventButton = null;

    [Header("Stats")]
    [SerializeField] private int[] actNumberOfItems;
    [SerializeField] private int[] totalNumberOfProducedItems;
    [SerializeField] private List<Obj> objectsTypes;
    [SerializeField] private List<int> actNumberOfBuildings;
    [SerializeField] private List<int> totalNumberOfBuildings;

    [SerializeField] private int[] numberOfLaunchedItems;

    private readonly float taskUpdateDelay = 1f;
    private float timeToUpdateTask = 0f;
    private List<MainTask> activeTasks = new List<MainTask>();
    private MainTask nowOpenTask = null;

    private bool isHidingPanel = false;
    private readonly float hidingTime = 2f;

    public enum SubTaskTypes
    {
        HoldItemsInColony=1, ProduceItems=2, HoldItemsInObjects=3,
        ObjectTypes=6, HaveStructures=7, BuildStructures=8,
        KillEnemyUnits=11, DestroyEnemyBase=12, SurviveWavesOfEnemies=13,
        LaunchedItems=16, HoldItemsInOrbitalStation=17, HaveTechnology=18, UnlockColony = 19,
    }
    public class MainTask
    {
        public bool wasInit = false;
        public GameEventControler.GameEvent eventNum = GameEventControler.GameEvent.None;
        public string mainText = "";

        public List<SubTask> subTasks = new List<SubTask>();

        public string skipButtonText = "";

        public GameEventControler.GameEvent gameEventToCall = GameEventControler.GameEvent.None;
        public string callEventButtonText = "";

        public bool callNextTask = false;

        public MainTask Init(GameEventControler.GameEvent eventNum, string mainText, bool callNextTask=true)
        {
            this.eventNum = eventNum;
            this.mainText = mainText;
            this.callNextTask = callNextTask;
            wasInit = true;
            return this;
        }
        public MainTask InitDef(GameEventControler.GameEvent eventNum, bool callNextTask=true)
        {
            string text = Language.GetText("TaskText." + eventNum + "_m");
            if (text == "null")
            {
                Debug.LogWarning("Main task(" + eventNum + ") text was not set in language(" + Language.GetUsingLangeage() + ") file");
                text = "hmmmm....\nI was gonna say something but forgot what....\nI think you know what to do.";
            }
            Init(eventNum, text, callNextTask);
            return this;
        }

       	public MainTask AddSubTask_HoldItemsInColonies(Res res, int qua, bool haveSpecialText = false)  { subTasks.Add(new SubTask(SubTaskTypes.HoldItemsInColony, (int)res, qua, GetSubTaskText(haveSpecialText)));     return this; }
        public MainTask AddSubTask_ProduceItems(Res res, int qua, bool haveSpecialText = false)         { subTasks.Add(new SubTask(SubTaskTypes.ProduceItems, (int)res, qua, GetSubTaskText(haveSpecialText)));     return this; }
        public MainTask AddSubTask_HaveStructures(Obj obj, int qua, bool haveSpecialText = false)       { subTasks.Add(new SubTask(SubTaskTypes.HaveStructures, (int)obj, qua, GetSubTaskText(haveSpecialText)));   return this; }
        public MainTask AddSubTask_BuildStructures(Obj obj, int qua, bool haveSpecialText = false)      { subTasks.Add(new SubTask(SubTaskTypes.BuildStructures, (int)obj, qua, GetSubTaskText(haveSpecialText)));  return this; }
        public MainTask AddSubTask_HoldItemsInObjects(Obj obj, Res res, int qua, bool haveSpecialText = false) { subTasks.Add(new SubTask(SubTaskTypes.HoldItemsInObjects, (int)obj + (int)res * 1000000, qua, GetSubTaskText(haveSpecialText))); return this; }
        public MainTask AddSubTask_LaunchedItems(Res res, int qua, bool haveSpecialText = false)        { subTasks.Add(new SubTask(SubTaskTypes.LaunchedItems, (int)res, qua, GetSubTaskText(haveSpecialText)));    return this; }
        public MainTask AddSubTask_HoldItemsInOS(Res res, int qua, bool haveSpecialText = false)        { subTasks.Add(new SubTask(SubTaskTypes.HoldItemsInOrbitalStation, (int)res, qua, GetSubTaskText(haveSpecialText)));    return this; }
        public MainTask AddSubTask_HaveTechnology(Technologies tech, bool haveSpecialText = false)      { subTasks.Add(new SubTask(SubTaskTypes.HaveTechnology, (int)tech, 1, GetSubTaskText(haveSpecialText)));    return this; }
        public MainTask AddSubTask_UnlockColony(ColonyNames colony, bool haveSpecialText = false)       { subTasks.Add(new SubTask(SubTaskTypes.UnlockColony, (int)colony, 1, GetSubTaskText(haveSpecialText)));    return this; }

        public MainTask SetConfirmButton(string buttonName) { skipButtonText = buttonName; return this; }
        public MainTask SetDefConfirmButton()
        {
            if (wasInit == false) { Debug.Log("ERROR! Can set confirm button, beacuse event num is not set!"); return this; }

            string text = Language.GetText("TaskText." + eventNum + "_b");
            if (text == "null")
            {
                Debug.Log("ERROR! Main task(" + eventNum + ") button text was not set in language(" + Language.GetUsingLangeage() + ") file");
                text = "OK";
            }
            SetConfirmButton(text);

            return this;
        }

        public MainTask SetCallEventButton(GameEventControler.GameEvent gameEvent, string message)
        {
            gameEventToCall = gameEvent;
            callEventButtonText = message;
            return this;
        }
        public MainTask SetDefCallEventButton(GameEventControler.GameEvent gameEvent)
        {
            if (wasInit == false) { Debug.Log("ERROR! Can set call event button, beacuse event num is not set!"); return this; }

            string text = Language.GetText("TaskText." + eventNum + "_c");
            if (text == "null")
            {
                Debug.Log("ERROR! Main task(" + eventNum + ") button text was not set in language(" + Language.GetUsingLangeage() + ") file");
                text = "No";
            }

            SetCallEventButton(gameEvent, text);

            return this;
        }

        private string GetSubTaskText(bool haveSpecialText)
        {
            string message = "";
            if (haveSpecialText == false) return message;
            if (wasInit == false) { Debug.Log("ERROR! Can add sub task, beacuse event num is not set!"); return message; }

            int taskNum = subTasks.Count + 1;
            string key = "TaskText." + eventNum + "_t" + taskNum;
            message = Language.GetText(key);
            if (message == "null")
            {
                Debug.Log("ERROR! Sub task(kay:" + key + ") text was not set in language(" + Language.GetUsingLangeage() + ") file");
                message = "";
            }

            return message;
        }
    }
    public class SubTask
    {
        public SubTaskTypes taskType;
        public int subcategory;

        public int targetValue;
        public bool done;

        public string message;

        public SubTask(SubTaskTypes taskType, int subcategory, int targetValue, string message="")
        {
            this.taskType = taskType;
            this.subcategory = subcategory;
            this.targetValue = targetValue;
            this.message = message;
            done = false;
        }
    }

    private void Awake()
    {
        if (instance != null) return;
        instance = this;

        SetDefaultStats();
    }
    private void Update()
    {
        timeToUpdateTask -= Time.deltaTime;
        if (timeToUpdateTask <= 0)
        {
            timeToUpdateTask = taskUpdateDelay;
            UpdateTextOfOpenTask();
            CheckTasks();
        }
    }

    public void OpenTask(GameEventControler.GameEvent gameEvent)
    {
        LeftPanel.instance.OpenTaskPanel();

        MainTask mainTask = null;
        foreach (MainTask t in activeTasks) { if (t.eventNum == gameEvent) { mainTask = t; break; } }

        if (mainTask == null) { Debug.LogError("Cant show task, dont found it in list"); return; }

        string text = Language.GetText("TaskText." + mainTask.eventNum + "_h");
        if (text == "null") { text = "Just the next task"; }
        nameText.text = text;
        mainText.text = mainTask.mainText;
        subTasksText.text = GetSubTasksText(mainTask.subTasks);
        nowOpenTask = mainTask;

        Canvas.ForceUpdateCanvases();

        float height = 80;
        height += group.rect.size.y;

        skipButton.gameObject.SetActive(false);
        if (mainTask.skipButtonText != "")
        {
            height += 50f;
            skipButton.gameObject.SetActive(true);
            skipButton.GetComponentInChildren<Text>().text = mainTask.skipButtonText;
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(() => DoneTask(mainTask, false));
        }

        callEventButton.gameObject.SetActive(false);
        if (mainTask.callEventButtonText != "")
        {
            if (mainTask.skipButtonText == "") { height += 50f; }
            callEventButton.gameObject.SetActive(true);
            callEventButton.GetComponentInChildren<Text>().text = mainTask.callEventButtonText;
            callEventButton.onClick.RemoveAllListeners();
            callEventButton.onClick.AddListener(() => GameEventControler.StartEvent(mainTask.gameEventToCall));
        }

        RectTransform newTaskRT = taskPanelGO.GetComponent<RectTransform>();
        newTaskRT.sizeDelta = new Vector2(newTaskRT.sizeDelta.x, height);

        MessageManager.instance.SetYMessOffset(height + 20f);
        MessageManager.instance.OffHighlightBorderOfTask();
        MessageManager.instance.SetHighlightBorderOfTask(gameEvent);
    }
    public void StartTask(MainTask mainTask)
    {
        if (mainTask.eventNum == GameEventControler.GameEvent.None) return;
        string text = Language.GetText("TaskText." + mainTask.eventNum + "_h");
        if (text == "null") { text = "Just the next task"; }

        float hTime = 1f;
        if (isHidingPanel) hTime += hidingTime + 1f;
        DOTween.Sequence()
            .AppendInterval(hTime)
            .OnComplete(Show);

        void Show()
        {
            activeTasks.Add(mainTask);
            MessageManager.instance.ShowTask(text, mainTask.eventNum);
        }
    }
    public void DoneTask(MainTask mainTask, bool wait, bool doEndingEventFunction = true)
    {
        //Debug.Log("done task " + mainTask.eventNum);

        // Remove from list
        for (int i = 0; i < activeTasks.Count; i++) { if (activeTasks[i].eventNum == mainTask.eventNum) { activeTasks.RemoveAt(i); break; } }

        // hide mess
        MessageManager.instance.CloseTask(mainTask.eventNum);

        // if this task is open close it
        if (nowOpenTask != null && nowOpenTask.eventNum == mainTask.eventNum)
        {
            if (!wait) LeftPanel.instance.CloseNowOpenPanel();
            else
            {
                isHidingPanel = true;
                DOTween.Sequence()
                .SetUpdate(true)
                .AppendInterval(hidingTime)
                .OnComplete(HidePanel)
                ;
            }
        }

        if (doEndingEventFunction) { GameEventControler.EndEvent(mainTask.eventNum, mainTask.callNextTask); }
        else { GameEventControler.activeGameEvents.Remove(mainTask.eventNum); GameEventControler.complateGameEvent.Add(mainTask.eventNum); }

        if (GameEventControler.activeGameEvents.Count == 0) { GameEventControler.CallNextAvailableEvent(); }

        SpaceBaseMainSc.instance.SaveGeneralData();
        SpaceBaseMainSc.instance.SaveColony();

        void HidePanel()
        {
            if (nowOpenTask != null && nowOpenTask.eventNum == mainTask.eventNum) LeftPanel.instance.CloseNowOpenPanel();
            isHidingPanel = false;
        }
    }
    public void EndTaskFromEnum(GameEventControler.GameEvent gameEvent, bool doEndingEventFunction = true)
    {
        foreach (MainTask mainTask in activeTasks)
        {
            if (mainTask.eventNum == gameEvent)
            {
                DoneTask(mainTask, false, doEndingEventFunction);
                return;
            }
        }
    }
    public void UpdateTextOfOpenTask()
    {
        if (nowOpenTask == null) return;

        subTasksText.text = GetSubTasksText(nowOpenTask.subTasks);
    }

    public void ClickCloseTaskPanel() => LeftPanel.instance.CloseNowOpenPanel();
    public void SetTaskPanelAsClose() => nowOpenTask = null;

    private void CheckTasks()
    {
        int length = activeTasks.Count;
        int remTask = 0;
        for (int i = 0; i < length; i++)
        {
            MainTask mainTask = activeTasks[i - remTask];

            bool isDone = true;

            if (mainTask.subTasks.Count == 0) { isDone = false; }
            else
            {
                foreach (SubTask subTask in mainTask.subTasks)
                {
                    if (subTask.done) { continue; }
                    if (GetCheckedValueOf(subTask) >= subTask.targetValue) {subTask.done = true; }
                    else { isDone = false; }
                }
            }

            if (isDone) { DoneTask(mainTask, true); remTask++; }
        }
    }
    private int GetCheckedValueOf(SubTask subTask)
    {
        int index;
        switch (subTask.taskType)
        {
            case SubTaskTypes.HoldItemsInColony:
                if (subTask.subcategory >= 0 && subTask.subcategory < actNumberOfItems.Length) return actNumberOfItems[subTask.subcategory];
                return -1;
            case SubTaskTypes.ProduceItems:
                if (subTask.subcategory >= 0 && subTask.subcategory < totalNumberOfProducedItems.Length) return totalNumberOfProducedItems[subTask.subcategory];
                return -1;
            case SubTaskTypes.HoldItemsInObjects:
                if (DronControler.instance == null) return -1;
                Obj obj = (Obj)(subTask.subcategory % 1000000);
                Res res = (Res)(subTask.subcategory / 1000000);
                return DronControler.instance.GetNumberOfResInObject(obj, res);

            case SubTaskTypes.HaveStructures:
                index = GetIndexOfBuilding((Obj)subTask.subcategory);
                if (index == -1) return 0;
                return actNumberOfBuildings[index];
            case SubTaskTypes.BuildStructures:
                index = GetIndexOfBuilding((Obj)subTask.subcategory);
                if (index == -1) return 0;
                return actNumberOfBuildings[index];

            case SubTaskTypes.UnlockColony:
                if (SpaceBaseMainSc.instance.unlockedColonies.Contains((ColonyNames)subTask.subcategory)) return 1;
                return 0;
            case SubTaskTypes.HoldItemsInOrbitalStation:
                if (subTask.subcategory >= 0 && subTask.subcategory < actNumberOfItems.Length) return SpaceBaseMainSc.instance.GetResQuaOfStorage((Res)subTask.subcategory);
                return -1;
            case SubTaskTypes.LaunchedItems:
                if (subTask.subcategory >= 0 && subTask.subcategory < numberOfLaunchedItems.Length) return numberOfLaunchedItems[subTask.subcategory];
                return -1;
            case SubTaskTypes.HaveTechnology:
                if (SpaceBaseMainSc.instance.IsTechnologyDiscovered((Technologies)subTask.subcategory)) return 1;
                return 0;


            case SubTaskTypes.KillEnemyUnits:
                break;
            case SubTaskTypes.DestroyEnemyBase:
                break;
            case SubTaskTypes.SurviveWavesOfEnemies:
                break;
        }
        return -1;
    }
    private int GetIndexOfBuilding(Obj obj)
    {
        for (int i = 0; i < objectsTypes.Count; i++)
        {
            if (objectsTypes[i] == obj) return i;
        }
        return -1;
    }
    private string GetSubTasksText(List<SubTask> subTasks)
    {
        string text = "";

        foreach (SubTask task in subTasks)
        {
            if (text != "") { text += "\n\n"; }

            string sts;
            if (task.message == "") { sts = DisplayedText(task); }
            else { sts = task.message; }

            if (task.done)
            {
                sts = "<color=#5A5A5A>" + sts + "</color>";
            }
            else
            {
                string numtext = GetRealNum(task);
                if (numtext != "") { sts += " " + numtext; }
            }

            text += sts;
        }
        return text;

        string GetRealNum(SubTask task)
        {
            switch (task.taskType)
            {
                case SubTaskTypes.HoldItemsInColony:        return string.Format("({0}/{1})", GetValue(task), task.targetValue);
                case SubTaskTypes.ProduceItems:             return string.Format("({0}/{1})", GetValue(task), task.targetValue);

                case SubTaskTypes.HaveStructures:           return string.Format("({0}/{1})", GetValue(task), task.targetValue);
                case SubTaskTypes.BuildStructures:          return string.Format("({0}/{1})", GetValue(task), task.targetValue);

                case SubTaskTypes.HoldItemsInObjects:       return string.Format("({0}/{1})", GetValue(task), task.targetValue);

                case SubTaskTypes.KillEnemyUnits:           break;
                case SubTaskTypes.DestroyEnemyBase:         break;
                case SubTaskTypes.SurviveWavesOfEnemies:    break;

                case SubTaskTypes.LaunchedItems:            return string.Format("({0}/{1})", GetValue(task), task.targetValue);
                case SubTaskTypes.HoldItemsInOrbitalStation:return string.Format("({0}/{1})", GetValue(task), task.targetValue);
            }
            return "";
        }
        int GetValue(SubTask task)
        {
            int v = GetCheckedValueOf(task);
            if (v < 0) return 0;
            return v;
        }
        string DisplayedText(SubTask subTask)
        {
            switch (subTask.taskType)
            {
                case SubTaskTypes.HoldItemsInColony:
                    if (subTask.subcategory <= 0) return "Hold " + subTask.targetValue + " items in your storage";
                    return "Hold " + subTask.targetValue + " of " + Language.NameOfRes((Res)subTask.subcategory) + " in your storage";
                case SubTaskTypes.ProduceItems:
                    if (subTask.subcategory <= 0) return "Produce  " + subTask.targetValue + " items in total";
                    return "Produce  " + subTask.targetValue + " of " + Language.NameOfRes((Res)subTask.subcategory) + " in total";

                case SubTaskTypes.HaveStructures:
                    if (subTask.subcategory <= 0) return "Have " + subTask.targetValue + " structures built at the same time";
                    return "Have " + subTask.targetValue + " " + GuiControler.instance.DisplayedNameOfObj((Obj)subTask.subcategory) + " built at the same time";
                case SubTaskTypes.BuildStructures:
                    if (subTask.subcategory <= 0) return "Build " + subTask.targetValue + " structures in total";
                    return "Build " + subTask.targetValue + " " + GuiControler.instance.DisplayedNameOfObj((Obj)subTask.subcategory) + " in total";

                case SubTaskTypes.HoldItemsInObjects:
                    Obj obj = (Obj)(subTask.subcategory % 1000000);
                    Res res = (Res)(subTask.subcategory / 1000000);
                    return "Hold " + subTask.targetValue + " of " + Language.NameOfRes(res) + " in " + GuiControler.instance.DisplayedNameOfObj(obj) + " in total";

                case SubTaskTypes.UnlockColony: return "Colonize " + (ColonyNames)subTask.subcategory;
                case SubTaskTypes.LaunchedItems:
                    if (subTask.subcategory <= 0) return "Launch " + subTask.targetValue + " items to orbital statiom storages";
                    return "Launch " + subTask.targetValue + " of " + Language.NameOfRes((Res)subTask.subcategory) + " to orbital statiom storages";
                case SubTaskTypes.HoldItemsInOrbitalStation:
                    if (subTask.subcategory <= 0) return "Hold " + subTask.targetValue + " items in orbital statiom storages";
                    return "Hold " + subTask.targetValue + " of " + Language.NameOfRes((Res)subTask.subcategory) + " in orbital statiom storages";
                case SubTaskTypes.HaveTechnology: return "Have " + Language.NameOfTech((Technologies)subTask.subcategory) + " technology";

                case SubTaskTypes.KillEnemyUnits: return "Kill " + subTask.targetValue + " enemy units";
                case SubTaskTypes.DestroyEnemyBase: return "Destroy " + subTask.targetValue + " enemy bases";
                case SubTaskTypes.SurviveWavesOfEnemies: return "Survive " + subTask.targetValue + " waves of enemies";
            }
            string rText = "<" + subTask.taskType + ">" + "subcategory:" + subTask.subcategory;
            return rText;
        }
    }
    public void ResetAllTask()
    {
        SetDefaultStats();
        activeTasks = new List<MainTask>();
    }

    //save and load
    public int[][] GetStats()
    {
        int length = 17;

        int[][] tab = new int[length][];

        for (int i = 0; i < length; i++) { tab[i] = new int[0]; }

        tab[(int)SubTaskTypes.HoldItemsInColony] = actNumberOfItems;
        tab[(int)SubTaskTypes.ProduceItems] = totalNumberOfProducedItems;

        int[] objectsTypesTab = new int[objectsTypes.Count];
        for (int i = 0; i < objectsTypesTab.Length; i++) { objectsTypesTab[i] = (int)objectsTypes[i]; }
        tab[(int)SubTaskTypes.ObjectTypes] = objectsTypesTab;
        tab[(int)SubTaskTypes.HaveStructures] = actNumberOfBuildings.ToArray();
        tab[(int)SubTaskTypes.BuildStructures] = totalNumberOfBuildings.ToArray();

        tab[(int)SubTaskTypes.LaunchedItems] = numberOfLaunchedItems;

        return tab;
    }
    public void SetStats(int[][] stats)
    {
        if (stats == null) { Debug.Log("ERROR! saved stats equals null!"); return; }

        int[] singleStat;

        singleStat = GetStatArray(SubTaskTypes.HoldItemsInColony);
        if (singleStat != null) { for (int i = 0; i < singleStat.Length; i++) { actNumberOfItems[i] = singleStat[i]; } }
        singleStat = GetStatArray(SubTaskTypes.ProduceItems);
        if (singleStat != null) { for (int i = 0; i < singleStat.Length; i++) { totalNumberOfProducedItems[i] = singleStat[i]; } }

        singleStat = GetStatArray(SubTaskTypes.ObjectTypes);
        if (singleStat != null) { for (int i = 0; i < singleStat.Length; i++) { objectsTypes.Add((Obj)singleStat[i]); } }
        singleStat = GetStatArray(SubTaskTypes.HaveStructures);
        if (singleStat != null) { for (int i = 0; i < singleStat.Length; i++) { actNumberOfBuildings.Add(singleStat[i]); } }
        singleStat = GetStatArray(SubTaskTypes.BuildStructures);
        if (singleStat != null) { for (int i = 0; i < singleStat.Length; i++) { totalNumberOfBuildings.Add(singleStat[i]); } }

        singleStat = GetStatArray(SubTaskTypes.LaunchedItems);
        if (singleStat != null) { for (int i = 0; i < singleStat.Length; i++) { numberOfLaunchedItems[i] = singleStat[i]; } }

        int[] GetStatArray(SubTaskTypes taskTypes)
        {
            int index = (int)taskTypes;
            if (stats.Length > index) return stats[index];

            Debug.Log("Old version of stats dont contains: " + taskTypes);
            return null;
        }
    }

    //act stats
    public void SetDefaultStats()
    {
        int ResLenght = Enum.GetNames(typeof(Res)).Length;
        actNumberOfItems = new int[ResLenght];
        totalNumberOfProducedItems = new int[ResLenght];

        objectsTypes = new List<Obj>();
        actNumberOfBuildings = new List<int>();
        totalNumberOfBuildings = new List<int>();

        numberOfLaunchedItems = new int[ResLenght];
    }
    public void ActBuilding(Obj obj, int qua)
    {
        if (WorldMenager.instance.loadingWorld) return;

        int index = GetIndexOfBuilding(obj);
        if (index == -1)
        {
            objectsTypes.Add(obj);
            actNumberOfBuildings.Add(qua);
            if (qua > 0) { totalNumberOfBuildings.Add(qua); } else { totalNumberOfBuildings.Add(0); }
        }
        else
        {
            actNumberOfBuildings[index] += qua;
            if (qua > 0) { totalNumberOfBuildings[index] += qua; }
        }
    }
    public void ActItem(Res res, int qua, bool isNew)
    {
        if (WorldMenager.instance.loadingWorld) return;

        int index = (int)res;
        actNumberOfItems[index] += qua;
        if(isNew && qua > 0) { totalNumberOfProducedItems[index] += qua; }
    }
    public void AddNewItem(Res res, int qua)
    {
        int index = (int)res;
        if (qua > 0) { totalNumberOfProducedItems[index] += qua; }
    }
    public void AddLaunchedItem(Res res, int qua)
    {
        int index = (int)res;
        if (qua > 0) { numberOfLaunchedItems[index] += qua; }
    }
}