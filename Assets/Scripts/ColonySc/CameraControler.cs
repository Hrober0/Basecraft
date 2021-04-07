using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CameraControler : MonoBehaviour
{
    public static CameraControler instance;
    void Awake()
    {
        if (instance != null) { Debug.Log("more the one CameraControler on scen"); return; }
        instance = this;

        mCamera = GetComponent<Camera>();
    }

    private Camera mCamera;
    [SerializeField] private Camera blurCamera = null;

    private float scrollSpeed = 25f;
    private float scale = 40f;      public float GetScale { get => scale; }
    private float CameraDefZ = -20f;
    private float minScale = 30f;
    private float maxScale = 200f;

    public bool usingMenu = false;
    public bool moving = false;

    private Vector2 minBorder;
    private Vector2 maxBorder;

    private Vector3 lastFramePosition;

    private void Start()
    {
        SetBorders();
        lastFramePosition = mCamera.ScreenToWorldPoint(Input.mousePosition);
        SetCameraSize(scale);
    }
    private void Update()
    {
        if (moving==false && EventSystem.current.IsPointerOverGameObject()) { usingMenu = true; }

        if (usingMenu)
        {
            if (Input.GetMouseButton(0) == false) { usingMenu = false; }
            return;
        }

        Move();
        Scroll();
    }

    private void Scroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            scale -= scroll * scrollSpeed;
            if (scale < minScale) { scale = minScale; }
            else if (scale > maxScale) { scale = maxScale; }
            mCamera.orthographicSize = scale;
            if (blurCamera != null) { blurCamera.orthographicSize = scale; }

            SetBorders();
            CheckMoveCamera();
        }
    }
    private void Move()
    {
        Vector3 currFramePosition = mCamera.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButton(0))
        {
            Vector3 diff = lastFramePosition - currFramePosition;
            mCamera.transform.Translate(diff);
            CheckMoveCamera();
            moving = true;
        }
        else { moving = false; }
        lastFramePosition = mCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    private void SetBorders()
    {
        float halfCameraHeight = mCamera.orthographicSize;
        float halfCameraWidth = halfCameraHeight * Screen.width / Screen.height;

        minBorder = new Vector2(halfCameraWidth, halfCameraHeight);
        maxBorder = WorldMenager.instance.mapSize * 10 - minBorder -new Vector2(10f, 10f);

        if (maxBorder.x < minBorder.x) { float average = (maxBorder.x + minBorder.x) / 2.0f; minBorder.x = average; maxBorder.x = average; }
        if (maxBorder.y < minBorder.y) { float average = (maxBorder.y + minBorder.y) / 2.0f; minBorder.y = average; maxBorder.y = average; }
    }
    private void CheckMoveCamera()
    {
        Vector2 myPos = new Vector2(transform.position.x, transform.position.y);

        myPos.x = Mathf.Clamp(myPos.x, minBorder.x, maxBorder.x);
        myPos.y = Mathf.Clamp(myPos.y, minBorder.y, maxBorder.y);
        mCamera.transform.position = new Vector3(myPos.x, myPos.y, CameraDefZ);
    }

    public void MoveCameraToPoint(Vector2 point)
    {
        mCamera.transform.position = new Vector3(point.x, point.y, CameraDefZ);
        SetBorders();
        CheckMoveCamera();
    }
    public void SetCameraPos(Vector2 pos)
    {
        Vector3 vector3 = pos;
        vector3.z = CameraDefZ;
        mCamera.transform.DOMove(vector3, 1f);
    }
    public void SetCameraSize(float _scale)
    {
        scale = _scale;
        if (scale < minScale) { scale = minScale; }
        else if (scale > maxScale) { scale = maxScale; }
        mCamera.orthographicSize = scale;
        if (blurCamera != null) { blurCamera.orthographicSize = scale; }

        SetBorders();
        CheckMoveCamera();
    }
}
