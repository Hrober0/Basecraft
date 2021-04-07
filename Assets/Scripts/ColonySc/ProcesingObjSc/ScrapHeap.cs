using UnityEngine;

public class ScrapHeap : MonoBehaviour
{
    private PlatformBehavior PlatformB;
    public float removingFreq = 5f;
    private int ai = 0;

    void Awake()
    {
        PlatformB = gameObject.GetComponent<PlatformBehavior>();
        PlatformB.canBeConnectedOut = false;
    }

    void Start()
    {
        InvokeRepeating("TryDeleteItem", 1f, 1/removingFreq);
    }

    void TryDeleteItem()
    {
        if (PlatformB.avalibleRes.Count == 0) { return; }
        if(ai >= PlatformB.avalibleRes.Count) { ai = 0; }
        PlatformB.AddItem(PlatformB.avalibleRes[ai], -1);
        ai++;
    }
}
