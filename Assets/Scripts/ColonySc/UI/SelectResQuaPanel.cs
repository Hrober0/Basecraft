using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SelectResQuaPanel : MonoBehaviour
{
    [SerializeField] private GameObject setResQuaPanelPanel = null;
    [SerializeField] private GameObject selectResourceButton = null;
    [SerializeField] private Text numberText = null;
    [SerializeField] private Slider slider = null;
    [SerializeField] private Image image = null;
    [SerializeField] private Text resText = null;
    [SerializeField] private Text titleText = null;


    private Res PGSelectRes;
    public enum SelectResQuaMode { None, AddRes, SetMaxQua, RequestRes }
    private SelectResQuaMode selectResQuaMode = SelectResQuaMode.None;

    public static SelectResQuaPanel instance;
    private void Awake()
    {
        if (instance != null) return;
        instance = this;
    }

    public void Show(Res res, SelectResQuaMode selectResMode)
    {
        PlatformBehavior usePBSc = GuiControler.instance.usePBSc;
        if (usePBSc == null || selectResMode == SelectResQuaMode.None) { return; }

        SelectItemPanel.instance.Hide();

        RectTransform rt = setResQuaPanelPanel.GetComponent<RectTransform>();
        int index = GuiControler.instance.GetCountOfNowOpenPanels() - 2;
        float yPos = GuiControler.instance.GetUpPositionOfPanelIndexInNowOpenPanels(index);
        if (yPos < 0f) return;
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, yPos);


        setResQuaPanelPanel.SetActive(true);


        selectResQuaMode = selectResMode;
        PGSelectRes = res;

        selectResourceButton.SetActive(false);

        switch (selectResQuaMode)
        {
            case SelectResQuaMode.AddRes: SetToAddRes(); break;
            case SelectResQuaMode.SetMaxQua: SetToMaxQua(); break;
            case SelectResQuaMode.RequestRes: SetRequestQua(); break;
        }


        void SetToAddRes()
        {
            setResQuaPanelPanel.transform.Find("TitleText").GetComponent<Text>().text = "Set amount of resource";
            if (res == Res.None)
            {
                selectResourceButton.SetActive(true);
                numberText.text = "-";
                slider.interactable = false;
                slider.maxValue = 0;
                slider.value = 0;
            }
            else
            {
                int qua = usePBSc.itemOnPlatform[(int)PGSelectRes].qua;
                resText.text = Language.NameOfRes(PGSelectRes);
                image.sprite = ImageLibrary.instance.GetResImage(PGSelectRes);
                numberText.text = qua.ToString();
                slider.interactable = true;
                slider.maxValue = usePBSc.itemOnPlatform[0].qua + qua;
                slider.value = qua;
            }
        }
        void SetToMaxQua()
        {
            int qua = usePBSc.itemOnPlatform[(int)PGSelectRes].maxQua;
            titleText.text = "Set max amount of resource";
            resText.text = Language.NameOfRes(PGSelectRes);
            image.sprite = ImageLibrary.instance.GetResImage(PGSelectRes);
            numberText.text = qua.ToString();
            slider.interactable = true;
            slider.maxValue = usePBSc.MaxEmptySpace;
            slider.value = qua;
        }
        void SetRequestQua()
        {
            int qua = usePBSc.GetRequestItem(PGSelectRes).qua; if (qua < 0) { qua = 0; }
            titleText.text = "Set amount of requested resource";
            resText.text = Language.NameOfRes(PGSelectRes);
            image.sprite = ImageLibrary.instance.GetResImage(PGSelectRes);
            numberText.text = qua.ToString();
            slider.interactable = true;
            slider.maxValue = usePBSc.MaxEmptySpace;
            slider.value = qua;
        }
    }
    public void Hide()
    {
        setResQuaPanelPanel.SetActive(false);
    }

    public void UpdatePGSetResQuaValue(float value) => numberText.text = ((int)value).ToString();
    public void SetPGSetResQua(Res res) => Show(res, SelectResQuaMode.AddRes);
    public void ClickPGConfirmSetResQua()
    {
        PlatformBehavior usePBSc = GuiControler.instance.usePBSc;
        setResQuaPanelPanel.SetActive(false);
        if (usePBSc == null) { return; }
        if (PGSelectRes == Res.None) { return; }
        if (numberText.text == "-") { return; }
        if (int.TryParse(numberText.text, out int qua) == false) { return; }

        switch (selectResQuaMode)
        {
            case SelectResQuaMode.AddRes: int delta = qua - usePBSc.itemOnPlatform[(int)PGSelectRes].qua; if (delta != 0) { usePBSc.AddItem(PGSelectRes, delta); } break;
            case SelectResQuaMode.SetMaxQua: usePBSc.itemOnPlatform[(int)PGSelectRes].maxQua = qua; break;
            case SelectResQuaMode.RequestRes: usePBSc.SetRequestItem(PGSelectRes, qua); break;
        }
    }
    public void ClickPGChooseSetResQua()
    {
        PlatformBehavior usePBSc = GuiControler.instance.usePBSc;
        if (usePBSc == null) { return; }
        setResQuaPanelPanel.SetActive(true);
        SelectItemPanel.instance.Show(SelectItemPanel.ItemSelectMode.Additem);
        Hide();
    }
}
