using UnityEngine;
using UnityEngine.UI; // top of file




public class ObjectInspector : MonoBehaviour
{
    public Image crosshair;  // assign in Inspector

    [Header("General")]
    public Camera playerCamera;
    public LayerMask inspectLayer;
    public float inspectDistance = 1.5f;
    public float moveSpeed = 10f;

    [Header("Rotation")]
    public float rotateSpeed = 5f;

    [Header("Input")]
    public KeyCode exitKey = KeyCode.Escape;

    Transform _inspectAnchor;
    InspectableObject _current;
    bool _isInspecting;
    public static bool IsInspecting { get; private set; }  // <- add at top (inside class)



    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        // Create invisible anchor in front of camera
        _inspectAnchor = new GameObject("InspectAnchor").transform;
        _inspectAnchor.SetParent(playerCamera.transform);
        _inspectAnchor.localPosition = new Vector3(0f, 0f, inspectDistance);
        _inspectAnchor.localRotation = Quaternion.identity;
    }

    void Update()
    {
        if (!_isInspecting)
        {
            HandleInspectStart();
        }
        else
        {
            HandleInspectRotate();
            HandleInspectEnd();
        }
    }

    void HandleInspectStart()
    {
        // Press E to start inspecting whatever is under the crosshair
        bool ePressed =
#if ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame;
#else
        Input.GetKeyDown(KeyCode.E);
#endif

        if (!ePressed) return;

        // Ray from the center of the screen (crosshair)
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, inspectLayer))
        {
            var inspectable = hit.collider.GetComponentInParent<InspectableObject>();
            if (inspectable != null)
            {
                StartInspect(inspectable);
            }
        }
    }

    void StartInspect(InspectableObject obj)
    {
        _current = obj;
        _current.StoreOriginalTransform();
        _current.OnInspectStart();

        _current.transform.SetParent(_inspectAnchor);
        _isInspecting = true;
        IsInspecting = true;                            // <- tell others

        // unlock mouse so you can drag freely (optional)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (crosshair) crosshair.enabled = false;   // hide
    }

    void HandleInspectRotate()
    {
        if (_current == null) return;

        // Smoothly move the inspected object toward the anchor in front of the camera
        _current.transform.position = Vector3.Lerp(
            _current.transform.position,
            _inspectAnchor.position,
            Time.deltaTime * moveSpeed
        );

        // --- Read mouse drag depending on input backend ---
        bool dragging;
        float mouseX, mouseY;

#if ENABLE_INPUT_SYSTEM
        // New Input System: delta is in pixels per frame
        var mouse = UnityEngine.InputSystem.Mouse.current;
        dragging = mouse != null && mouse.leftButton.isPressed;
        Vector2 delta = (mouse != null) ? mouse.delta.ReadValue() : Vector2.zero;
        mouseX = delta.x;
        mouseY = delta.y;
#else
    // Old Input Manager
    dragging = Input.GetMouseButton(0);
    mouseX = Input.GetAxis("Mouse X");
    mouseY = Input.GetAxis("Mouse Y");
#endif

        // Rotate while dragging: yaw around camera up, pitch around camera right
        if (dragging)
        {
            _current.transform.Rotate(playerCamera.transform.up, -mouseX * rotateSpeed, Space.World);
            _current.transform.Rotate(playerCamera.transform.right, mouseY * rotateSpeed, Space.World);
        }
    }


    void HandleInspectEnd()
    {
        // Right click or key to exit
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(exitKey))
        {
            EndInspect();
        }
    }

    void EndInspect()
    {
        _current.transform.SetParent(_current.OriginalParent);
        _current.transform.position = _current.OriginalPosition;
        _current.transform.rotation = _current.OriginalRotation;

        _current.OnInspectEnd();
        _current = null;
        _isInspecting = false;
        IsInspecting = false;                           // <- done inspecting

        // re-lock for FPS look (optional; if your mouse-look handles this, skip)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshair) crosshair.enabled = true;    // show
    }
}
