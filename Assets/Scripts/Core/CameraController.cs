using UnityEngine;

/// <summary>
/// RTS Camera — WASD / arrow keys to pan, scroll wheel to zoom.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Pan")]
    public float panSpeed   = 10f;
    public float panBorder  = 20f;   // pixels from edge to trigger pan

    [Header("Zoom")]
    public float zoomSpeed  = 3f;
    public float minZoom    = 3f;
    public float maxZoom    = 12f;

    [Header("Bounds")]
    public float mapWidth   = 20f;
    public float mapHeight  = 14f;

    private Camera cam;

    void Awake() => cam = GetComponent<Camera>();

    void Update()
    {
        HandlePan();
        HandleZoom();
        ClampPosition();
    }

    void HandlePan()
    {
        Vector3 move = Vector3.zero;
        float speed = panSpeed * Time.unscaledDeltaTime;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)    || Input.mousePosition.y >= Screen.height - panBorder) move.y += speed;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)  || Input.mousePosition.y <= panBorder)                  move.y -= speed;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width  - panBorder) move.x += speed;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)  || Input.mousePosition.x <= panBorder)                  move.x -= speed;

        transform.position += move;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * zoomSpeed;
        cam.orthographicSize  = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }

    void ClampPosition()
    {
        float hw = mapWidth  / 2f;
        float hh = mapHeight / 2f;
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, -hw, hw);
        p.y = Mathf.Clamp(p.y, -hh, hh);
        p.z = -10f;
        transform.position = p;
    }
}
