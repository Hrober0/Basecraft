using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpaceStationIMovement : MonoBehaviour
{
    [SerializeField] private Transform SpaceStationT = null;
    [SerializeField] private Transform PlanetT = null;
    private SpriteRenderer SpaceStationImage = null;
    [SerializeField] private float rotSpeed = 20f;
    [SerializeField] private float xSpread = 0f;
    [SerializeField] private float ySpread = 0f;
    [SerializeField] private float yOfset = 0f;
    private float timer = 4f;
    [SerializeField] private bool isOverPlanet = false;

    enum State { Show, Showing, Hide, Hiding }
    [SerializeField] State state = State.Show;

    void Start()
    {
        SpaceStationImage = SpaceStationT.GetComponent<SpriteRenderer>();
        timer = 4f;
    }

    void Update()
    {
        UpdateSpaceStation();
    }

    private void UpdateSpaceStation()
    {
        timer -= Time.unscaledDeltaTime * rotSpeed;
        float x = Mathf.Cos(timer) * xSpread;
        float z = Mathf.Sin(timer);
        float y = -z * ySpread + yOfset;

        //set siblinks
        bool isOver = z > 0;
        if (isOver != isOverPlanet)
        {
            isOverPlanet = isOver;
            if (isOverPlanet) { SpaceStationImage.sortingOrder = 4; }
            else { SpaceStationImage.sortingOrder = 1; }
        }

        if (state==State.Show && PlanetT.localScale.x > 1f) 
        {
            state = State.Hiding;
            SpaceStationImage.DOColor(Color.clear, 0.1f).SetUpdate(true);
        }
        else if (state == State.Hiding && PlanetT.localScale.x == 3f)
        {
            state = State.Hide;
        }
        else if (state == State.Hide && PlanetT.localScale.x < 1.5f)
        {
            state = State.Showing;
            SpaceStationImage.DOColor(Color.white, 0.2f).SetUpdate(true);
        }
        else if (state == State.Showing && PlanetT.localScale.x == 1f)
        {
            state = State.Show;
        }

        if (state == State.Hide) { return; }

        //set pos and scale
        float scale = (1 + z) * 0.5f;
        Vector3 pos = new Vector2(x, y);
        SpaceStationT.position = pos + PlanetT.position;
        SpaceStationT.transform.localScale = Vector2.one * scale;

        //faster behind
        float k = 6f;
        if (!isOver && x < k && x > -k) { timer -= 0.01f; }
    }
}
