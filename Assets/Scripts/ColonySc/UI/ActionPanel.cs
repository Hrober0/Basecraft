using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class ActionPanel : MonoBehaviour
{
    public Transform ActionPanelT;
    [SerializeField] private Transform ButtonGroup = null;
    [SerializeField] private GameObject ButtonBorder = null;
    [SerializeField] private GameObject Pointer = null;

    [SerializeField] private Sprite minePImage = null;
    [SerializeField] private Sprite plantPImage = null;
    [SerializeField] private Sprite cutPImage = null;
    [SerializeField] private Sprite repairPImage = null;

    [SerializeField] private Color normalPColor = Color.white;
    [SerializeField] private Color errorPColor = Color.red;

    public enum ActionOpction { None=0, Mine=1, MasiveMine=2, Plant=3, Cut=4, Repair=5 }
    public ActionOpction selectedOpction = ActionOpction.None;
    private Transform selectedButton = null;
    private float lastCameraScale = 0;
    private Image PImage;
    private bool isRed = false;

    void Start()
    {
        PImage = Pointer.GetComponent<Image>();
        Cancel();
    }

    void Update()
    {
        if (Pointer.activeSelf)
        {
            if (lastCameraScale != CameraControler.instance.GetScale)
            {
                lastCameraScale = CameraControler.instance.GetScale;
                float scale = 60f / lastCameraScale;
                Pointer.transform.localScale = new Vector2(scale, scale);
            }
            Vector2 screenPosition = Input.mousePosition;
            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
                worldPosition.x += 5f; worldPosition.x = (int)(worldPosition.x / 10) * 10;
                worldPosition.y += 5f; worldPosition.y = (int)(worldPosition.y / 10) * 10;
                screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            }
            Pointer.transform.position = screenPosition;
        }
    }

    public void Cancel()
    {
        ButtonBorder.SetActive(false);
        Pointer.SetActive(false);
        selectedButton = null;
        ClickMenager.instance.SetClickMode(ClickMenager.ClickMode.Normal);
        selectedOpction = ActionOpction.None;
    }
    public void ClickButon(int v)
    {
        string name = EventSystem.current.currentSelectedGameObject.name;
        Transform button = ButtonGroup.Find(name);

        if (button == selectedButton)
        {
            Cancel();
            return;
        }

        ActionOpction action = (ActionOpction)v;

        selectedButton = button;
        ButtonBorder.transform.SetParent(button, false);
        ButtonBorder.SetActive(true);
        Pointer.SetActive(true);
        ClickMenager.instance.SetClickMode(ClickMenager.ClickMode.OtherActions);
        selectedOpction = action;
        switch (action)
        {
            case ActionOpction.MasiveMine:
            case ActionOpction.Mine:    PImage.sprite = minePImage;     break;
            case ActionOpction.Plant:   PImage.sprite = plantPImage;    break;
            case ActionOpction.Cut:     PImage.sprite = cutPImage;      break;
            case ActionOpction.Repair:  PImage.sprite = repairPImage;   break;
        }
    }
    public void ShowErrorOfPointer()
    {
        if (isRed) { return; }
        isRed = true;
        DOTween.Sequence()
            .Append(PImage.DOColor(errorPColor, 0.1f))
            .Append(PImage.DOColor(normalPColor, 0.1f))
            .OnComplete(() => { isRed = false; })
        ;
    }
}