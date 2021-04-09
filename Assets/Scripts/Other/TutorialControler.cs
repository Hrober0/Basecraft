using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialControler : MonoBehaviour
{
    [SerializeField] private Transform panelTrans = null;
    [SerializeField] private Text titleText = null;
    [SerializeField] private Text mainText = null;
    [SerializeField] private Text pageText = null;
    [SerializeField] private Button nextNutton = null;
    [SerializeField] private Button prevNutton = null;

    private int selectedPage = 1;
    private readonly int numOfPage = 7;

    private void Start()
    {
        SetPage(1);
    }
    private void OnEnable()
    {
        panelTrans.DOKill();
        panelTrans.localScale = Vector2.zero;
        panelTrans.DOScale(Vector2.one, 0.4f).SetEase(Ease.OutExpo).SetUpdate(true);
        panelTrans.GetComponent<CanvasGroup>().DOFade(1f, 0.4f).SetEase(Ease.OutExpo).SetUpdate(true);

        SetPage(selectedPage);
    }

    public void Close()
    {
        panelTrans.DOScale(Vector2.zero, 0.4f).SetEase(Ease.OutExpo).SetUpdate(true);
        panelTrans.GetComponent<CanvasGroup>().DOFade(0f, 0.4f).SetEase(Ease.OutExpo).SetUpdate(true).OnComplete(() => { gameObject.SetActive(false); });

        LeftPanel.instance.CloseNowOpenPanel();
        if (GuiControler.instance != null) GuiControler.instance.CloseNowOpenGui();
    }

    public void NextPage()
    {
        if (selectedPage >= numOfPage) { return; }
        selectedPage++;
        SetPage(selectedPage);
    }
    public void Prevpage()
    {
        if (selectedPage <= 1) { return; }
        selectedPage--;
        SetPage(selectedPage);
    }

    public void SetPage(int page)
    {
        selectedPage = page;

        nextNutton.interactable = true;
        prevNutton.interactable = true;
        if (selectedPage <= 1) { prevNutton.interactable = false; }
        else if (selectedPage >= numOfPage) { nextNutton.interactable = false; }

        pageText.text = string.Format("{0}/{1}", selectedPage, numOfPage);

        string tileT = Language.GetText("Tutorial." + page + "_h");
        if (tileT == "null") tileT = "Tutorial page " + page;
        titleText.text = tileT;

        string mainT = Language.GetText("Tutorial." + page + "_m");
        if (mainT == "null") mainT = "Missing tutorial text from page " + page;
        mainText.text = mainT;

        LeftPanel.instance.CloseNowOpenPanel();
        if (GuiControler.instance == null)
        {
            nextNutton.interactable = false;
            prevNutton.interactable = false;
            pageText.text = "If u want to see more, you must be in the colony";
            selectedPage = 1;
            return;
        }
        GuiControler.instance.CloseNowOpenGui();

        switch (page)
        {
            case 1:
                // setings and open
                SetPos(0f, 60f);
                LeftPanel.instance.OpenSettingsPanel();
                break;
            case 2:
                // drone station
                SetPos(-400f, 60f);
                Vector2Int mPos = WorldMenager.instance.mapSize / 2;
                Vector2Int dSPos = WorldMenager.instance.FindTheNearestObject(Obj.DroneStation, mPos.x, mPos.y, mPos.x);
                if (dSPos.x < 0) break;
                int x = dSPos.x;
                int y = dSPos.y;
                Transform dSTrans = WorldMenager.instance.GetTransforOfObj(x, y);
                PlatformBehavior dSPBSc = dSTrans.GetComponent<PlatformBehavior>();
                GuiControler.instance.ShowPlatformOpction(Obj.DroneStation, WorldMenager.instance.GetTerrainTile(x, y), x, y, dSPBSc);
                Vector2 pos = dSPos * 10 - Vector2.right * 20;
                CameraControler.instance.SetCameraPos(pos);
                break;
            case 3:
                // actions
                SetPos(50f, -60f);
                GuiControler.instance.ShowActionPanel();
                break;
            case 4:
                // build
                SetPos(50f, -60f);
                GuiControler.instance.ShowBuildGui();
                break;
            case 5:
                // demolition
                SetPos(50f, -60f);
                GuiControler.instance.ShowRemovingPanel();
                break;
            case 6:
                // tech
                SetPos(-400f, -100f);
                LeftPanel.instance.OpenSciencePanel();
                break;
            case 7:
                // storage
                SetPos(-70f, -60f);
                LeftPanel.instance.OpenStoragePanel();
                break;
        }
    }

    private void SetPos(float x, float y)
    {
        panelTrans.GetComponent<RectTransform>().DOAnchorPos(new Vector2(x, y), 0.5f).SetUpdate(true);
    }
}
