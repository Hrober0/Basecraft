﻿using System.Collections.Generic;
using UnityEngine;
using System;

public static class GameEventControler
{
    public enum GameEvent
    {
        None=0,
        P1_1=1,  P1_2, P1_3, P1_4, P1_5, P1_6, P1_7, P1_8, P1_9, P1_10, P1_11, P1_12, P1_13, P1_3_SkipTotorial=18,
        P2_1=21, P2_2, P2_3, P2_4, P2_5, P2_6,
        P3_1=41, P3_2, P3_3, P3_4, P3_5, P3_6, P3_7, P3_8, P3_9, P3_10, P3_11,
        P4_1=61, P4_2, P4_3, P4_4, P4_5, P4_6,

        NoMoreTask=81,

        T1=1000001, T2, T3, T4, T5, T6, T7, T8
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
            case GameEvent.T4: newTask.InitDef(gameEvent, false).SetDefConfirmButton(); break;
            case GameEvent.T5: newTask.InitDef(gameEvent, false).SetDefConfirmButton(); break;
            case GameEvent.T6: newTask.InitDef(gameEvent, false).SetDefConfirmButton(); break;
            case GameEvent.T7: newTask.InitDef(gameEvent, false).SetDefConfirmButton(); break;
            case GameEvent.T8: newTask.InitDef(gameEvent, false).SetDefConfirmButton(); break;

