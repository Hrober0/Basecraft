using UnityEngine;

public class MainMenuCameraControler : MonoBehaviour
{
    public static MainMenuCameraControler instance;
    void Awake()
    {
        if (instance != null) { Debug.Log("more the one CameraControler on scen"); return; }
        instance = this;

        kamera = GetComponent<Camera>();
    }

    private Camera kamera;

    private Vector3 minBorder;
    private Vector3 maxBorder;
    private Vector2Int mapSize;
    private bool wasSet = false;
    private Vector2 target;

    public float speed = 10f;

    public void SetCamera(Vector2Int _mapSize)
    {
        mapSize = _mapSize;
        SetBorders();
        kamera.transform.position = minBorder;
        target = maxBorder;
        wasSet = true;
    }
    public Vector2 GetCameraSize()
    {
        float halfCameraHeight = kamera.orthographicSize;
        float halfCameraWidth = halfCameraHeight * Screen.width / Screen.height;

        return new Vector2(halfCameraWidth, halfCameraHeight) * 2f;
    }

    void Update()
    {
        if (!wasSet) { return; }

        Vector2 dir = target - (Vector2)transform.position;
        float disThisFrame = speed * Time.deltaTime;
        if (dir.magnitude <= disThisFrame)
        {
            if (target == (Vector2)maxBorder) { target = minBorder; }
            else { target = maxBorder; }
            return;
        }
        transform.Translate(dir.normalized * disThisFrame, Space.World);
    }

    private void SetBorders()
    {
        float halfCameraHeight = kamera.orthographicSize;
        float halfCameraWidth = halfCameraHeight * Screen.width / Screen.height;

        minBorder = new Vector3(halfCameraWidth, halfCameraHeight, -10f);
        Vector2 t = mapSize * 10 - new Vector2(minBorder.x + 10f, minBorder.y + 10f);
        maxBorder = new Vector3(t.x, t.y, -10f);

        if (maxBorder.x < minBorder.x) { float average = (maxBorder.x + minBorder.x) / 2.0f; minBorder.x = average; maxBorder.x = average; }
        if (maxBorder.y < minBorder.y) { float average = (maxBorder.y + minBorder.y) / 2.0f; minBorder.y = average; maxBorder.y = average; }
    }
}
