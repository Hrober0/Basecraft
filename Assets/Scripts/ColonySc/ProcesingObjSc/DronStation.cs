using System.Collections.Generic;
using UnityEngine;

public class DronStation : MonoBehaviour
{
    private PlatformBehavior PlatformB;
    public List<DronBehavior> availableDrons = new List<DronBehavior>();
    public int maxDronsQua = 12;
    public int operatingNetwork = 0;

    private Animator DSAnimator;
    //private bool canClose = false;
    private bool isOpen = false;

    void Awake()
    {
        maxDronsQua = 12;

        DSAnimator = GetComponent<Animator>();
        PlatformB = transform.GetComponent<PlatformBehavior>();

        PlatformB.range = DronControler.instance.DSRange;

        PlatformB.itemOnPlatform[(int)Res.Drone].canOut = false;

        PlatformB.usingGuiType = PlatfotmGUIType.DronStation;
        PlatformB.itemSendingType = PlatformItemSendingType.Storage;
        PlatformB.SetMaxEmptySpace(100);

        DronControler.instance.AddDS(this);
    }
    void Start()
    {
        InvokeRepeating("DoTask", 1f, WorldMenager.instance.frequencyOfChecking);
    }

    private void DoTask()
    {
        if(availableDrons.Count < maxDronsQua && PlatformB.itemOnPlatform[(int)Res.Drone].qua > 0)
        {
            int delta = maxDronsQua - availableDrons.Count;
            int n = PlatformB.itemOnPlatform[(int)Res.Drone].qua;
            if (n > delta) { n = delta; }
            for (int i = 0; i < n; i++)
            {
                PlatformB.itemOnPlatform[(int)Res.Drone].qua--;
                PlatformB.itemOnPlatform[0].qua++;
                DronControler.instance.SpownDron(transform, true);
            }
        }

        if (availableDrons.Count == 0) { return; }
        DronControler.instance.TryGetTask(availableDrons[0]);
    }

    
    public void AddDron(DronBehavior DBSc)
    {
        //if (availableDrons.Count >= maxDronsQua) { return; }
        availableDrons.Add(DBSc);
    }
    public void RemoveDron(DronBehavior DBSc)
    {
        if (availableDrons.Contains(DBSc) == false) return;
        availableDrons.Remove(DBSc);
    }

    public void TryOpen()
    {
        if (isOpen) { return; }

        DSAnimator.SetTrigger("Open");
    }

    public Vector2Int GetTabPos()
    {
        return PlatformB.GetTabPos();
    }
}