            // P1
            case GameEvent.P1_1: newTask.InitDef(gameEvent).SetDefConfirmButton(); break;
            case GameEvent.P1_2: newTask.InitDef(gameEvent).AddSubTask_UnlockColony(ColonyNames.Valley, true); break;
            case GameEvent.P1_3: newTask.InitDef(gameEvent, false).SetDefConfirmButton().SetDefCallEventButton(GameEvent.P1_3_SkipTotorial); break;
            case GameEvent.P1_3_SkipTotorial: EndEvent(GameEvent.P1_3_SkipTotorial, false); break;
            case GameEvent.P1_4: newTask.InitDef(gameEvent).AddSubTask_HaveStructures(Obj.BuildingUnderConstruction, 1, true); break;
            case GameEvent.P1_5: newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.StoneOre, 10, true).AddSubTask_HaveStructures(Obj.Smelter, 1, true); break;
            case GameEvent.P1_6: newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.CopperOreCtm, 10, true); break;
            case GameEvent.P1_7: newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.Wood, 5, true); break;
            case GameEvent.P1_8: newTask.InitDef(gameEvent).AddSubTask_HoldItemsInObjects(Obj.Smelter, Res.CopperOreCtm, 10, true); break;
            case GameEvent.P1_9: newTask.InitDef(gameEvent).AddSubTask_HoldItemsInObjects(Obj.Smelter, Res.Wood, 5, true); break;
            case GameEvent.P1_10: newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.CopperPlate, 3, true); break;
            case GameEvent.P1_11: newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.StoneOre, 20, true); break;
            case GameEvent.P1_12: newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.StoneBrick, 5, true); break;
            case GameEvent.P1_13: newTask.InitDef(gameEvent, false).AddSubTask_HaveStructures(Obj.BasicCrafter, 1, true);
                SpaceBaseMainSc.instance.UpdateTechs(); break;

            // P2
            case GameEvent.P2_1: newTask.InitDef(gameEvent).AddSubTask_HoldItemsInObjects(Obj.BasicCrafter, Res.Wood, 5, true); break;
            case GameEvent.P2_2: newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.Planks, 2, true); break;
            case GameEvent.P2_3: newTask.InitDef(gameEvent).AddSubTask_HaveStructures(Obj.Warehouse1, 1, true);
                SpaceBaseMainSc.instance.UpdateTechs(); break;
            case GameEvent.P2_4: newTask.InitDef(gameEvent).AddSubTask_HaveStructures(Obj.Connection1, 1, true); break;
            case GameEvent.P2_5: newTask.InitDef(gameEvent).AddSubTask_HoldItemsInObjects(Obj.Warehouse1, Res.Wood, 10, true); break;
            case GameEvent.P2_6: newTask.InitDef(gameEvent, false).AddSubTask_HaveStructures(Obj.Sapling, 5, true); break;

            // P3
            case GameEvent.P3_1: newTask.InitDef(gameEvent).AddSubTask_HoldItemsInObjects(Obj.BasicCrafter, Res.CopperPlate, 1, true).AddSubTask_ProduceItems(Res.CopperCable, 4, true); break;
            case GameEvent.P3_2: newTask.InitDef(gameEvent, false).AddSubTask_HaveStructures(Obj.Launchpad, 1, true);
                SpaceBaseMainSc.instance.UpdateTechs(); break;
            case GameEvent.P3_3: newTask.InitDef(gameEvent, false).SetDefConfirmButton(); break;
            case GameEvent.P3_4: newTask.InitDef(gameEvent).AddSubTask_HoldItemsInOS(Res.IronOre, 10, true); break;
            case GameEvent.P3_5: newTask.InitDef(gameEvent, false).AddSubTask_HoldItemsInOS(Res.CopperPlate, 5, true).AddSubTask_HaveTechnology(Technologies.IronPlate, true); break;
            case GameEvent.P3_6: newTask.InitDef(gameEvent).AddSubTask_HaveTechnology(Technologies.Planter, true); break;
            case GameEvent.P3_7: newTask.InitDef(gameEvent).AddSubTask_HaveStructures(Obj.Planter, 1, true); break;
            case GameEvent.P3_8: newTask.InitDef(gameEvent).AddSubTask_HaveStructures(Obj.WindTurbine1, 1, true); break;
            case GameEvent.P3_9: newTask.InitDef(gameEvent).AddSubTask_HoldItemsInOS(Res.IronPlate, 8, true).AddSubTask_HoldItemsInOS(Res.CopperCable, 12, true).AddSubTask_HaveTechnology(Technologies.IronGear, true); break;
            case GameEvent.P3_10: newTask.InitDef(gameEvent).AddSubTask_HaveTechnology(Technologies.WindTurbine1, true); break;
            case GameEvent.P3_11: newTask.InitDef(gameEvent, false).AddSubTask_HaveStructures(Obj.WindTurbine1, 1, true); break;

            // P4
            case GameEvent.P4_1: newTask.InitDef(gameEvent).AddSubTask_HaveTechnology(Technologies.Quarry, true); break;
            case GameEvent.P4_2: newTask.InitDef(gameEvent).AddSubTask_HaveStructures(Obj.Quarry, 1, true); break;
            case GameEvent.P4_3: newTask.InitDef(gameEvent).AddSubTask_HoldItemsInOS(Res.CopperPlate, 10, true).AddSubTask_HaveTechnology(Technologies.WoodenCircuit, true); break;
            case GameEvent.P4_4: newTask.InitDef(gameEvent).AddSubTask_ProduceItems(Res.WoodenCircuit, 10, true); break;
            case GameEvent.P4_5: newTask.InitDef(gameEvent).AddSubTask_HaveTechnology(Technologies.Woodcuter, true); break;
            case GameEvent.P4_6: newTask.InitDef(gameEvent, false).AddSubTask_HaveStructures(Obj.Woodcuter, 1, true); break;

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

        if (endEvenItWasEnded == false && complateGameEvent.Contains(gameEvent)) return;

        if (complateGameEvent.Contains(gameEvent)==false) complateGameEvent.Add(gameEvent);

        if (callNextTask)
        {
            int eventNumber = (int)gameEvent + 1;
            if (Enum.IsDefined(typeof(GameEvent), eventNumber) == false)
                Debug.Log("Error! Cant call next task, because event number " + eventNumber + " don't exist!");
            else
                StartEvent((GameEvent)eventNumber);
        }

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
            case GameEvent.P1_7: StartEvent(GameEvent.T1); break;
            case GameEvent.P1_11: StartEvent(GameEvent.T2); break;
            case GameEvent.P1_12: StartEvent(GameEvent.T3); break;
            case GameEvent.P1_13: StartEvent(GameEvent.P2_1); break;

            // P2
            case GameEvent.P2_2: StartEvent(GameEvent.P3_1); break;
            case GameEvent.P2_3: StartEvent(GameEvent.T4); break;
            case GameEvent.T4: StartEvent(GameEvent.T5); break;
            case GameEvent.P2_4: StartEvent(GameEvent.T6); break;
            case GameEvent.T6: StartEvent(GameEvent.T7); break;

            // P3
            case GameEvent.P3_2: StartEvent(GameEvent.P3_3); StartEvent(GameEvent.P3_4); break;
            case GameEvent.P3_5: StartEvent(GameEvent.P3_6); StartEvent(GameEvent.P3_9); break;
            case GameEvent.P3_11: StartEvent(GameEvent.P4_1); break;

            // P4
            case GameEvent.P4_4: StartEvent(GameEvent.T8); break;
            case GameEvent.P4_6: StartEvent(GameEvent.NoMoreTask); break;
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
