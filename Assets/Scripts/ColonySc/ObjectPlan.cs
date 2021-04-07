using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
public class ObjectPlan : MonoBehaviour
{
    public Obj objName;
    public ObjectPlanType planType;
    public Vector2Int startRoadPoint, endRoadPoint;
    public List<ItemRAQ> needItems = new List<ItemRAQ>();
    public List<ItemRAQ> keptItems = new List<ItemRAQ>();

    [SerializeField] private Canvas canvas=null;
    [SerializeField] private Transform panel=null;
    [SerializeField] private GameObject image=null;
    [SerializeField] private GameObject krp = null;

    private void Start()
    {
        image.SetActive(false);
        krp.SetActive(false);
        canvas.transform.rotation = Quaternion.Euler(0, 0, -transform.rotation.z);

        UpdateItemsPanel();
    }

    private void UpdateItemsPanel()
    {
        if (keptItems == null) { keptItems = new List<ItemRAQ>(); }
        if (needItems == null) { needItems = new List<ItemRAQ>(); }

        if (keptItems.Count==0 && needItems.Count==0)
        {
            canvas.gameObject.SetActive(false);
            return;
        }

        for (int i = 2; i < panel.childCount; i++) { Destroy(panel.GetChild(i).gameObject); }
        canvas.gameObject.SetActive(true);

        int ic;
        if (planType == ObjectPlanType.Building)
        {
            ic = needItems.Count;
            if (ic > 8)
            {
                for (int i = 0; i < 3; i++) { CreateImage(needItems[i].res, needItems[i].qua); }
                CreateKrp();
                CreateKrp();
                for (int i = ic-1; i > ic-4; i--) { CreateImage(needItems[i].res, needItems[i].qua); }
            }
            else
            {
                foreach (ItemRAQ item in needItems) { CreateImage(item.res, item.qua); }
            }
        }
        else
        {
            ic = keptItems.Count;
            if (ic > 8)
            {
                for (int i = 0; i < 3; i++) { CreateImage(keptItems[i].res, keptItems[i].qua); }
                CreateKrp();
                CreateKrp();
                for (int i = ic-1; i > ic-4; i--) { CreateImage(keptItems[i].res, keptItems[i].qua); }
            }
            else
            {
                foreach (ItemRAQ item in keptItems) { CreateImage(item.res, item.qua); }
            }
        }

        if(ic > 8)  canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 100);
        else        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2((ic <= 4 ? ic * 50 : 200), (ic > 4 ? 100 : 50));

        void CreateImage(Res res, int qua)
        {
            GameObject newImage = Instantiate(image);
            newImage.transform.SetParent(panel);
            newImage.transform.localScale = new Vector3(1, 1, 1);
            newImage.GetComponent<Image>().sprite = ImageLibrary.instance.GetResImage(res);
            newImage.GetComponentInChildren<Text>().text = qua.ToString();
            newImage.SetActive(true);
        }
        void CreateKrp()
        {
            GameObject newImage = Instantiate(krp);
            newImage.transform.SetParent(panel);
            newImage.transform.localScale = new Vector3(1, 1, 1);
            newImage.SetActive(true);
        }
    }

    public void AddItem(Res res, int qua)
    {
        if (qua <= 0) { Debug.Log("Cant add negative number of items"); }

        if((keptItems == null || keptItems.Count == 0) && (needItems == null || needItems.Count == 0))
        {
            canvas.gameObject.SetActive(false);
            return;
        }

        for (int i = 0; i < needItems.Count; i++)
        {
            if (needItems[i].res == res)
            {
                needItems[i].qua -= qua;
                if (needItems[i].qua <= 0) { needItems.RemoveAt(i); }
                break;
            }
        }

        bool was = false;
        for (int i = 0; i < keptItems.Count; i++)
        {
            if (keptItems[i].res == res)
            {
                keptItems[i].qua += qua;
                was = true;
                break;
            }
        }
        if (was==false)
        {
            keptItems.Add(new ItemRAQ(res, qua));
        }
        UpdateItemsPanel();
    }
    public void TakeItem(Res res, int qua)
    {
        if (qua <= 0) { Debug.Log("Cant take negative number of items"); }
        for (int i = 0; i < keptItems.Count; i++)
        {
            if (keptItems[i].res == res)
            {
                keptItems[i].qua -= qua;
                if(keptItems[i].qua <= 0)
                {
                    keptItems.RemoveAt(i);
                }
                break;
            }
        }
        UpdateItemsPanel();
    }

    public Vector2Int GetTabPos() { return new Vector2Int((int)(transform.position.x / 10), (int)(transform.position.y / 10)); }
}