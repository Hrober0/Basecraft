using System.Collections.Generic;
using UnityEngine;
using System;

public static class GameEventControler
{
    public enum GameEvent
    {
        None=0,
        P1_1=1,  P1_2, P1_3, P1_4, P1_5, P1_6, P1_7, P1_8, P1_9, P1_10, P1_11, P1_12, P1_13, P1_3_SkipTotorial=18,

        NoMoreTask=21,

        T1=1000001, T2, T3,
    }

    public static List<GameEvent> activeGameEvents = new List<GameEvent>();
    public static List<GameEvent> complateGameEvent = new List<GameEvent>();

    public static void StartEvent(GameEvent gameEvent, bool force=false)
    {
        if (gameEvent == GameEvent.None) return;
        if (activeGameEvents.Contains(gameEvent)) return;
        if (complateGameEvent.Contains(gameEvent) && !force) return;
        activeGameEvents.Add(gameEvent);
        TaskManager.MainTask newTask = new TaskManager.MainTask();
        switch (gameEvent)
        {
            // Tips
            case GameEvent.T1: newTask.InitDef(gameEvent, false).SetDefConfirmButton(); break;
            case GameEvent.T2: newTask.InitDef(gameEvent, false).SetDefConfirmButton(); break;
            case GameEvent.T3: newTask.InitDef(gameEvent, false).SetDefConfirmButton(); break;

            // P1
            case GameEvent.P1_1: newTask.InitDef(gameEvent).SetDefConfirmButton(); break;
            case GameEvent.P1_2: newTask.InitDef(gameEvent).AddSubTask_UnlockColony(ColonyNames.Valley, true); break;
            case GameEvent.P1_3: newTask.InitDef(gameEvent, false).SetDefConfirmButton().SetDefCallEventButton(GameEvent.P1_3_SkipTotorial); break;
            case GameEvent.P1_3_SkipTotorial: EndEvent(GameEvent.P1_3_SkipTotorial, false); break;
            case GameEvent.P1_4: newTask.InitDef(gameEvent).AddSubTask_HaveStructures(Obj.BuildingUnderConstruction, 1, true); break;
            case GameEvent.P1_5: newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.StoneOre, 10, true).AddSubTask_HaveStructures(Obj.Smelter, 1, true); break;
            case GameEvent.P1_6: newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.CopperOreCtm, 10, true); break;
            case GameEvent.P1_7: newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.Wood, 5, true); break;
            case GameEvent.P1_8:
                StartEvent(GameEvent.T1);
                newTask.InitDef(gameEvent).AddSubTask_HoldItemsInObjects(Obj.Smelter, Res.CopperOreCtm, 10, true);
                break;
            case GameEvent.P1_9: newTask.InitDef(gameEvent).AddSubTask_HoldItemsInObjects(Obj.Smelter, Res.Wood, 5, true); break;
            case GameEvent.P1_10: newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.CopperPlate, 3, true); break;
            case GameEvent.P1_11: newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.StoneOre, 20, true); break;
            case GameEvent.P1_12:
                StartEvent(GameEvent.T2);
                newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.StoneBrick, 5, true);
                break;
            case GameEvent.P1_13:
                StartEvent(GameEvent.T3);
                newTask.InitDef(gameEvent, false).AddSubTask_HaveStructures(Obj.BasicCrafter, 1, true);
                break;

            // No more tasks
            case GameEvent.NoMoreTask: newTask.InitDef(gameEvent, false); break;
        }
        Debug.Log("Start event " + gameEvent);
        if (newTask.wasInit) TaskManager.instance.StartTask(newTask);
    }
    public static void EndEvent(GameEvent gameEvent, bool callNextTask, bool endEvenItWasEnded=false)
    {
        if (gameEvent == GameEvent.None) return;
        if (activeGameEvents.Contains(gameEvent) == false) return;
        activeGameEvents.Remove(gameEvent);

        if (callNextTask)
        {
            int eventNumber = (int)gameEvent + 1;
            if (Enum.IsDefined(typeof(GameEvent), eventNumber) == false)
                Debug.Log("Error! Cant call next task, because event number " + eventNumber + " don't exist!");
            else
                StartEvent((GameEvent)eventNumber);
        }

        if (endEvenItWasEnded == false && complateGameEvent.Contains(gameEvent)) return;

        if (complateGameEvent.Contains(gameEvent)==false) complateGameEvent.Add(gameEvent);

        switch (gameEvent)
        {
            // P1
            case GameEvent.P1_1: SpaceBaseMainSc.instance.UnlockColony(ColonyNames.Valley); break;
            case GameEvent.P1_3:
                complateGameEvent.Add(GameEvent.P1_3_SkipTotorial);
                GameEventTrigger.instance.OpenTutorial();
                StartEvent(GameEvent.P1_4);
                break;
            case GameEvent.P1_3_SkipTotorial:
                TaskManager.instance.EndTaskFromEnum(GameEvent.P1_3, false);
                LeftPanel.instance.UpdateSettingsPanel();
                SpaceBaseMainSc.instance.skippedTotorial = true;
                SpaceBaseMainSc.instance.UpdateTechs();
                StartEvent(GameEvent.P1_4);
                break;
            case GameEvent.P1_13:
                StartEvent(GameEvent.NoMoreTask);
                break;
        }
    }

    public static void CallNextAvailableEvent()
    {
        Debug.Log("Looking next avaliable event");
        foreach (GameEvent gEvent in Enum.GetValues(typeof(GameEvent)))
        {
            if (gEvent == GameEvent.None) { continue; }

            if (complateGameEvent.Contains(gEvent) || activeGameEvents.Contains(gEvent)) { continue; }

            Debug.Log("Calling " + gEvent);
            StartEvent(gEvent);
            return;
        }
    }
}
