using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour
{
    public float updateWhiteListDelay = 1f;
    private float avaTimeToUpdateWhiteList = 0f;

    public int bufferSize = 0;

    private PlatformBehavior PlatformB;

    private List<PlatformBehavior> connectedPlatform = new List<PlatformBehavior>();
    private int[] resSpace;
    int numberOfRes;

    void Awake()
    {
        PlatformB = gameObject.GetComponent<PlatformBehavior>();

        PlatformB.usingGuiType = PlatfotmGUIType.None;
        PlatformB.itemSendingType = PlatformItemSendingType.Procesing;

        PlatformB.canDronesGetRes = false;

        PlatformB.SetMaxEmptySpace(200);

        numberOfRes = PlatformB.itemOnPlatform.Length;
    }

    void Update()
    {
        float worldTime = WorldMenager.instance.worldTime;
        if(avaTimeToUpdateWhiteList <= worldTime)
        {
            avaTimeToUpdateWhiteList = worldTime + updateWhiteListDelay;
            UpdateWhiteList();
        }
    }

    private void UpdateWhiteList()
    {
        connectedPlatform = new List<PlatformBehavior>();
        foreach (RoadBehavior connection in PlatformB.roadListOut)
        {
            if (connection == null || connection.sendOff) { continue; }
            PlatformBehavior pB = connection.EPBSc;
            if (pB == null) { continue; }
            connectedPlatform.Add(pB);
        }

        resSpace = new int[numberOfRes];

        foreach (PlatformBehavior pB in connectedPlatform)
        {
            if (pB.itemOnPlatform[0].qua <= 0) { continue; }

            int freeSpace = pB.itemOnPlatform[0].qua;

            for (int i = 1; i < numberOfRes; i++)
            {
                PlatformBehavior.ROP item = pB.itemOnPlatform[i];
                if (pB.itemOnPlatform[i].canIn == false) { continue; }
                if (pB.itemSendingType == PlatformItemSendingType.Storage)
                {
                    int delta = item.maxQua - item.qua;
                    if (delta > pB.itemOnPlatform[0].qua) { delta = pB.itemOnPlatform[0].qua; }
                    if (delta > 0) { resSpace[i] += delta; }
                }
                else
                {
                    int delta = item.maxQua - item.qua;
                    if (delta > 0) { resSpace[i] += delta; }
                }
            }
        }

        for (int i = 0; i < numberOfRes; i++)
        {
            PlatformBehavior.ROP item = PlatformB.itemOnPlatform[i];
            if (resSpace[i] != 0)
            {
                item.maxQua = resSpace[i] + bufferSize;
            }
            else
            {
                item.maxQua = 0;
            }
        }
    }
}
